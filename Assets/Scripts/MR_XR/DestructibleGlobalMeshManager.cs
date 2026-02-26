using System;
using UnityEngine;
using UnityEngine.Events;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;
using Unity.Collections;
using System.Linq;
using Stopwatch = System.Diagnostics.Stopwatch;

public class DestructibleGlobalMeshManager : MonoBehaviour
{
    [Serializable]
    public class SegmentDestroyedEvent : UnityEvent<GameObject>
    {
    }

    [Header("References")]
    public GunScript gunScript;
    public DestructibleGlobalMeshSpawner meshSpawner;

    [Header("Audio")]
    [Tooltip("Single clip (used if no variants assigned). Kept for backward compatibility.")]
    public AudioClip destroySound;
    [Tooltip("Optional: multiple variants for more natural sound when many walls break. If set, a random variant is chosen each time.")]
    public AudioClip[] destroySoundVariants;
    [Tooltip("Pitch variation so simultaneous destructions don't sound identical.")]
    [SerializeField] private float destroySoundPitchMin = 0.88f;
    [SerializeField] private float destroySoundPitchMax = 1.12f;
    [Tooltip("Master volume for wall destroy sounds (slider).")]
    [Range(0f, 1f)]
    [SerializeField] private float destroySoundVolume = 0.6f;

    [Header("Debris Prefabs")]
    public GameObject debrisLargePrefab;
    public GameObject debrisMediumPrefab;
    public GameObject debrisSmallPrefab;

    [Header("Debris Settings")]
    // The old script used 0.3. You might need 0.5 in MR, but start with 0.3 to match old behavior.
    [SerializeField] private float debrisOffset = 0.3f; 
    
    [SerializeField] private float debrisSpawnOffsetRange = 0.5f;
    [SerializeField] private float debrisLifetime = 5f;
    
    [Header("Performance Settings")]
    [SerializeField] private int maxDebrisPerDestruction = 8;
    [SerializeField] private bool enableBarycentricCoordinates = true;
    [SerializeField] private bool enableDebugLogs = false;

    [Header("Events")]
    [SerializeField] private SegmentDestroyedEvent onSegmentDestroyed = new SegmentDestroyedEvent();

    private List<GameObject> segments = new List<GameObject>();
    private DestructibleMeshComponent currentComponent;
    
    // OPTIMIZATION: Dictionary for O(1) segment lookup instead of O(n) linear search
    private Dictionary<GameObject, GameObject> segmentLookup = new Dictionary<GameObject, GameObject>();

    // Used to pass hit position into HandleSegmentDestroyed (same frame) for accurate 3D audio
    private Vector3 lastDestroyHitPosition;

    void Awake()
    {
        OVRManager.eyeFovPremultipliedAlphaModeEnabled = false;
    }

    void Start()
    {
        if (meshSpawner == null) Debug.LogError("MeshSpawner is NULL!");
        if (gunScript == null) Debug.LogError("GunScript is NULL!");
        
        // Ensure ceiling can be destroyed by setting ReservedTop to -1 (no reservation)
        // This allows the ceiling to be included in the destructible mesh segments
        if (meshSpawner != null)
        {
            meshSpawner.ReservedTop = -1f; // -1 means no reservation, allowing ceiling destruction
        }
    }

    void OnEnable()
    {
        if (gunScript != null) gunScript.OnShootAndHit.AddListener(DestroyMeshSegment);
        if (meshSpawner != null)
        {
            meshSpawner.OnDestructibleMeshCreated.AddListener(SetupDestructibleComponents);
            if (enableBarycentricCoordinates) meshSpawner.OnSegmentationCompleted += AddBarycentricCoordinates;
            }
        onSegmentDestroyed.AddListener(HandleSegmentDestroyed);
    }

