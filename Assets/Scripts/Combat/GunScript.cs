using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunScript : MonoBehaviour
{
    [Header("Gun Settings")]
    public LayerMask layerMask;
    public OVRInput.RawButton shootingButton;
    public Transform shootingPoint;
    public float maxLineDistance = 5;
    public float projectileLifetime = 2f;
    
    [Header("Shooting Mode")]
    [Tooltip("Use projectile mode (shoots sphere) or laser mode (shoots line)")]
    public bool useProjectileMode = true;
    
    [Header("Projectile Mode")]
    [Tooltip("Your Specter Ammo sphere prefab")]
    public GameObject projectilePrefab;
    [Tooltip("Speed of projectile (launch velocity)")]
    public float projectileSpeed = 25f;
    [Tooltip("Gravity multiplier (1 = normal gravity, higher = more arc, lower = straighter shots)")]
    [Range(0f, 2f)]
    public float gravityMultiplier = 0.3f;
    [Tooltip("If true, wall destruction happens on projectile hit (not instant)")]
    public bool delayWallDestruction = true;
    
    [Header("Laser Mode (Legacy)")]
    public LineRenderer linePrefab;
    public float lineShowTimer = 0.3f;
    
    [Header("Impact Effects")]
    public GameObject rayImpactPrefab;
    
    [Header("Audio")]
    public AudioSource source;
    public AudioClip shootingAudioClip;

    [Header("Events")]
    public UnityEvent OnShoot;
    public UnityEvent<GameObject> OnShootAndHit;
    public UnityEvent OnShootAndMiss;
    
    [Header("Debug")]
    [Tooltip("Enable verbose debug logging (disable for better performance) - KEEP THIS OFF IN BUILDS!")]
    public bool enableDebugLogging = false;
    
    [Header("Equip / Safety")]
    [Tooltip("If true, the gun will only shoot after LoadoutManager equips it (recommended for this project).")]
    public bool requireEquippedToShoot = true;
    private bool isEquipped = false;

    public void SetEquipped(bool equipped)
    {
        isEquipped = equipped;
    }

    // Helper to check if gun is currently held (Universal Safe Version)
    public bool IsHeld()
    {
        // 1. Check OVRGrabbable
        var ovr = GetComponent<OVRGrabbable>();
        if (ovr != null && ovr.isGrabbed) return true;

        // 2. Check Parent (Universal for ISDK / XRI / Custom)
        // If parent is null, it's definitely not held (unless it's a root object, which guns usually aren't when held)
        if (transform.parent == null) return false;

        string pName = transform.parent.name.ToLower();
        if (pName.Contains("hand") || pName.Contains("controller") || pName.Contains("interactor") || pName.Contains("grab"))
        {
            return true;
        }

        // Default: If it has no parent or weird parent, assume NOT held to be safe
        // UNLESS it has no Rigidbody, in which case it might be static/testing
        if (GetComponent<Rigidbody>() == null) return true;

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        // Equip gate: in our "dummy pickup -> equip in hand" flow, this is the authoritative check.
        if (requireEquippedToShoot && !isEquipped) return;

        // Safety Catch: Don't shoot if not held
        // If you're using the snap-to-mount approach, parent name checks can be wrong; keep this only when not using equip gating.
        if (!requireEquippedToShoot && !IsHeld()) return;

        if (OVRInput.GetDown(shootingButton))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        if (enableDebugLogging) Debug.Log("[GunScript] ===== SHOOT TRIGGERED =====");
        OnShoot.Invoke();

        // Play audio with proper null check
        if (source != null && shootingAudioClip != null)
        {
            source.PlayOneShot(shootingAudioClip);
        }

        if (shootingPoint == null)
        {
            Debug.LogError("[GunScript] ShootingPoint is NULL! Cannot shoot.");
            return;
        }

        // For projectile mode with delayed wall destruction, we don't do instant raycast
        if (useProjectileMode && delayWallDestruction)
        {
            // Just spawn the projectile - it will handle all hit detection
            CreateProjectile(shootingPoint.position, shootingPoint.forward, maxLineDistance);
            return;
        }

        // Original instant raycast behavior (for laser mode or instant wall destruction)
        Ray ray = new Ray(shootingPoint.position, shootingPoint.forward);
        if (enableDebugLogging)
        {
            Debug.Log($"[GunScript] Raycast: From {shootingPoint.position}, Direction {shootingPoint.forward}, MaxDistance {maxLineDistance}, LayerMask {layerMask.value} (binary: {System.Convert.ToString(layerMask.value, 2)})");
            // Draw debug ray in scene view
            Debug.DrawRay(shootingPoint.position, shootingPoint.forward * maxLineDistance, Color.red, 2f);
        }
        
        bool hasHit = Physics.Raycast(ray, out RaycastHit hit, maxLineDistance, layerMask);

        Vector3 endPoint = Vector3.zero;

        if (hasHit)
        {
            endPoint = hit.point;
            GameObject hitObject = hit.collider.gameObject;
            
            if (enableDebugLogging)
            {
                Debug.Log($"[GunScript] ✓✓✓ HIT DETECTED ✓✓✓");
                Debug.Log($"[GunScript] Hit Object: {hitObject.name}");
                Debug.Log($"[GunScript] Hit Layer: {hitObject.layer} ({LayerMask.LayerToName(hitObject.layer)})");
                Debug.Log($"[GunScript] Hit Point: {hit.point}");
                Debug.Log($"[GunScript] Hit Collider: {hit.collider.GetType().Name} (Enabled: {hit.collider.enabled})");
                Debug.Log($"[GunScript] Has MeshCollider: {hit.collider is MeshCollider}");
                if (hit.collider is MeshCollider)
                {
                    MeshCollider mc = hit.collider as MeshCollider;
                    Debug.Log($"[GunScript] MeshCollider Convex: {mc.convex}, Has Mesh: {mc.sharedMesh != null}");
                }
                Debug.Log($"[GunScript] Invoking OnShootAndHit event with: {hitObject.name}");
                Debug.Log($"[GunScript] OnShootAndHit listener count: {OnShootAndHit.GetPersistentEventCount()}");
            }
            
            OnShootAndHit.Invoke(hitObject);
            
            if (enableDebugLogging) Debug.Log($"[GunScript] OnShootAndHit event invoked!");

            // Check for ghost component more efficiently
            Ghost ghost = hit.collider.GetComponent<Ghost>();
            if (ghost == null)
            {
                ghost = hit.collider.GetComponentInParent<Ghost>();
            }

            if (ghost != null)
            {
                ghost.Kill();
            }
            else
            {
                // Create impact effect with object pooling if available
                CreateImpactEffect(hit.point, hit.normal);
            }
        }
        else
        {
            endPoint = shootingPoint.position + shootingPoint.forward * maxLineDistance;
            
            if (enableDebugLogging)
            {
                Debug.Log($"[GunScript] No hit detected. Ray from {shootingPoint.position} direction {shootingPoint.forward}, distance {maxLineDistance}, layerMask: {layerMask.value}");
                
                // Try a raycast without layer mask to see if there's anything in the way
                if (Physics.Raycast(ray, out RaycastHit debugHit, maxLineDistance))
                {
                    Debug.LogWarning($"[GunScript] Found object {debugHit.collider.gameObject.name} on layer {debugHit.collider.gameObject.layer} but it's not in layerMask!");
                }
            }
            
            OnShootAndMiss.Invoke();
        }

        // Create visual effect (projectile or laser line)
        if (useProjectileMode)
        {
            Vector3 direction = (endPoint - shootingPoint.position).normalized;
            CreateProjectile(shootingPoint.position, direction, maxLineDistance);
        }
        else
        {
            CreateLaserLine(shootingPoint.position, endPoint);
        }
    }

    private void CreateImpactEffect(Vector3 position, Vector3 normal)
    {
        if (rayImpactPrefab != null)
        {
            Quaternion rayImpactRotation = Quaternion.LookRotation(-normal);
            GameObject rayimpact = Instantiate(rayImpactPrefab, position, rayImpactRotation);
            
            // Keep on Default layer for visibility
            rayimpact.layer = LayerMask.NameToLayer("Default");
            
            Destroy(rayimpact, 1f);
        }
    }

    private void CreateProjectile(Vector3 start, Vector3 direction, float maxDistance)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[GunScript] Projectile prefab is not assigned! Cannot shoot projectile.");
            return;
        }

        // CRITICAL FIX: Spawn projectile slightly forward to avoid immediate collision with gun/player
        // This prevents the "impact 1 foot in front" issue caused by instant collision
        float spawnOffset = 0.3f; // Spawn 30cm in front of shooting point
        Vector3 spawnPosition = start + direction * spawnOffset;
        
        // Instantiate the projectile at the offset position
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        
        // Keep on Default layer for visibility
        projectile.layer = LayerMask.NameToLayer("Default");
        
        // Make projectile face the direction it's traveling
        if (direction != Vector3.zero)
        {
            projectile.transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Ensure projectile has a collider for collision detection
        SphereCollider collider = projectile.GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = projectile.AddComponent<SphereCollider>();
            collider.radius = 0.1f; // Adjust based on your sphere size
        }
        
        // CRITICAL FIX: Use NON-TRIGGER collider for reliable wall detection
        // Trigger colliders don't work with non-convex MeshColliders (Unity limitation)
        // We'll use collision detection instead of trigger detection
        collider.isTrigger = false;
        
        // Add Rigidbody for physics-based movement
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        rb.mass = 0.05f; // Lighter = less affected by gravity
        rb.linearDamping = 0f; // No air resistance
        rb.angularDamping = 0.5f; // Prevent excessive spinning
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth movement
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Better collision detection for fast-moving objects
        
        // CRITICAL FIX: Configure physics material for no bounce
        PhysicsMaterial projectilePhysicsMaterial = new PhysicsMaterial("ProjectileMaterial");
        projectilePhysicsMaterial.bounciness = 0f; // No bounce
        projectilePhysicsMaterial.dynamicFriction = 0f; // Slide freely
        projectilePhysicsMaterial.staticFriction = 0f; // Slide freely
        projectilePhysicsMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
        projectilePhysicsMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
        collider.material = projectilePhysicsMaterial;
        
        // Apply gravity scale
        rb.useGravity = false; // We'll apply custom gravity in ProjectileController for more control
        
        // Set constraints to prevent unwanted rotation
        rb.constraints = RigidbodyConstraints.None; // Allow all movement for now
        
        // Launch projectile with velocity
        rb.linearVelocity = direction * projectileSpeed;
        
        // Add a component to control the projectile
        ProjectileController controller = projectile.AddComponent<ProjectileController>();
        controller.Initialize(projectileSpeed, projectileLifetime, gravityMultiplier, rayImpactPrefab, this, enableDebugLogging, gameObject);
        
        if (enableDebugLogging) Debug.Log($"[GunScript] Created projectile at {spawnPosition} (offset from {start}), Direction: {direction}, Speed: {projectileSpeed}, Gravity: {gravityMultiplier}x");
    }

    private void CreateLaserLine(Vector3 start, Vector3 end)
    {
        if (linePrefab != null)
        {
            LineRenderer line = Instantiate(linePrefab);
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            
            // Keep on Default layer (as it was working before)
            line.gameObject.layer = LayerMask.NameToLayer("Default");
            
            // Set sorting order to ensure it renders on top (even within same layer)
            line.sortingOrder = 32767; // Maximum sorting order value
            
            // CRITICAL FIX: Ensure the material renders in front by adjusting render queue and depth settings
            if (line.material != null)
            {
                // Create a material instance to avoid modifying the prefab material
                Material lineMaterial = new Material(line.material);
                
                // Use Overlay render queue (3000+) to render after ALL geometry including walls
                lineMaterial.renderQueue = 4000; // High value ensures it renders last/on top
                
                // Disable depth writing so it doesn't affect depth buffer but still renders
                if (lineMaterial.HasProperty("_ZWrite"))
                {
                    lineMaterial.SetFloat("_ZWrite", 0f);
                }
                
                // CRITICAL: Set ZTest to Always - this makes it always render regardless of depth
                // This is what makes it visible even when walls are in front
                if (lineMaterial.HasProperty("_ZTest"))
                {
                    // ZTest Always = 8 in Unity's CompareFunction enum
                    lineMaterial.SetInt("_ZTest", 8);
                }
                
                // Alternative property name some shaders use
                if (lineMaterial.HasProperty("_ZTestMode"))
                {
                    lineMaterial.SetInt("_ZTestMode", 8);
                }
                
                line.material = lineMaterial;
            }
            else if (enableDebugLogging)
            {
                Debug.LogWarning("[GunScript] Laser line has no material! Cannot set render settings.");
            }
            
            // Also set the width to ensure it's visible
            if (line.widthMultiplier <= 0.01f)
            {
                line.widthMultiplier = 0.02f; // Ensure minimum visible width
            }
            
            if (enableDebugLogging) Debug.Log($"[GunScript] Created laser line from {start} to {end}, Layer: {line.gameObject.layer} ({LayerMask.LayerToName(line.gameObject.layer)}), SortingOrder: {line.sortingOrder}, RenderQueue: {(line.material != null ? line.material.renderQueue : -1)}");
            
            Destroy(line.gameObject, lineShowTimer);
        }
    }
}

