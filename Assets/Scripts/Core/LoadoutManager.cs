using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class LoadoutManager : MonoBehaviour
{
    public static LoadoutManager instance;

    // ═════════════════════════════════════════════════════════════════════════════
    // SECTION 1: FLOOR PICKUPS - What spawns on the ground for player to collect
    // ═════════════════════════════════════════════════════════════════════════════
    [Header("--- 1. FLOOR PICKUPS (Prefabs) ---")]
    [Tooltip("Dummy gun prefab that spawns on floor")]
    public GameObject dummyGunPrefab;
    [Tooltip("Dummy flashlight prefab that spawns on floor")]
    public GameObject dummyFlashlightPrefab;

    // ═════════════════════════════════════════════════════════════════════════════
    // SECTION 2: IN-HAND ITEMS - What appears in hands after pickup
    // ═════════════════════════════════════════════════════════════════════════════
    [Header("--- 2. IN-HAND ITEMS (Scene Objects) ---")]
    [Tooltip("Real gun object from scene (starts disabled)")]
    public GameObject realGunObject;
    [Tooltip("Real flashlight object from scene (starts disabled)")]
    public GameObject realFlashlightObject;

    // ═════════════════════════════════════════════════════════════════════════════
    // SECTION 3: EXTRA OBJECTS - Additional objects to turn on after pickup
    // ═════════════════════════════════════════════════════════════════════════════
    [Header("--- 3. EXTRA OBJECTS TO ACTIVATE ---")]
    [Tooltip("Objects to enable when flashlight collected (e.g., passthrough cone)")]
    public GameObject[] enableOnFlashlightCollected;
    [Tooltip("Objects to enable when gun collected (optional)")]
    public GameObject[] enableOnGunCollected;

    // ═════════════════════════════════════════════════════════════════════════════
    // SECTION 4: IN-HAND POSITIONING - Where and how items attach to controllers
    // ═════════════════════════════════════════════════════════════════════════════
    [Header("--- 4. IN-HAND POSITION & ROTATION ---")]
    [Tooltip("Where gun attaches (usually RightControllerInHandAnchor)")]
    public Transform gunMount;
    [Tooltip("Where flashlight attaches (usually LeftControllerInHandAnchor)")]
    public Transform flashlightMount;
    
    [Space(5)]
    [Tooltip("Gun position adjustment in hand (X, Y, Z in meters)")]
    public Vector3 gunHandPosition = Vector3.zero;
    [Tooltip("Gun rotation adjustment in hand (X, Y, Z in degrees)")]
    public Vector3 gunHandRotation = Vector3.zero;
    
    [Space(5)]
    [Tooltip("Flashlight position adjustment in hand (X, Y, Z in meters)")]
    public Vector3 flashlightHandPosition = Vector3.zero;
    [Tooltip("Flashlight rotation adjustment in hand (X, Y, Z in degrees)")]
    public Vector3 flashlightHandRotation = Vector3.zero;

    // ═════════════════════════════════════════════════════════════════════════════
    // SECTION 5: FLOOR SPAWN SETTINGS - Where and when items appear on floor
    // ═════════════════════════════════════════════════════════════════════════════
    [Header("--- 5. FLOOR SPAWN SETTINGS ---")]
    [Tooltip("Wait time before spawning items (seconds)")]
    public float spawnDelay = 2.0f;
    [Tooltip("Keep items this far from walls (meters)")]
    public float wallDistance = 0.15f;
    [Tooltip("Optional spawn effect VFX")]
    public GameObject spawnEffectPrefab;
    
    [Space(5)]
    [Tooltip("Max attempts to find valid spawn spot per item")]
    public int maxAttempts = 200;
    [Tooltip("Keep spawns this far from room edges (meters)")]
    public float edgeBuffer = 0.2f;
    [Tooltip("Max attempts to find valid spots for BOTH items together")]
    public int maxPairAttempts = 50;
    [Tooltip("Minimum distance between gun and flashlight (meters)")]
    public float itemSpacing = 0.5f;
    [Tooltip("Maximum distance between gun and flashlight (meters) - keeps items visible together")]
    public float maxItemDistance = 1.2f;

    // ═════════════════════════════════════════════════════════════════════════════
    // SECTION 6: FURNITURE AVOIDANCE - Prevent spawning inside beds/couches/tables
    // ═════════════════════════════════════════════════════════════════════════════
    [Header("--- 6. FURNITURE AVOIDANCE ---")]
    [Tooltip("Layers with furniture to avoid (VirtualEnvironment, etc.)")]
    public LayerMask furnitureLayers = ~0;
    [Tooltip("Horizontal clearance around spawn (meters) - increase if spawning in furniture")]
    public float horizontalClearance = 0.25f;
    [Tooltip("Vertical clearance above floor (meters) - increase for beds/tall furniture")]
    public float verticalClearance = 0.60f;
    [Tooltip("Minimum distance from furniture anchors (beds, couches, tables) - meters")]
    public float minFurnitureDistance = 0.8f;

    // ═════════════════════════════════════════════════════════════════════════════
    // SECTION 7: FLOOR APPEARANCE - How items look when resting on ground
    // ═════════════════════════════════════════════════════════════════════════════
    [Header("--- 7. FLOOR APPEARANCE ---")]
    [Tooltip("Animate items settling onto floor")]
    public bool useSettleAnimation = true;
    [Tooltip("Start height for settle animation (meters)")]
    public float settleHeight = 0.12f;
    [Tooltip("Settle animation duration (seconds)")]
    public float settleSpeed = 0.25f;
    
    [Space(5)]
    [Tooltip("Random rotation amount around vertical axis (degrees)")]
    public float randomRotation = 180f;
    [Tooltip("Random tilt amount for natural look (degrees)")]
    public float randomTilt = 6f;
    
    [Space(5)]
    [Tooltip("Gun rotation on floor (X,Y,Z degrees) - try (0,0,90) for laying on side")]
    public Vector3 gunFloorRotation = new Vector3(0f, 0f, 90f);
    [Tooltip("Flashlight rotation on floor (X,Y,Z degrees)")]
    public Vector3 flashlightFloorRotation = new Vector3(90f, 0f, 0f);
    
    [Space(5)]
    [Tooltip("Gun height above floor (meters) - adjust if clipping or floating")]
    public float gunFloorHeight = 0.02f;
    [Tooltip("Flashlight height above floor (meters)")]
    public float flashlightFloorHeight = 0.02f;

    // ═════════════════════════════════════════════════════════════════════════════
    // SECTION 8: DEBUG OPTIONS - Troubleshooting and visualization tools
    // ═════════════════════════════════════════════════════════════════════════════
    [Header("--- 8. DEBUG OPTIONS ---")]
    [Tooltip("Skip floor spawn, equip items in hands immediately")]
    public bool skipFloorSpawn = false;
    [Tooltip("Show colored spheres at spawn attempts (yellow=trying, red=rejected, orange=blocked, cyan=accepted)")]
    public bool showAttempts = false;
    [Tooltip("Show green transparent sphere at final spawn locations")]
    public bool showFinalSpawns = true;
    [Tooltip("Use camera-relative spawn as PRIMARY method (guaranteed visible, ignores MRUK furniture)")]
    public bool useCameraRelativeSpawn = false;

    private bool gunCollected = false;
    private bool flashlightCollected = false;
    private bool controllersHidden = false;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Validate prefabs
        if (dummyGunPrefab == null)
            Debug.LogError("[LoadoutManager] CRITICAL: Dummy Gun Prefab is NOT ASSIGNED in Inspector!");
        else
            Debug.LogError($"[LoadoutManager] Dummy Gun Prefab OK: {dummyGunPrefab.name}");
            
        if (dummyFlashlightPrefab == null)
            Debug.LogError("[LoadoutManager] CRITICAL: Dummy Flashlight Prefab is NOT ASSIGNED in Inspector!");
        else
            Debug.LogError($"[LoadoutManager] Dummy Flashlight Prefab OK: {dummyFlashlightPrefab.name}");
        
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
        if (skipFloorSpawn)
        {
            skipFloorSpawn = false;
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

                SnapToMount(realGunObject, gunMount, gunHandPosition, gunHandRotation);

                // Hide controller models to prevent overlap with gun
                HideControllerModels();

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

                SnapToMount(realFlashlightObject, flashlightMount, flashlightHandPosition, flashlightHandRotation);

                // Hide controller models to prevent overlap with flashlight
                HideControllerModels();

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
        yield return new WaitForSeconds(spawnDelay);

        Debug.LogError("[LoadoutManager] 2. Delay finished. Checking MRUK...");

        // Try to wait for MRUK, increased timeout to 10 seconds for better reliability
        float timeout = 10.0f;
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
            Debug.LogError("[LoadoutManager] 3a. MRUK Ready with validated floor. Logging furniture diagnostics...");
            LogMRUKFurnitureDiagnostics(MRUK.Instance.GetCurrentRoom());
            
            // Check if user wants camera-relative spawn as primary method
            if (useCameraRelativeSpawn)
            {
                Debug.LogError("[LoadoutManager] 3b. Using camera-relative spawn (user preference).");
                if (TryCameraRelativeGuaranteedSpawn(out Vector3 gunPos, out Vector3 flashlightPos))
                {
                    SpawnAt(dummyGunPrefab, gunPos, Vector3.up, SimplePickup.ItemType.Gun);
                    SpawnAt(dummyFlashlightPrefab, flashlightPos, Vector3.up, SimplePickup.ItemType.Flashlight);
                }
                else
                {
                    Debug.LogError("[LoadoutManager] Camera-relative spawn failed! Falling back to MRUK room spawn.");
                    SpawnItemsInRoom(MRUK.Instance.GetCurrentRoom());
                }
            }
            else
            {
                Debug.LogError("[LoadoutManager] 3b. Spawning on floor using MRUK room detection.");
                SpawnItemsInRoom(MRUK.Instance.GetCurrentRoom());
            }
        }
        else
        {
            Debug.LogError("[LoadoutManager] 3b. MRUK timed out or no valid floor found. Using Fallback Spawning.");
            SpawnFallback();
        }
    }

    void SpawnFallback()
    {
        Debug.LogError("[LoadoutManager] === SPAWNING CAMERA FALLBACK (Last Resort) ===");
        
        if (Camera.main == null)
        {
             Debug.LogError("[LoadoutManager] CRITICAL: Camera.main is NULL! Cannot find spawn position.");
             return;
        }

        // Spawn at floor level in front of player (better than chest level)
        // Get floor Y position from camera Y minus typical player height
        float estimatedFloorY = Camera.main.transform.position.y - 1.6f; // Assume 1.6m player height
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0; // Keep horizontal only
        forward.Normalize();
        
        Vector3 spawnCenter = Camera.main.transform.position;
        spawnCenter.y = estimatedFloorY;
        spawnCenter += forward * 1.0f; // 1 meter in front
        
        Debug.LogError($"[LoadoutManager] Camera fallback center: {spawnCenter}");

        if (dummyGunPrefab != null)
        {
            Vector3 gunPos = spawnCenter + Vector3.right * 0.5f;
            Debug.LogError($"[LoadoutManager] Camera Fallback: Instantiating Dummy Gun at {gunPos}");
            GameObject g = Instantiate(dummyGunPrefab, gunPos, Quaternion.identity);
            
            var p = g.GetComponent<SimplePickup>(); 
            if (p == null) p = g.AddComponent<SimplePickup>();
            p.itemType = SimplePickup.ItemType.Gun;
            
            var rb = g.GetComponent<Rigidbody>();
            if (rb != null) 
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }
            
            Debug.LogError($"[LoadoutManager] Camera Fallback: Gun spawned! {g.name}");
        }
        else
        {
            Debug.LogError("[LoadoutManager] CRITICAL: Dummy Gun Prefab is NULL!");
        }

        if (dummyFlashlightPrefab != null)
        {
            Vector3 flashlightPos = spawnCenter - Vector3.right * 0.5f;
            Debug.LogError($"[LoadoutManager] Camera Fallback: Instantiating Dummy Flashlight at {flashlightPos}");
            GameObject f = Instantiate(dummyFlashlightPrefab, flashlightPos, Quaternion.identity);
            
            var p = f.GetComponent<SimplePickup>();
            if (p == null) p = f.AddComponent<SimplePickup>();
            p.itemType = SimplePickup.ItemType.Flashlight;
            
            var rb = f.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }
            
            Debug.LogError($"[LoadoutManager] Camera Fallback: Flashlight spawned! {f.name}");
        }
        else
        {
            Debug.LogError("[LoadoutManager] CRITICAL: Dummy Flashlight Prefab is NULL!");
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
            Debug.LogError($"[LoadoutManager] SUCCESS! Found floor spawn pair. Gun: {gunPos}, Flashlight: {flashlightPos}");
            SpawnAt(dummyGunPrefab, gunPos, gunNormal, SimplePickup.ItemType.Gun);
            SpawnAt(dummyFlashlightPrefab, flashlightPos, flashlightNormal, SimplePickup.ItemType.Flashlight);
        }
        else
        {
            // If MRUK is "ready" but we still can't find positions, fall back to room bounds center
            Debug.LogWarning("[LoadoutManager] Failed to find valid spawn pair inside room. Using room-bounds fallback.");
            if (TryGetRoomBoundsFallbackPair(room, out gunPos, out flashlightPos))
            {
                Debug.LogError($"[LoadoutManager] Room-bounds fallback SUCCESS! Gun: {gunPos}, Flashlight: {flashlightPos}");
                // Bounds fallback doesn't have a reliable floor normal; assume up.
                SpawnAt(dummyGunPrefab, gunPos, Vector3.up, SimplePickup.ItemType.Gun);
                SpawnAt(dummyFlashlightPrefab, flashlightPos, Vector3.up, SimplePickup.ItemType.Flashlight);
            }
            else
            {
                // Try camera-relative guaranteed spawn (always works, always visible)
                Debug.LogWarning("[LoadoutManager] Room-bounds fallback FAILED! Trying camera-relative guaranteed spawn...");
                if (TryCameraRelativeGuaranteedSpawn(out gunPos, out flashlightPos))
                {
                    Debug.LogError($"[LoadoutManager] Camera-relative spawn SUCCESS! Gun: {gunPos}, Flashlight: {flashlightPos}");
                    SpawnAt(dummyGunPrefab, gunPos, Vector3.up, SimplePickup.ItemType.Gun);
                    SpawnAt(dummyFlashlightPrefab, flashlightPos, Vector3.up, SimplePickup.ItemType.Flashlight);
                }
                else
                {
                    // Absolute last resort: existing camera fallback
                    Debug.LogError("[LoadoutManager] Camera-relative spawn FAILED! Using basic camera fallback.");
                    SpawnFallback();
                }
            }
        }
    }
    
    void LogMRUKFurnitureDiagnostics(MRUKRoom room)
    {
        if (room == null || room.Anchors == null)
        {
            Debug.LogError("[LoadoutManager] DIAGNOSTIC: Room or Anchors is NULL!");
            return;
        }
        
        Debug.LogError($"[LoadoutManager] ===== MRUK FURNITURE DIAGNOSTICS =====");
        Debug.LogError($"[LoadoutManager] Total anchors found: {room.Anchors.Count}");
        
        int furnitureCount = 0;
        foreach (var anchor in room.Anchors)
        {
            if (anchor == null) continue;
            
            string labels = anchor.Label.ToString();
            Vector3 pos = anchor.transform.position;
            Vector3 scale = anchor.transform.lossyScale;
            
            bool isFurniture = anchor.Label.HasFlag(MRUKAnchor.SceneLabels.BED) ||
                              anchor.Label.HasFlag(MRUKAnchor.SceneLabels.COUCH) ||
                              anchor.Label.HasFlag(MRUKAnchor.SceneLabels.TABLE) ||
                              anchor.Label.HasFlag(MRUKAnchor.SceneLabels.OTHER);
            
            if (isFurniture)
            {
                furnitureCount++;
                Debug.LogError($"[LoadoutManager] FURNITURE #{furnitureCount}: Label={labels}, Pos=({pos.x:F2}, {pos.y:F2}, {pos.z:F2}), Scale=({scale.x:F2}, {scale.y:F2}, {scale.z:F2})");
            }
            else
            {
                Debug.LogError($"[LoadoutManager] Anchor: Label={labels}, Pos=({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
            }
        }
        
        Debug.LogError($"[LoadoutManager] Total furniture anchors: {furnitureCount}");
        Debug.LogError($"[LoadoutManager] Min furniture distance setting: {minFurnitureDistance}m");
        Debug.LogError($"[LoadoutManager] ========================================");
    }
    
    bool TryCameraRelativeGuaranteedSpawn(out Vector3 gunPos, out Vector3 flashlightPos)
    {
        gunPos = Vector3.zero;
        flashlightPos = Vector3.zero;
        
        if (Camera.main == null)
        {
            Debug.LogError("[LoadoutManager] Camera.main is NULL! Cannot use camera-relative spawn.");
            return false;
        }
        
        // Get camera position and forward direction
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f; // Keep horizontal only
        cameraForward.Normalize();
        
        // Use standard floor height (0.0f) or MRUK floor if available
        float floorY = 0f;
        if (MRUK.Instance != null && MRUK.Instance.IsInitialized)
        {
            var room = MRUK.Instance.GetCurrentRoom();
            if (room != null && room.Anchors != null)
            {
                foreach (var anchor in room.Anchors)
                {
                    if (anchor.Label.HasFlag(MRUKAnchor.SceneLabels.FLOOR))
                    {
                        floorY = anchor.transform.position.y;
                        break;
                    }
                }
            }
        }
        
        // Spawn 1.2m in front of player
        Vector3 spawnCenter = cameraPos;
        spawnCenter.y = floorY; // Use verified floor height
        spawnCenter += cameraForward * 1.2f;
        
        // Place items side-by-side
        Vector3 right = Vector3.Cross(cameraForward, Vector3.up).normalized;
        
        gunPos = spawnCenter + right * 0.4f;
        flashlightPos = spawnCenter - right * 0.4f;
        
        // Ensure both positions are at floor level
        gunPos.y = floorY;
        flashlightPos.y = floorY;
        
        Debug.LogError($"[LoadoutManager] Camera-relative spawn positions calculated:");
        Debug.LogError($"[LoadoutManager]   Camera: {cameraPos}");
        Debug.LogError($"[LoadoutManager]   FloorY: {floorY}");
        Debug.LogError($"[LoadoutManager]   Gun: {gunPos}");
        Debug.LogError($"[LoadoutManager]   Flashlight: {flashlightPos}");
        
        return true;
    }

    void SpawnAt(GameObject prefab, Vector3 floorPos, Vector3 floorNormal, SimplePickup.ItemType type)
    {
        if (prefab == null)
        {
            Debug.LogError($"[LoadoutManager] SpawnAt FAILED! Prefab is NULL for {type}");
            return;
        }
        
        Debug.LogError($"[LoadoutManager] SpawnAt called for {type} at position {floorPos}");

        // Compute a natural "resting" rotation on the floor
        Quaternion restRot = ComputeRestRotation(floorNormal, type);

        // Place slightly above the floor based on collider bounds so it doesn't clip
        Vector3 placePos = ComputeRestingPosition(prefab, floorPos, floorNormal, restRot, type);

        // Debug: Show final spawn location
        if (showFinalSpawns)
        {
            string itemName = type == SimplePickup.ItemType.Gun ? "GUN" : "FLASHLIGHT";
            Color sphereColor = type == SimplePickup.ItemType.Gun ? Color.green : Color.cyan;
            float sphereSize = type == SimplePickup.ItemType.Gun ? 0.12f : 0.15f;  // Make flashlight sphere bigger
            CreateDebugSphere(placePos, sphereSize, sphereColor, 30f, itemName);
        }

        GameObject obj = Instantiate(prefab, placePos, restRot);
        Debug.LogError($"[LoadoutManager] Instantiated {type} successfully! Object: {obj.name}, Position: {obj.transform.position}");

        // Force enable the GameObject and all children
        obj.SetActive(true);
        foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.SetActive(true);
        }
        
        // Force enable all renderers to ensure visibility
        MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>(true);
        Debug.LogError($"[LoadoutManager] {type} has {renderers.Length} MeshRenderers");
        foreach (var r in renderers)
        {
            r.enabled = true;
            r.gameObject.SetActive(true);
            Debug.LogError($"[LoadoutManager] Enabled renderer on: {r.gameObject.name}, Visible: {r.isVisible}");
        }
        
        // Validate scale
        Debug.LogError($"[LoadoutManager] {type} Scale: Local={obj.transform.localScale}, World={obj.transform.lossyScale}");

        // Disable gravity so items don't fall through floor (pickup-only flow)
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            Debug.LogError($"[LoadoutManager] {type} Rigidbody configured: useGravity=false, isKinematic=true");
        }
        else
        {
            Debug.LogWarning($"[LoadoutManager] {type} has NO Rigidbody component!");
        }

        // Setup SimplePickup component
        SimplePickup pickup = obj.GetComponent<SimplePickup>();
        if (pickup == null) pickup = obj.AddComponent<SimplePickup>();
        pickup.itemType = type;
        Debug.LogError($"[LoadoutManager] {type} SimplePickup component configured");

        if (spawnEffectPrefab != null) Instantiate(spawnEffectPrefab, placePos, Quaternion.identity);

        if (useSettleAnimation)
        {
            // Start slightly higher and settle down for a more natural look
            Vector3 startPos = placePos + floorNormal.normalized * Mathf.Max(0f, settleHeight);
            obj.transform.position = startPos;
            StartCoroutine(SettleToPose(obj.transform, placePos, restRot, floorNormal));
        }
        
        Debug.LogError($"[LoadoutManager] ===== {type} SPAWN COMPLETE! Final position: {obj.transform.position} =====");
    }

    bool TryGetSpawnPairInRoom(MRUKRoom room, out Vector3 gunPos, out Vector3 gunNormal, out Vector3 flashlightPos, out Vector3 flashlightNormal)
    {
        gunPos = Vector3.zero;
        gunNormal = Vector3.up;
        flashlightPos = Vector3.zero;
        flashlightNormal = Vector3.up;
        if (room == null) return false;

        // Try with strict furniture avoidance first (using default minFurnitureDistance, e.g. 0.8m)
        Debug.LogError($"[LoadoutManager] Attempting paired spawn with furniture avoidance (maxPairAttempts={maxPairAttempts})...");
        for (int pairAttempt = 0; pairAttempt < maxPairAttempts; pairAttempt++)
        {
            if (!TryGetFloorSpawnPosition(room, out gunPos, out gunNormal, true, -1f)) continue;
            if (!TryGetFloorSpawnPosition(room, out flashlightPos, out flashlightNormal, true, -1f)) continue;

            float distance = Vector3.Distance(gunPos, flashlightPos);
            if (distance < itemSpacing) continue; // Too close
            if (distance > maxItemDistance) continue; // Too far apart

            // Both positions valid and far enough apart
            Debug.LogError($"[LoadoutManager] STRICT spawn SUCCESS on attempt {pairAttempt + 1}!");
            return true;
        }

        // If strict validation failed, try again with RELAXED furniture checking (half distance)
        // This is safer than disabling checks entirely, which causes spawns under beds
        float relaxedDistance = minFurnitureDistance * 0.5f;
        Debug.LogWarning($"[LoadoutManager] Strict spawn failed after all attempts. Trying with RELAXED furniture distance ({relaxedDistance:F2}m)...");
        
        for (int pairAttempt = 0; pairAttempt < maxPairAttempts; pairAttempt++)
        {
            if (!TryGetFloorSpawnPosition(room, out gunPos, out gunNormal, true, relaxedDistance)) continue;
            if (!TryGetFloorSpawnPosition(room, out flashlightPos, out flashlightNormal, true, relaxedDistance)) continue;

            float distance = Vector3.Distance(gunPos, flashlightPos);
            if (distance < itemSpacing) continue; // Too close
            if (distance > maxItemDistance) continue; // Too far apart

            // Both positions valid and far enough apart
            Debug.LogError($"[LoadoutManager] RELAXED spawn SUCCESS on attempt {pairAttempt + 1}!");
            return true;
        }

        Debug.LogError("[LoadoutManager] BOTH spawn methods failed completely! (Strict + Relaxed)");
        return false;
    }

    bool TryGetFloorSpawnPosition(MRUKRoom room, out Vector3 floorPos, out Vector3 floorNormal, bool checkFurniture = true, float furnitureDistanceOverride = -1f)
    {
        floorPos = Vector3.zero;
        floorNormal = Vector3.up;
        if (room == null) return false;

        // Prefer spawning on the FLOOR surface (ground floor). This avoids tables/couches/etc.
        for (int i = 0; i < maxAttempts; i++)
        {
            bool found = room.GenerateRandomPositionOnSurface(
                MRUK.SurfaceType.FACING_UP,
                wallDistance,
                new LabelFilter(MRUKAnchor.SceneLabels.FLOOR),
                out Vector3 pos,
                out Vector3 norm
            );

            if (!found) continue;
            
            // Debug: Show attempted position
            if (showAttempts && i % 20 == 0) // Only show every 20th attempt to avoid spam
            {
                CreateDebugSphere(pos, 0.05f, Color.yellow, 5f);
            }
            
            if (!room.IsPositionInRoom(pos, true))
            {
                if (showAttempts && i % 20 == 0)
                {
                    CreateDebugSphere(pos, 0.05f, Color.red, 5f); // Red = outside room
                }
                continue;
            }

            floorPos = pos;
            floorNormal = norm.sqrMagnitude > 0.0001f ? norm.normalized : Vector3.up;
            
            // Only check furniture if requested
            if (checkFurniture)
            {
                // Use override distance if provided, otherwise default
                float dist = furnitureDistanceOverride > 0f ? furnitureDistanceOverride : minFurnitureDistance;

                // Check 1: MRUK semantic furniture labels (catches beds/couches without colliders)
                if (!IsClearOfMRUKFurniture(room, floorPos, dist))
                {
                    if (showAttempts && i % 20 == 0)
                    {
                        CreateDebugSphere(pos, 0.05f, new Color(1f, 0f, 1f), 5f); // Magenta = near MRUK furniture
                    }
                    continue;
                }
                
                // Check 2: Physics collider overlap (traditional furniture detection)
                if (!IsVerticalClear(floorPos, floorNormal))
                {
                    if (showAttempts && i % 20 == 0)
                    {
                        CreateDebugSphere(pos, 0.05f, new Color(1f, 0.5f, 0f), 5f); // Orange = blocked by collider
                    }
                    continue;
                }
            }
            
            // Success! Show cyan sphere
            if (showAttempts)
            {
                CreateDebugSphere(pos, 0.08f, Color.cyan, 10f);
            }
            
            return true;
        }

        // Fallback: any point in room volume (still inside room), then lift slightly.
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3? posOpt = room.GenerateRandomPositionInRoom(edgeBuffer, true);
            if (!posOpt.HasValue) continue;
            Vector3 pos = posOpt.Value;
            if (!room.IsPositionInRoom(pos, true)) continue;
            
            floorPos = pos;
            floorNormal = Vector3.up;
            
            // Apply same furniture checks to fallback positions
            if (checkFurniture)
            {
                float dist = furnitureDistanceOverride > 0f ? furnitureDistanceOverride : minFurnitureDistance;
                if (!IsClearOfMRUKFurniture(room, floorPos, dist)) continue;
                if (!IsVerticalClear(floorPos, floorNormal)) continue;
            }
            
            return true;
        }

        return false;
    }

    bool TryGetRoomBoundsFallbackPair(MRUKRoom room, out Vector3 gunPos, out Vector3 flashlightPos)
    {
        gunPos = Vector3.zero;
        flashlightPos = Vector3.zero;
        if (room == null) return false;

        // Try to get actual floor height from MRUK floor anchor
        float floorY = 0f;
        bool foundFloor = false;
        
        foreach (var anchor in room.Anchors)
        {
            if (anchor.Label.HasFlag(MRUKAnchor.SceneLabels.FLOOR))
            {
                floorY = anchor.transform.position.y;
                foundFloor = true;
                Debug.LogWarning($"[LoadoutManager] Room-Bounds Fallback using floor Y: {floorY}");
                break;
            }
        }
        
        if (!foundFloor)
        {
            Bounds b = room.GetRoomBounds();
            floorY = b.min.y + 0.1f; // Fallback: slightly above room bottom
            Debug.LogWarning($"[LoadoutManager] No floor anchor found, using room bounds min + 0.1m: {floorY}");
        }

        // Put them near the center, offset left/right
        Bounds bounds = room.GetRoomBounds();
        if (bounds.size.sqrMagnitude <= 0.001f) return false;
        
        Vector3 center = bounds.center;

        gunPos = new Vector3(center.x + 0.3f, floorY, center.z);
        flashlightPos = new Vector3(center.x - 0.3f, floorY, center.z);

        // Check if these positions are clear of furniture
        if (!IsClearOfMRUKFurniture(room, gunPos, minFurnitureDistance) || !IsClearOfMRUKFurniture(room, flashlightPos, minFurnitureDistance))
        {
            Debug.LogWarning("[LoadoutManager] Room-bounds fallback positions are too close to furniture!");
            return false;
        }

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
        
        // Do multiple checks at different heights to catch beds, tables, and furniture
        // Check 1: Low check (item height)
        Vector3 center1 = floorPos + n * (verticalClearance * 0.3f);
        Vector3 halfExtents1 = new Vector3(horizontalClearance, verticalClearance * 0.3f, horizontalClearance);
        
        // Check 2: Mid check (typical furniture edge height)
        Vector3 center2 = floorPos + n * (verticalClearance * 0.65f);
        Vector3 halfExtents2 = new Vector3(horizontalClearance * 0.8f, verticalClearance * 0.35f, horizontalClearance * 0.8f);
        
        Collider[] hits1 = Physics.OverlapBox(center1, halfExtents1, Quaternion.identity, furnitureLayers, QueryTriggerInteraction.Ignore);
        Collider[] hits2 = Physics.OverlapBox(center2, halfExtents2, Quaternion.identity, furnitureLayers, QueryTriggerInteraction.Ignore);
        
        // Combine all hits
        List<Collider> allHits = new List<Collider>();
        if (hits1 != null) allHits.AddRange(hits1);
        if (hits2 != null) allHits.AddRange(hits2);
        
        if (allHits.Count == 0) return true;
        
        // Filter out floor colliders (very close to floor level)
        foreach (var h in allHits)
        {
            if (h == null) continue;
            
            // Allow the floor mesh itself if it's AT floor level
            if (h.bounds.center.y < floorPos.y + 0.02f && h.bounds.max.y < floorPos.y + 0.05f) 
                continue;
            
            // Anything else = blocked (likely furniture)
            if (showAttempts)
            {
                Debug.LogWarning($"[LoadoutManager] Spawn blocked by: {h.gameObject.name} at {h.bounds.center}");
            }
            return false;
        }
        
        return true;
    }

    bool IsClearOfMRUKFurniture(MRUKRoom room, Vector3 floorPos, float minDistance)
    {
        if (room == null || room.Anchors == null) return true;
        
        // Check distance to all furniture anchors (beds, couches, tables)
        foreach (var anchor in room.Anchors)
        {
            // Check if this is furniture we should avoid
            if (anchor.Label.HasFlag(MRUKAnchor.SceneLabels.BED) ||
                anchor.Label.HasFlag(MRUKAnchor.SceneLabels.COUCH) ||
                anchor.Label.HasFlag(MRUKAnchor.SceneLabels.TABLE) ||
                anchor.Label.HasFlag(MRUKAnchor.SceneLabels.OTHER))
            {
                // Try to get anchor bounds for more accurate checking
                float distanceToFurniture = GetDistanceToFurnitureAnchor(anchor, floorPos);
                
                if (distanceToFurniture < minDistance)
                {
                    if (showAttempts)
                    {
                        Debug.LogWarning($"[LoadoutManager] Too close to {anchor.Label} furniture at {anchor.transform.position} (distance: {distanceToFurniture:F2}m, required: {minDistance:F2}m)");
                    }
                    return false;
                }
            }
        }
        
        return true;
    }
    
    float GetDistanceToFurnitureAnchor(MRUKAnchor anchor, Vector3 floorPos)
    {
        if (anchor == null) return float.MaxValue;
        
        // Try to use anchor bounds if available (more accurate)
        // MRUK anchors have a Bounds property that represents the anchor's volume
        try
        {
            // Check if we can access bounds through reflection or direct property
            // MRUKAnchor might have bounds or we can estimate from transform scale
            Vector3 anchorPos = anchor.transform.position;
            Vector3 anchorScale = anchor.transform.lossyScale;
            
            // Estimate anchor size from scale (if available)
            // For furniture, we care about horizontal distance (X/Z), not vertical (Y)
            float estimatedRadius = Mathf.Max(anchorScale.x, anchorScale.z) * 0.5f;
            
            // Calculate horizontal distance (ignore Y difference)
            Vector3 anchorPos2D = new Vector3(anchorPos.x, floorPos.y, anchorPos.z);
            float horizontalDistance = Vector3.Distance(floorPos, anchorPos2D);
            
            // Subtract estimated radius to get distance to edge
            float distanceToEdge = horizontalDistance - estimatedRadius;
            
            // If anchor has no scale info, fall back to center distance
            if (estimatedRadius < 0.1f)
            {
                return horizontalDistance;
            }
            
            return Mathf.Max(0f, distanceToEdge);
        }
        catch
        {
            // Fallback: simple distance to center
            Vector3 anchorPos = anchor.transform.position;
            Vector3 anchorPos2D = new Vector3(anchorPos.x, floorPos.y, anchorPos.z);
            return Vector3.Distance(floorPos, anchorPos2D);
        }
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
        float yaw = Random.Range(-randomRotation, randomRotation);
        Quaternion yawLocal = Quaternion.AngleAxis(yaw, Vector3.up); // in floorFrame space

        // Per-item "rest" rotation applied in floorFrame space (this is what you tweak)
        Vector3 restEuler = type == SimplePickup.ItemType.Gun ? gunFloorRotation : flashlightFloorRotation;
        Quaternion restLocal = Quaternion.Euler(restEuler);

        // Small random tilt for realism (in floorFrame space)
        float tiltX = Random.Range(-randomTilt, randomTilt);
        float tiltZ = Random.Range(-randomTilt, randomTilt);
        Quaternion tiltLocal = Quaternion.Euler(tiltX, 0f, tiltZ);

        return floorFrame * yawLocal * restLocal * tiltLocal;
    }

    Vector3 ComputeRestingPosition(GameObject prefab, Vector3 floorPos, Vector3 floorNormal, Quaternion rotation, SimplePickup.ItemType type)
    {
        // Use per-item vertical offset to adjust height above floor
        // This allows fine-tuning so handles/grips rest naturally on the floor
        float verticalOffset = type == SimplePickup.ItemType.Gun ? gunFloorHeight : flashlightFloorHeight;
        Vector3 n = (floorNormal.sqrMagnitude > 0.0001f) ? floorNormal.normalized : Vector3.up;
        return floorPos + n * verticalOffset;
    }

    IEnumerator SettleToPose(Transform t, Vector3 targetPos, Quaternion targetRot, Vector3 floorNormal)
    {
        if (t == null) yield break;
        float dur = Mathf.Max(0.01f, settleSpeed);
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
        
        // Make it bright and visible with transparency
        Renderer r = sphere.GetComponent<Renderer>();
        if (r != null)
        {
            // Try to find a transparent shader, fallback to standard if not found
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            
            r.material = new Material(shader);
            
            // Make semi-transparent
            color.a = 0.5f; // 50% transparent
            r.material.color = color;
            
            // Enable transparency
            r.material.SetFloat("_Surface", 1); // Transparent mode
            r.material.SetFloat("_Blend", 0); // Alpha blend
            r.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            r.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            r.material.SetInt("_ZWrite", 0);
            r.material.renderQueue = 3000;
            r.material.SetFloat("_Smoothness", 0.8f);
        }
        
        // Remove collider so it doesn't interfere
        Collider c = sphere.GetComponent<Collider>();
        if (c != null) Destroy(c);
        
        // Auto-destroy after lifetime
        Destroy(sphere, lifetime);
    }

    /// <summary>
    /// Hides Meta Quest controller models to prevent overlap with gun/flashlight models.
    /// Handles both traditional controller anchors and Building Block controllers.
    /// </summary>
    void HideControllerModels()
    {
        if (controllersHidden) return; // Already hidden

        bool foundAny = false;

        // Method 1: Find controller anchors (traditional approach)
        GameObject leftAnchor = GameObject.Find("LeftControllerAnchor");
        GameObject rightAnchor = GameObject.Find("RightControllerAnchor");

        if (leftAnchor != null)
        {
            HideRenderersInHierarchy(leftAnchor);
            foundAny = true;
        }

        if (rightAnchor != null)
        {
            HideRenderersInHierarchy(rightAnchor);
            foundAny = true;
        }

        // Method 2: Find Building Block controller tracking GameObjects
        GameObject leftBB = GameObject.Find("[BuildingBlock] Controller Tracking Left");
        GameObject rightBB = GameObject.Find("[BuildingBlock] Controller Tracking Right");

        if (leftBB != null)
        {
            HideRenderersInHierarchy(leftBB);
            foundAny = true;
        }

        if (rightBB != null)
        {
            HideRenderersInHierarchy(rightBB);
            foundAny = true;
        }

        // Method 3: Search for controller models by common name patterns
        // This catches controller models that might be nested differently
        Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (Transform t in allTransforms)
        {
            if (t == null) continue;
            string name = t.name;
            
            // Look for controller model names (common patterns)
            if (name.Contains("controller_model") || 
                name.Contains("ControllerModel") ||
                name.Contains("OVRControllerPrefab") ||
                name.Contains("TouchControllerModel"))
            {
                HideRenderersInHierarchy(t.gameObject);
                foundAny = true;
            }
        }

        controllersHidden = true;
        if (foundAny)
        {
            Debug.Log("[LoadoutManager] Controller models hidden to prevent overlap with equipped items.");
        }
        else
        {
            Debug.LogWarning("[LoadoutManager] No controller models found to hide. Controllers may use a different structure.");
            Debug.LogWarning("[LoadoutManager] Searched for: LeftControllerAnchor, RightControllerAnchor, [BuildingBlock] Controller Tracking Left/Right");
        }
    }

    /// <summary>
    /// Hides renderers in controller hierarchy, skipping weapon objects (gun/flashlight).
    /// Also disables GameObjects that look like controller models (for Building Block controllers).
    /// </summary>
    void HideRenderersInHierarchy(GameObject parent)
    {
        if (parent == null) return;

        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>(true);
        int hiddenCount = 0;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;

            // Skip if this looks like a weapon (gun, flashlight) - we want these visible
            if (IsWeaponRenderer(renderer.gameObject))
            {
                continue;
            }

            // Hide controller model renderers
            if (renderer.enabled)
            {
                renderer.enabled = false;
                hiddenCount++;
            }
        }

        // Also disable GameObjects that look like controller models (for Building Block controllers)
        // These might not use standard Renderer components
        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child == null || child == parent.transform) continue;

            // Skip if this looks like a weapon
            if (IsWeaponRenderer(child.gameObject))
            {
                continue;
            }

            string childName = child.name.ToLower();
            // Check if this looks like a controller model GameObject
            if ((childName.Contains("controller_model") || 
                 childName.Contains("controllermodel") ||
                 childName.Contains("ovrcontroller") ||
                 childName.Contains("touchcontroller")) &&
                child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                hiddenCount++;
            }
        }

        if (hiddenCount > 0)
        {
            Debug.Log($"[LoadoutManager] Hidden {hiddenCount} renderer(s)/GameObject(s) in {parent.name} hierarchy");
        }
    }

    /// <summary>
    /// Checks if a GameObject is part of a weapon (gun/flashlight) hierarchy.
    /// </summary>
    bool IsWeaponRenderer(GameObject obj)
    {
        if (obj == null) return false;

        // Check if this object or its parents contain weapon keywords
        string name = obj.name.ToLower();

        if (name.Contains("gun") ||
            name.Contains("weapon") ||
            name.Contains("flashlight") ||
            name.Contains("revolver") ||
            name.Contains("pistol"))
        {
            return true;
        }

        // Check parent hierarchy
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            string parentName = parent.name.ToLower();
            if (parentName.Contains("gun") ||
                parentName.Contains("weapon") ||
                parentName.Contains("flashlight") ||
                parentName.Contains("revolver") ||
                parentName.Contains("pistol"))
            {
                return true;
            }
            parent = parent.parent;
        }

        return false;
    }
}