    void OnDisable()
    {
        if (gunScript != null) gunScript.OnShootAndHit.RemoveListener(DestroyMeshSegment);
        if (meshSpawner != null)
        {
            meshSpawner.OnDestructibleMeshCreated.RemoveListener(SetupDestructibleComponents);
            if (enableBarycentricCoordinates) meshSpawner.OnSegmentationCompleted -= AddBarycentricCoordinates;
        }
        onSegmentDestroyed.RemoveListener(HandleSegmentDestroyed);
    }

    public void SetupDestructibleComponents(DestructibleMeshComponent component)
    {
        if (component == null) return;
        currentComponent = component;
        segments.Clear();
        component.GetDestructibleMeshSegments(segments);
        segments.RemoveAll(s => s == null);

        // OPTIMIZATION: Build dictionary for fast O(1) segment lookup
        segmentLookup.Clear();

        // Standard setup for the wall pieces
        foreach (var item in segments)
        {
            if (item == null) continue;

            MeshCollider col = item.GetComponent<MeshCollider>();
            if (col == null) col = item.AddComponent<MeshCollider>();
            
            // Force non-convex for accurate wall hits
            col.convex = false; 
            col.enabled = true;

            MeshFilter mf = item.GetComponent<MeshFilter>();
            if (mf != null) col.sharedMesh = mf.sharedMesh;

            // FORCE all segments to Default layer for guaranteed collision with bullets
            item.layer = LayerMask.NameToLayer("Default");
            
            // Add to dictionary for fast lookup
            segmentLookup[item] = item;
            
            // Also add children for fast lookup (in case bullet hits child object)
            foreach (Transform child in item.transform)
            {
                if (child != null)
                {
                    segmentLookup[child.gameObject] = item;
                }
            }
        }
    }

    public void DestroyMeshSegment(GameObject segment)
    {
        if (currentComponent == null || segment == null) return;
        if (currentComponent.ReservedSegment == segment) return;

        Stopwatch sw = null;
        if (enableDebugLogs)
        {
            sw = Stopwatch.StartNew();
        }

        GameObject segmentToDestroy = null;

        // OPTIMIZATION: Fast O(1) dictionary lookup first
        if (segmentLookup.TryGetValue(segment, out segmentToDestroy))
        {
            // Found it immediately!
        }
        else
        {
            // QUICK REJECT: If the hit object is not part of the destructible mesh hierarchy, 
            // it's an environment object (Tree, Floor, etc.). Stop here.
            if (currentComponent != null && !segment.transform.IsChildOf(currentComponent.transform))
            {
                // This is a normal occurrence when shooting through holes or at non-destructible objects
                return;
            }

            if (enableDebugLogs) Debug.LogWarning($"[DestructibleGlobalMeshManager] Slow lookup for {segment.name} - falling back to hierarchy search");
            
            // Fallback: Check parent hierarchy (for edge cases)
            Transform current = segment.transform;
            while (current != null && segmentToDestroy == null)
            {
                if (segmentLookup.TryGetValue(current.gameObject, out segmentToDestroy))
                {
                    break;
                }
                current = current.parent;
            }

            // If still not found, check if any segment is a child of the hit object (rare case)
            if (segmentToDestroy == null)
            {
                // This loop is O(N) and expensive. Only run it if we are sure we hit the destructible structure.
                foreach (var seg in segments)
                {
                    if (seg != null && seg.transform.IsChildOf(segment.transform))
                    {
                        segmentToDestroy = seg;
                        break;
                    }
                }
            }
        }

        // If we couldn't find a valid segment, log warning and return
        if (segmentToDestroy == null)
        {
            // Suppress this warning as it happens frequently when hitting non-mapped children
            if (enableDebugLogs) Debug.LogWarning($"[DestructibleGlobalMeshManager] Could not find segment to destroy for hit object: {segment.name}");
            return;
        }

        // Compute hit position for accurate 3D audio (before we destroy the segment)
        Vector3 hitPosition = segment.transform.position;
        Vector3 hitNormal = Vector3.up;
        if (gunScript != null && gunScript.shootingPoint != null)
        {
            Ray ray = new Ray(gunScript.shootingPoint.position, gunScript.shootingPoint.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, gunScript.maxLineDistance, gunScript.layerMask))
            {
                if (hit.collider.gameObject == segment)
                {
                    hitPosition = hit.point;
                    hitNormal = hit.normal;
                }
                else
                {
                    hitNormal = (hitPosition - gunScript.shootingPoint.position).normalized;
                }
            }
        }
        lastDestroyHitPosition = hitPosition;