/// <summary>
/// SIMPLIFIED Controller component for projectiles
/// Handles physics, gravity, and collision detection
/// Simple approach: Any collision with non-gun/non-player object = destroy bullet and trigger wall destruction
/// </summary>
public class ProjectileController : MonoBehaviour
{
    private float lifetime;
    private float spawnTime;
    private float gravityMultiplier;
    private bool initialized = false;
    private GameObject impactEffectPrefab;
    private GunScript gunScript;
    private Rigidbody rb;
    private bool hasCollided = false;
    private bool debugLogging = false;
    private GameObject gunObject;
    private Collider projectileCollider;
    
    // Collision is disabled for this time after spawn to prevent immediate self-collision
    private const float COLLISION_ENABLE_DELAY = 0.1f;
    private bool collisionEnabled = false;

    public void Initialize(float projectileSpeed, float projectileLifetime, float gravity, GameObject impactPrefab, GunScript gun, bool enableDebug = false, GameObject gunGameObject = null)
    {
        lifetime = projectileLifetime;
        gravityMultiplier = gravity;
        spawnTime = Time.time;
        impactEffectPrefab = impactPrefab;
        gunScript = gun;
        debugLogging = enableDebug;
        gunObject = gunGameObject;
        
        rb = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<Collider>();
        
        // CRITICAL: Disable collider initially to prevent immediate collision
        if (projectileCollider != null)
        {
            projectileCollider.enabled = false;
        }
        
        // Set up collision ignores with gun and player
        SetupCollisionIgnores();
        
        initialized = true;
        
        if (debugLogging) Debug.Log($"[ProjectileController] Initialized. Collider disabled for {COLLISION_ENABLE_DELAY}s");
    }
    
