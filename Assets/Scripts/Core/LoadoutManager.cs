using System.Collections;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class LoadoutManager : MonoBehaviour
{
    public static LoadoutManager instance;

    [Header("Dummy Prefabs (Spawned on Floor)")]
    public GameObject dummyGunPrefab;
    public GameObject dummyFlashlightPrefab;

    [Header("Real Items (In Hand - Assigned in Scene)")]
    public GameObject realGunObject;       // Drag the gun from RightHandAnchor here
    public GameObject realFlashlightObject; // Drag flashlight from LeftHandAnchor here

    [Header("Flashlight Extras (Enable when flashlight is collected)")]
    [Tooltip("Any additional objects to enable when the flashlight is collected (e.g., your cone/stencil passthrough object, building block passthrough flashlight object).")]
    public GameObject[] enableOnFlashlightCollected;

    [Header("Gun Extras (Enable when gun is collected)")]
    [Tooltip("Any additional objects to enable when the gun is collected (optional).")]
    public GameObject[] enableOnGunCollected;

    [Header("Snap (Orientation)")]
    [Tooltip("If assigned, real gun will be parented to this transform when equipped.")]
    public Transform gunMount;
    [Tooltip("If assigned, real flashlight will be parented to this transform when equipped.")]
    public Transform flashlightMount;

    [Tooltip("Applied after parenting to Gun Mount (local space).")]
    public Vector3 gunLocalPositionOffset = Vector3.zero;
    [Tooltip("Applied after parenting to Gun Mount (local space, Euler degrees).")]
    public Vector3 gunLocalEulerOffset = Vector3.zero;

    [Tooltip("Applied after parenting to Flashlight Mount (local space).")]
    public Vector3 flashlightLocalPositionOffset = Vector3.zero;
    [Tooltip("Applied after parenting to Flashlight Mount (local space, Euler degrees).")]
    public Vector3 flashlightLocalEulerOffset = Vector3.zero;

    [Header("Spawn Settings")]
    public float initialDelay = 2.0f;
    public float minEdgeDistance = 0.3f;
    public GameObject spawnEffectPrefab;
    [Tooltip("How many attempts to find a valid spawn position inside the room.")]
    public int maxSpawnAttempts = 200;
    [Tooltip("Minimum radius from room boundaries for in-room random positions.")]
    public float inRoomMinRadius = 0.2f;
    [Tooltip("How many attempts to find a valid PAIR of positions (gun + flashlight) before giving up.")]
    public int maxPairSpawnAttempts = 50;
    [Tooltip("Minimum distance between gun and flashlight spawn points.")]
    public float minDistanceBetweenItems = 0.5f;

    [Header("Furniture/Obstacle Avoidance")]
    [Tooltip("Colliders on these layers will be treated as obstacles (e.g., MRUK furniture surfaces, room mesh, custom blockers).")]
    public LayerMask spawnBlockerMask = ~0; // everything by default
    [Tooltip("Horizontal clearance radius around spawn point (meters). Avoid spawning if any obstacle overlaps this area above the floor.")]
    public float spawnClearanceRadius = 0.18f;
    [Tooltip("Vertical clearance above the floor that must be empty (meters). Increase if items still clip into beds/couches.")]
    public float spawnClearanceHeight = 0.45f;

    [Header("Spawn Look (Natural Rest Pose)")]
    [Tooltip("Spawn items slightly above the floor and ease them down for a more natural look.")]
    public bool animateSettle = true;
    [Tooltip("How far above the floor to start the settle animation (meters).")]
    public float settleStartHeight = 0.12f;
    [Tooltip("How long the settle animation should take (seconds).")]
    public float settleDuration = 0.25f;
    [Tooltip("Random yaw around floor normal (degrees).")]
    public float randomYawDegrees = 180f;
    [Tooltip("Random tilt around sideways axes (degrees).")]
    public float randomTiltDegrees = 6f;
    [Tooltip("Per-item rest rotation applied AFTER aligning to floor (Euler degrees). Use this to make an item lay on its side naturally.")]
    public Vector3 gunRestEuler = new Vector3(90f, 0f, 0f);
    [Tooltip("Per-item rest rotation applied AFTER aligning to floor (Euler degrees).")]
    public Vector3 flashlightRestEuler = new Vector3(90f, 0f, 0f);

    [Header("Debug")]
    public bool debugForceStart = false;
    [Tooltip("Show persistent red/blue spheres at attempted spawn locations (red=rejected, blue=accepted)")]
    public bool debugShowSpawnAttempts = false;
    [Tooltip("Show large green sphere at final spawn locations")]
    public bool debugShowFinalSpawns = true;

    private bool gunCollected = false;
    private bool flashlightCollected = false;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Ensure real items are hidden at start
        if (realGunObject != null) realGunObject.SetActive(false);
        if (realFlashlightObject != null) realFlashlightObject.SetActive(false);
        SetActiveForAll(enableOnFlashlightCollected, false);
        SetActiveForAll(enableOnGunCollected, false);

        if (GameManager.instance != null)
        {
             StartCoroutine(IntroSequence());
        }
    }

    void Update()
    {
        if (debugForceStart)
        {
            debugForceStart = false;
            OnItemPickedUp(SimplePickup.ItemType.Gun);
            OnItemPickedUp(SimplePickup.ItemType.Flashlight);
        }
    }

    // Called by SimplePickup.cs
    public void OnItemPickedUp(SimplePickup.ItemType type)
    {
        if (type == SimplePickup.ItemType.Gun)
        {
            gunCollected = true;
            if (realGunObject != null)
            {
                Debug.LogError($"[LoadoutManager] Activating Real Gun: {realGunObject.name}");
                realGunObject.SetActive(true);
                
                // Force enable all child renderers (in case children are disabled)
                EnableAllChildRenderers(realGunObject);

                SnapToMount(realGunObject, gunMount, gunLocalPositionOffset, gunLocalEulerOffset);

                // Enable any extra gun-related objects
                SetActiveForAll(enableOnGunCollected, true);

                // Tell the GunScript it's equipped so it can fire (important if using GunMount)
                GunScript gun = realGunObject.GetComponentInChildren<GunScript>(true);
                if (gun != null)
                {
                    gun.SetEquipped(true);
                }
            }
            else
            {
                Debug.LogError("[LoadoutManager] Real Gun Object is NULL! Check Inspector assignment.");
            }
        }
        else if (type == SimplePickup.ItemType.Flashlight)
        {
            flashlightCollected = true;
            if (realFlashlightObject != null)
            {
                Debug.LogError($"[LoadoutManager] Activating Real Flashlight: {realFlashlightObject.name}");
                realFlashlightObject.SetActive(true);
                
                EnableAllChildRenderers(realFlashlightObject);

                SnapToMount(realFlashlightObject, flashlightMount, flashlightLocalPositionOffset, flashlightLocalEulerOffset);

                // Enable any extra flashlight-related objects (your cone / secondary passthrough, etc.)
                SetActiveForAll(enableOnFlashlightCollected, true);
            }
            else
            {
                Debug.LogError("[LoadoutManager] Real Flashlight Object is NULL! Check Inspector assignment.");
            }
        }

        CheckAllCollected();
    }
    
    // Helper to force-enable all mesh renderers in case children are disabled
    void EnableAllChildRenderers(GameObject obj)
    {
        MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var r in renderers)
        {
            r.enabled = true;
            r.gameObject.SetActive(true);
        }
    }

    void SnapToMount(GameObject obj, Transform mount, Vector3 localPosOffset, Vector3 localEulerOffset)
    {
        if (obj == null || mount == null) return;

        // Preserve local scale in case parent changes scale
        Vector3 originalLocalScale = obj.transform.localScale;

        obj.transform.SetParent(mount, worldPositionStays: false);
        obj.transform.localPosition = localPosOffset;
        obj.transform.localRotation = Quaternion.Euler(localEulerOffset);
        obj.transform.localScale = originalLocalScale;

        Debug.LogError($"[LoadoutManager] Snapped {obj.name} to mount {mount.name} | pos {localPosOffset} rot {localEulerOffset}");
    }

    void SetActiveForAll(GameObject[] objs, bool active)
    {
        if (objs == null) return;
        foreach (var o in objs)
        {
            if (o != null) o.SetActive(active);
        }
    }

    void CheckAllCollected()
    {
        if (gunCollected && flashlightCollected)
        {
            Debug.Log("[LoadoutManager] All items collected! Starting game.");
            StartGameLoop();
        }
    }

    void StartGameLoop()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.StartGame();
        }
    }

    IEnumerator IntroSequence()
    {
        Debug.LogError("[LoadoutManager] 1. Intro STARTED. Waiting for delay...");
        yield return new WaitForSeconds(initialDelay);

        Debug.LogError("[LoadoutManager] 2. Delay finished. Checking MRUK...");

        // Try to wait for MRUK, increased timeout to 5 seconds for better reliability
        float timeout = 5.0f;
        float timer = 0f;
        
        bool mrukReady = false;

        while (timer < timeout)
        {
            if (MRUK.Instance != null && MRUK.Instance.IsInitialized)
            {
                var room = MRUK.Instance.GetCurrentRoom();
                if (room != null && room.Anchors != null)
                {
                    // Extra validation: Check if room has floor anchors
                    bool hasFloor = false;
                    foreach (var anchor in room.Anchors)
                    {
                        if (anchor.Label.HasFlag(MRUKAnchor.SceneLabels.FLOOR))
                        {
                            Debug.LogError($"[LoadoutManager] MRUK Room validated with floor anchor: {anchor.name}");
                            hasFloor = true;
                            break;
                        }
                    }
                    
                    if (hasFloor)
                    {
                        mrukReady = true;
                        break;
                    }
                    else
                    {
                        Debug.LogWarning($"[LoadoutManager] MRUK room exists but no FLOOR anchor found yet. Waiting... ({timer:F1}s/{timeout}s)");
                    }
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (mrukReady)
        {
            Debug.LogError("[LoadoutManager] 3a. MRUK Ready with validated floor. Spawning on floor.");
            SpawnItemsInRoom(MRUK.Instance.GetCurrentRoom());
        }
        else
        {
            Debug.LogError("[LoadoutManager] 3b. MRUK timed out or no valid floor found. Using Fallback Spawning.");
            SpawnFallback();
        }
    }

    void SpawnFallback()
    {
        Debug.LogError("[LoadoutManager] Spawning Fallback Dummies in front of Camera.");
        
        if (Camera.main == null)
        {
             Debug.LogError("[LoadoutManager] CRITICAL: Camera.main is NULL! Cannot find spawn position.");
             return;
        }

        // Spawn 0.5m in front of face, and 0.3m down (Chest level / floating)
        // We use "floating" spawn to guarantee it doesn't clip through floor
        Vector3 spawnPos = Camera.main.transform.position + (Camera.main.transform.forward * 0.5f) + (Vector3.down * 0.3f);

        if (dummyGunPrefab != null)
        {
            Debug.LogError($"[LoadoutManager] Instantiating Dummy Gun at {spawnPos}");
            GameObject g = Instantiate(dummyGunPrefab, spawnPos + Vector3.right * 0.2f, Quaternion.identity);
            
            // DEBUG: Force visible scale and create a marker
            // If import scale is 0.01, we might need to boost it, but usually prefabs handle this.
            // g.transform.localScale = Vector3.one; 
            
            var p = g.GetComponent<SimplePickup>(); 
            if (p == null) p = g.AddComponent<SimplePickup>();
            p.itemType = SimplePickup.ItemType.Gun;
            
            // Disable gravity temporarily to ensure they don't fall through floor
            var rb = g.GetComponent<Rigidbody>();
            if (rb != null) rb.useGravity = false; 
        }
        else
        {
            Debug.LogError("[LoadoutManager] Dummy Gun Prefab is NULL!");
        }

        if (dummyFlashlightPrefab != null)
        {
            Debug.LogError($"[LoadoutManager] Instantiating Dummy Flashlight at {spawnPos}");
            GameObject f = Instantiate(dummyFlashlightPrefab, spawnPos - Vector3.right * 0.2f, Quaternion.identity);
            
            var p = f.GetComponent<SimplePickup>();
            if (p == null) p = f.AddComponent<SimplePickup>();
            p.itemType = SimplePickup.ItemType.Flashlight;
            
            var rb = f.GetComponent<Rigidbody>();
            if (rb != null) rb.useGravity = false;
        }
        else
        {
            Debug.LogError("[LoadoutManager] Dummy Flashlight Prefab is NULL!");
        }
    }

    void SpawnItemsInRoom(MRUKRoom room)
    {
        // Transactional spawn: ensure BOTH items spawn inside the room, or retry.
        if (dummyGunPrefab == null || dummyFlashlightPrefab == null)
        {
            Debug.LogError("[LoadoutManager] Dummy prefabs are not assigned. Cannot spawn items.");
            return;
        }

        if (TryGetSpawnPairInRoom(room, out Vector3 gunPos, out Vector3 gunNormal, out Vector3 flashlightPos, out Vector3 flashlightNormal))
        {
            SpawnAt(dummyGunPrefab, gunPos, gunNormal, SimplePickup.ItemType.Gun);
            SpawnAt(dummyFlashlightPrefab, flashlightPos, flashlightNormal, SimplePickup.ItemType.Flashlight);
        }
        else
        {
            // If MRUK is "ready" but we still can't find positions, fall back to room bounds center
            Debug.LogWarning("[LoadoutManager] Failed to find valid spawn pair inside room. Using room-bounds fallback.");
            if (TryGetRoomBoundsFallbackPair(room, out gunPos, out flashlightPos))
            {
                // Bounds fallback doesn't have a reliable floor normal; assume up.
                SpawnAt(dummyGunPrefab, gunPos, Vector3.up, SimplePickup.ItemType.Gun);
                SpawnAt(dummyFlashlightPrefab, flashlightPos, Vector3.up, SimplePickup.ItemType.Flashlight);
            }
            else
            {
                // Absolute last resort: existing camera fallback (can be outside room if MRUK data is broken)
                SpawnFallback();
            }
        }
    }

    void SpawnAt(GameObject prefab, Vector3 floorPos, Vector3 floorNormal, SimplePickup.ItemType type)
    {
        if (prefab == null) return;

        // Compute a natural "resting" rotation on the floor
        Quaternion restRot = ComputeRestRotation(floorNormal, type);

        // Place slightly above the floor based on collider bounds so it doesn't clip
        Vector3 placePos = ComputeRestingPosition(prefab, floorPos, floorNormal, restRot);

        // Debug: Show final spawn location
        if (debugShowFinalSpawns)
        {
            string itemName = type == SimplePickup.ItemType.Gun ? "GUN" : "FLASHLIGHT";
            CreateDebugSphere(placePos, 0.12f, Color.green, 30f, itemName);
        }

        GameObject obj = Instantiate(prefab, placePos, restRot);

        // Disable gravity so items don't fall through floor (pickup-only flow)
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Setup SimplePickup component
        SimplePickup pickup = obj.GetComponent<SimplePickup>();
        if (pickup == null) pickup = obj.AddComponent<SimplePickup>();
        pickup.itemType = type;

        if (spawnEffectPrefab != null) Instantiate(spawnEffectPrefab, placePos, Quaternion.identity);

        if (animateSettle)
        {
            // Start slightly higher and settle down for a more natural look
            Vector3 startPos = placePos + floorNormal.normalized * Mathf.Max(0f, settleStartHeight);
            obj.transform.position = startPos;
            StartCoroutine(SettleToPose(obj.transform, placePos, restRot, floorNormal));
        }
    }

    bool TryGetSpawnPairInRoom(MRUKRoom room, out Vector3 gunPos, out Vector3 gunNormal, out Vector3 flashlightPos, out Vector3 flashlightNormal)
    {
        gunPos = Vector3.zero;
        gunNormal = Vector3.up;
        flashlightPos = Vector3.zero;
        flashlightNormal = Vector3.up;
        if (room == null) return false;

        for (int pairAttempt = 0; pairAttempt < maxPairSpawnAttempts; pairAttempt++)
        {
            if (!TryGetFloorSpawnPosition(room, out gunPos, out gunNormal)) continue;
            if (!TryGetFloorSpawnPosition(room, out flashlightPos, out flashlightNormal)) continue;

            if (Vector3.Distance(gunPos, flashlightPos) < minDistanceBetweenItems) continue;

            // Both positions valid and far enough apart
            return true;
        }

        return false;
    }

    bool TryGetFloorSpawnPosition(MRUKRoom room, out Vector3 floorPos, out Vector3 floorNormal)
    {
        floorPos = Vector3.zero;
        floorNormal = Vector3.up;
        if (room == null) return false;

        // Prefer spawning on the FLOOR surface (ground floor). This avoids tables/couches/etc.
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            bool found = room.GenerateRandomPositionOnSurface(
                MRUK.SurfaceType.FACING_UP,
                minEdgeDistance,
                new LabelFilter(MRUKAnchor.SceneLabels.FLOOR),
                out Vector3 pos,
                out Vector3 norm
            );

            if (!found) continue;
            
            // Debug: Show attempted position
            if (debugShowSpawnAttempts && i % 20 == 0) // Only show every 20th attempt to avoid spam
            {
                CreateDebugSphere(pos, 0.05f, Color.yellow, 5f);
            }
            
            if (!room.IsPositionInRoom(pos, true))
            {
                if (debugShowSpawnAttempts && i % 20 == 0)
                {
                    CreateDebugSphere(pos, 0.05f, Color.red, 5f); // Red = outside room
                }
                continue;
            }

            floorPos = pos;
            floorNormal = norm.sqrMagnitude > 0.0001f ? norm.normalized : Vector3.up;
            
            if (!IsVerticalClear(floorPos, floorNormal))
            {
                if (debugShowSpawnAttempts && i % 20 == 0)
                {
                    CreateDebugSphere(pos, 0.05f, new Color(1f, 0.5f, 0f), 5f); // Orange = blocked by furniture
                }
                continue;
            }
            
            // Success! Show blue sphere
            if (debugShowSpawnAttempts)
            {
                CreateDebugSphere(pos, 0.08f, Color.cyan, 10f);
            }
            
            return true;
        }

        // Fallback: any point in room volume (still inside room), then lift slightly.
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3? posOpt = room.GenerateRandomPositionInRoom(inRoomMinRadius, true);
            if (!posOpt.HasValue) continue;
            Vector3 pos = posOpt.Value;
            if (!room.IsPositionInRoom(pos, true)) continue;
            floorPos = pos;
            floorNormal = Vector3.up;
            if (!IsVerticalClear(floorPos, floorNormal)) continue;
            return true;
        }

        return false;
    }

    bool TryGetRoomBoundsFallbackPair(MRUKRoom room, out Vector3 gunPos, out Vector3 flashlightPos)
    {
        gunPos = Vector3.zero;
        flashlightPos = Vector3.zero;
        if (room == null) return false;

        Bounds b = room.GetRoomBounds();
        if (b.size.sqrMagnitude <= 0.001f) return false;

        // Put them near the center, offset left/right inside the room bounds.
        Vector3 center = b.center;
        float y = b.min.y + 1.0f;
        Vector3 right = Vector3.right;

        gunPos = new Vector3(center.x + 0.3f, y, center.z);
        flashlightPos = new Vector3(center.x - 0.3f, y, center.z);

        if (!room.IsPositionInRoom(gunPos, true) || !room.IsPositionInRoom(flashlightPos, true))
        {
            // Try without vertical bounds check in case floor/ceiling anchors are weird
            if (!room.IsPositionInRoom(gunPos, false) || !room.IsPositionInRoom(flashlightPos, false))
            {
                return false;
            }
        }

        return true;
    }

    bool IsVerticalClear(Vector3 floorPos, Vector3 floorNormal)
    {
        Vector3 n = (floorNormal.sqrMagnitude > 0.0001f) ? floorNormal.normalized : Vector3.up;
        // Center the check box halfway up the clearance height
        Vector3 center = floorPos + n * (spawnClearanceHeight * 0.5f);
        Vector3 halfExtents = new Vector3(spawnClearanceRadius, spawnClearanceHeight * 0.5f, spawnClearanceRadius);
        // Align the box to world (axis-aligned works fine for small extents)
        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, spawnBlockerMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return true;
        // We allow the floor mesh itself if present very close to the floor plane; otherwise treat as blocked
        // A pragmatic approach: ignore colliders whose world center is below the floor and closer than 1cm.
        foreach (var h in hits)
        {
            if (h == null) continue;
            if (h.bounds.center.y < floorPos.y + 0.01f) continue;
            return false;
        }
        return true;
    }

    Quaternion ComputeRestRotation(Vector3 floorNormal, SimplePickup.ItemType type)
    {
        Vector3 n = (floorNormal.sqrMagnitude > 0.0001f) ? floorNormal.normalized : Vector3.up;

        // Build a stable "floor frame" rotation where Up == floor normal.
        // This makes the rest euler offsets intuitive:
        // - local Y rotates around floor normal (spin on the floor)
        // - local X/Z roll/tilt around axes that lie on the floor plane
        Vector3 forwardOnFloor = Vector3.ProjectOnPlane(Vector3.forward, n);
        if (forwardOnFloor.sqrMagnitude < 0.001f)
        {
            forwardOnFloor = Vector3.ProjectOnPlane(Vector3.right, n);
        }
        forwardOnFloor.Normalize();

        Quaternion floorFrame = Quaternion.LookRotation(forwardOnFloor, n);

        // Random yaw around floor normal (spin on floor)
        float yaw = Random.Range(-randomYawDegrees, randomYawDegrees);
        Quaternion yawLocal = Quaternion.AngleAxis(yaw, Vector3.up); // in floorFrame space

        // Per-item "rest" rotation applied in floorFrame space (this is what you tweak)
        Vector3 restEuler = type == SimplePickup.ItemType.Gun ? gunRestEuler : flashlightRestEuler;
        Quaternion restLocal = Quaternion.Euler(restEuler);

        // Small random tilt for realism (in floorFrame space)
        float tiltX = Random.Range(-randomTiltDegrees, randomTiltDegrees);
        float tiltZ = Random.Range(-randomTiltDegrees, randomTiltDegrees);
        Quaternion tiltLocal = Quaternion.Euler(tiltX, 0f, tiltZ);

        return floorFrame * yawLocal * restLocal * tiltLocal;
    }

    Vector3 ComputeRestingPosition(GameObject prefab, Vector3 floorPos, Vector3 floorNormal, Quaternion rotation)
    {
        // Try to estimate "half height" along floor normal from collider bounds.
        // Since prefab bounds aren’t directly accessible without instantiating,
        // use a conservative small offset and let the settle animation hide minor clipping.
        float baseOffset = 0.02f;
        Vector3 n = (floorNormal.sqrMagnitude > 0.0001f) ? floorNormal.normalized : Vector3.up;
        return floorPos + n * baseOffset;
    }

    IEnumerator SettleToPose(Transform t, Vector3 targetPos, Quaternion targetRot, Vector3 floorNormal)
    {
        if (t == null) yield break;
        float dur = Mathf.Max(0.01f, settleDuration);
        Vector3 startPos = t.position;
        Quaternion startRot = t.rotation;
        float elapsed = 0f;

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / dur);
            // Ease-out with a tiny "drop" feel
            float ease = 1f - Mathf.Pow(1f - u, 3f);
            t.position = Vector3.Lerp(startPos, targetPos, ease);
            t.rotation = Quaternion.Slerp(startRot, targetRot, ease);
            yield return null;
        }

        t.position = targetPos;
        t.rotation = targetRot;
    }

    void CreateDebugSphere(Vector3 position, float radius, Color color, float lifetime, string label = "")
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = $"DebugSphere_{label}";
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * radius * 2f;
        
        // Make it bright and visible
        Renderer r = sphere.GetComponent<Renderer>();
        if (r != null)
        {
            r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            r.material.color = color;
            r.material.SetFloat("_Smoothness", 0.8f);
        }
        
        // Remove collider so it doesn't interfere
        Collider c = sphere.GetComponent<Collider>();
        if (c != null) Destroy(c);
        
        // Auto-destroy after lifetime
        Destroy(sphere, lifetime);
    }
}