        currentComponent.DestroySegment(segmentToDestroy);
        onSegmentDestroyed.Invoke(segmentToDestroy);

        // DEBRIS DISABLED - Taking a break from debris functionality
        // InstantiateDebris(hitPosition, hitNormal);

        if (enableDebugLogs && sw != null)
        {
            sw.Stop();
            // Reduced logging frequency/verbosity to prevent spam during normal gameplay
            if (sw.Elapsed.TotalMilliseconds > 1.0f)
            {
                UnityEngine.Debug.Log($"[DestructibleGlobalMeshManager] DestroyMeshSegment took {sw.Elapsed.TotalMilliseconds:F2} ms. Segments tracked: {segments.Count}. Lookup size: {segmentLookup.Count}");
            }
        }
    }

    private void HandleSegmentDestroyed(GameObject destroyedSegment)
    {
        AudioClip clip = ChooseDestroyClip();
        if (clip == null || destroyedSegment == null) return;

        // Use actual hit position for correct directional 3D audio (set in DestroyMeshSegment same frame)
        Vector3 pos = lastDestroyHitPosition;
        float volume = destroySoundVolume * UnityEngine.Random.Range(0.92f, 1f);
        float pitch = UnityEngine.Random.Range(destroySoundPitchMin, destroySoundPitchMax);

        PlayDestroySoundOneShot(clip, pos, volume, pitch);
    }

    private AudioClip ChooseDestroyClip()
    {
        if (destroySoundVariants != null && destroySoundVariants.Length > 0)
        {
            AudioClip chosen = destroySoundVariants[UnityEngine.Random.Range(0, destroySoundVariants.Length)];
            if (chosen != null) return chosen;
        }
        return destroySound;
    }

    private void PlayDestroySoundOneShot(AudioClip clip, Vector3 position, float volume, float pitch)
    {
        GameObject oneShot = new GameObject("DestructibleMesh_DestroyOneShot");
        oneShot.transform.position = position;
        AudioSource source = oneShot.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.playOnAwake = false;

        // Full 3D spatial so direction matches the hit location
        source.spatialBlend = 1f;
        source.dopplerLevel = 0f;
        source.spread = 0f;           // Point source = clear left/right/front/behind
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 1f;
        source.maxDistance = 20f;

        source.Play();
        Destroy(oneShot, clip.length > 0f ? clip.length + 0.1f : 2f);
    }

    // =================================================================
    // THE OLD SCHOOL INSTANTIATION LOGIC
    // No Forces. No Physics Overrides. Just Spawn.
    // =================================================================
    private void InstantiateDebris(Vector3 hitPosition, Vector3 hitNormal)
    {
        // 1. Calculate simple spawn position
        Vector3 spawnPosition = hitPosition + hitNormal.normalized * debrisOffset;
        
        // 2. Random Counts (Matches old script)
        int largeDebrisCount = UnityEngine.Random.Range(0, 2);
        int mediumDebrisCount = UnityEngine.Random.Range(1, 4);
        int smallDebrisCount = UnityEngine.Random.Range(3, 6);

        int totalDebrisSpawned = 0;

        // 3. Spawn Loops - PURE INSTANTIATION
        // We trust the Prefab's Rigidbody to handle gravity.
        
        // Large
        for (int i = 0; i < largeDebrisCount; i++)
        {
            if (maxDebrisPerDestruction > 0 && totalDebrisSpawned >= maxDebrisPerDestruction) break;
            SpawnSingleDebris(debrisLargePrefab, spawnPosition);
            totalDebrisSpawned++;
        }

        // Medium
        for (int i = 0; i < mediumDebrisCount; i++)
            {
                if (maxDebrisPerDestruction > 0 && totalDebrisSpawned >= maxDebrisPerDestruction) break;
            SpawnSingleDebris(debrisMediumPrefab, spawnPosition);
                totalDebrisSpawned++;
            }

        // Small
        for (int i = 0; i < smallDebrisCount; i++)
            {
                if (maxDebrisPerDestruction > 0 && totalDebrisSpawned >= maxDebrisPerDestruction) break;
            SpawnSingleDebris(debrisSmallPrefab, spawnPosition);
                totalDebrisSpawned++;
            }
        }

    private void SpawnSingleDebris(GameObject prefab, Vector3 centerPos)
        {
        if (prefab == null) return;

                Vector3 randomOffset = new Vector3(
                    UnityEngine.Random.Range(-debrisSpawnOffsetRange, debrisSpawnOffsetRange),
                    UnityEngine.Random.Range(-debrisSpawnOffsetRange, debrisSpawnOffsetRange),
                    UnityEngine.Random.Range(-debrisSpawnOffsetRange, debrisSpawnOffsetRange)
                );

        GameObject debris = Instantiate(prefab, centerPos + randomOffset, UnityEngine.Random.rotation);
        
        // We do NOT touch the Rigidbody here. 
        // It relies 100% on the Inspector settings of the Prefab.
        
        Destroy(debris, debrisLifetime);
    }

    // --- Shader Support ---
    private DestructibleMeshComponent.MeshSegmentationResult AddBarycentricCoordinates(DestructibleMeshComponent.MeshSegmentationResult res)
    {
        Stopwatch sw = null;
        if (enableDebugLogs) 
        {
            sw = Stopwatch.StartNew();
            UnityEngine.Debug.Log("[DestructibleGlobalMeshManager] AddBarycentricCoordinates called - starting calculation...");
        }
        
        List<DestructibleMeshComponent.MeshSegment> newSegs = new List<DestructibleMeshComponent.MeshSegment>();
        foreach (var seg in res.segments) newSegs.Add(AddBar(seg));
        
        var result = new DestructibleMeshComponent.MeshSegmentationResult() { segments = newSegs, reservedSegment = AddBar(res.reservedSegment) };

        if (enableDebugLogs && sw != null)
        {
            sw.Stop();
            UnityEngine.Debug.Log($"[DestructibleGlobalMeshManager] AddBarycentricCoordinates finished in {sw.Elapsed.TotalMilliseconds:F2} ms for {res.segments.Count} segments.");
        }

        return result;
    }

    private static DestructibleMeshComponent.MeshSegment AddBar(DestructibleMeshComponent.MeshSegment seg)
    {
        using NativeArray<Vector3> vs = new NativeArray<Vector3>(seg.positions, Allocator.Temp);
        using NativeArray<int> ts = new NativeArray<int>(seg.indices, Allocator.Temp);
        Vector3[] v = new Vector3[ts.Length];
        int[] idx = new int[ts.Length];
        Vector4[] bc = new Vector4[ts.Length];
        for (int i = 0; i < ts.Length; i += 3) {
            v[i] = vs[ts[i]]; v[i+1] = vs[ts[i+1]]; v[i+2] = vs[ts[i+2]];
            bc[i] = new Vector4(1,0,0,0); bc[i+1] = new Vector4(0,1,0,0); bc[i+2] = new Vector4(0,0,1,0);
        }
        for(int i=0; i<ts.Length; i++) idx[i] = i;
        return new DestructibleMeshComponent.MeshSegment() { indices = idx, positions = v, tangents = bc };
    }
}