    private void SetupCollisionIgnores()
    {
        if (projectileCollider == null) return;
        
        // Ignore gun colliders
        if (gunObject != null)
        {
            Collider[] gunColliders = gunObject.GetComponentsInChildren<Collider>();
            foreach (Collider gunCollider in gunColliders)
            {
                if (gunCollider != null)
                {
                    Physics.IgnoreCollision(projectileCollider, gunCollider, true);
                }
            }
            
            // Also ignore parent hierarchy (hand, controller, etc.)
            Transform parent = gunObject.transform.parent;
            while (parent != null)
            {
                Collider[] parentColliders = parent.GetComponentsInChildren<Collider>();
                foreach (Collider col in parentColliders)
                {
                    if (col != null && col != projectileCollider)
                    {
                        Physics.IgnoreCollision(projectileCollider, col, true);
                    }
                }
                parent = parent.parent;
            }
        }
        
        // Ignore player colliders
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider[] playerColliders = player.GetComponentsInChildren<Collider>();
            foreach (Collider playerCollider in playerColliders)
            {
                if (playerCollider != null)
                {
                    Physics.IgnoreCollision(projectileCollider, playerCollider, true);
                }
            }
        }
        
        // Also find OVRCameraRig and ignore all its colliders (VR player rig)
        GameObject cameraRig = GameObject.Find("OVRCameraRig");
        if (cameraRig == null) cameraRig = GameObject.Find("XR Origin");
        if (cameraRig == null) cameraRig = GameObject.Find("CameraRig");
        
