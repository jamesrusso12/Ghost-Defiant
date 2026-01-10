using System;
using UnityEngine;
using UnityEngine.Events;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;
using Unity.Collections;
using System.Linq;

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
    public AudioClip destroySound;

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

    [Header("Events")]
    [SerializeField] private SegmentDestroyedEvent onSegmentDestroyed = new SegmentDestroyedEvent();

    private List<GameObject> segments = new List<GameObject>();
    private DestructibleMeshComponent currentComponent;
    
    // OPTIMIZATION: Dictionary for O(1) segment lookup instead of O(n) linear search
    private Dictionary<GameObject, GameObject> segmentLookup = new Dictionary<GameObject, GameObject>();

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

        GameObject segmentToDestroy = null;

        // OPTIMIZATION: Fast O(1) dictionary lookup first
        if (segmentLookup.TryGetValue(segment, out segmentToDestroy))
        {
            // Found it immediately!
        }
        else
        {
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
            Debug.LogWarning($"[DestructibleGlobalMeshManager] Could not find segment to destroy for hit object: {segment.name}");
            return;
        }

        currentComponent.DestroySegment(segmentToDestroy);
        onSegmentDestroyed.Invoke(segmentToDestroy);

        // Simple Hit Calculation
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

        // DEBRIS DISABLED - Taking a break from debris functionality
        // InstantiateDebris(hitPosition, hitNormal);
    }

    private void HandleSegmentDestroyed(GameObject destroyedSegment)
    {
        if (destroySound != null && destroyedSegment != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, destroyedSegment.transform.position, UnityEngine.Random.Range(0.8f, 1.2f));
        }
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
        List<DestructibleMeshComponent.MeshSegment> newSegs = new List<DestructibleMeshComponent.MeshSegment>();
        foreach (var seg in res.segments) newSegs.Add(AddBar(seg));
        return new DestructibleMeshComponent.MeshSegmentationResult() { segments = newSegs, reservedSegment = AddBar(res.reservedSegment) };
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