        if (cameraRig != null)
        {
            Collider[] rigColliders = cameraRig.GetComponentsInChildren<Collider>();
            foreach (Collider col in rigColliders)
            {
                if (col != null)
                {
                    Physics.IgnoreCollision(projectileCollider, col, true);
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!initialized || rb == null) return;

        // Apply custom gravity
        rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        
        // Enable collision after delay
        if (!collisionEnabled && Time.time - spawnTime >= COLLISION_ENABLE_DELAY)
        {
            if (projectileCollider != null)
            {
                projectileCollider.enabled = true;
                collisionEnabled = true;
                if (debugLogging) Debug.Log("[ProjectileController] Collider enabled");
            }
        }

        // Check if exceeded lifetime
        if (Time.time - spawnTime >= lifetime)
        {
            if (debugLogging) Debug.Log("[ProjectileController] Lifetime exceeded, destroying");
            Destroy(gameObject);
        }
    }

    // Simple collision detection - any collision destroys the bullet and triggers wall destruction
    void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;
        if (!collisionEnabled) return; // Extra safety
        
        // Ignore gun and player
        if (gunObject != null && (collision.transform.IsChildOf(gunObject.transform) || collision.gameObject == gunObject))
        {
            return;
        }
        
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }
        
        // Get collision data
        Vector3 impactPosition = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
        Vector3 impactNormal = collision.contacts.Length > 0 ? collision.contacts[0].normal : Vector3.up;
        
        if (debugLogging) Debug.Log($"[ProjectileController] Collision with: {collision.gameObject.name}");
        
        // Handle the impact
        HandleImpact(collision.gameObject, impactPosition, impactNormal);
    }
    
    private void HandleImpact(GameObject hitObject, Vector3 impactPosition, Vector3 impactNormal)
    {
        if (hasCollided) return;
        hasCollided = true;
        
        if (debugLogging) Debug.Log($"[ProjectileController] Impact! Hit: {hitObject.name}");
        
        // Create impact effect
        if (impactEffectPrefab != null)
        {
            Quaternion impactRotation = Quaternion.LookRotation(-impactNormal);
            GameObject impact = Instantiate(impactEffectPrefab, impactPosition, impactRotation);
            impact.layer = LayerMask.NameToLayer("Default");
            Destroy(impact, 1f);
        }
        
        // Trigger hit events - this will destroy the wall segment
        if (gunScript != null)
        {
            gunScript.OnShootAndHit.Invoke(hitObject);
            
            // Check for Ghost
            Ghost ghost = hitObject.GetComponent<Ghost>();
            if (ghost == null) ghost = hitObject.GetComponentInParent<Ghost>();
            if (ghost != null) ghost.Kill();
        }
        
        // Destroy the bullet
        Destroy(gameObject);
    }
}
