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
    [Tooltip("Speed of projectile (launch velocity) - Recommended: 15-20 for visible arc")]
    public float projectileSpeed = 15f;
    [Tooltip("Gravity multiplier (1 = normal gravity, higher = more arc, lower = straighter shots) - Recommended: 0.8-1.2")]
    [Range(0f, 2f)]
    public float gravityMultiplier = 1.0f;
    [Tooltip("If true, wall destruction happens on projectile hit (not instant)")]
    public bool delayWallDestruction = true;
    [Tooltip("Projectile collision layer - should collide with destructible mesh segments")]
    public string projectileLayer = "Default";
    
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
        if (transform.parent == null) return false;

        string pName = transform.parent.name.ToLower();
        if (pName.Contains("hand") || pName.Contains("controller") || pName.Contains("interactor") || pName.Contains("grab"))
        {
            return true;
        }

        if (GetComponent<Rigidbody>() == null) return true;

        return false;
    }

    void Update()
    {
        if (requireEquippedToShoot && !isEquipped) return;
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

        if (source != null && shootingAudioClip != null)
        {
            source.PlayOneShot(shootingAudioClip);
        }

        if (shootingPoint == null)
        {
            Debug.LogError("[GunScript] ShootingPoint is NULL! Cannot shoot.");
            return;
        }

        // For projectile mode with delayed wall destruction, just spawn the projectile
        if (useProjectileMode && delayWallDestruction)
        {
            CreateProjectile(shootingPoint.position, shootingPoint.forward, maxLineDistance);
            return;
        }

        // Original instant raycast behavior (for instant wall destruction)
        Ray ray = new Ray(shootingPoint.position, shootingPoint.forward);
        bool hasHit = Physics.Raycast(ray, out RaycastHit hit, maxLineDistance, layerMask);

        if (hasHit)
        {
            GameObject hitObject = hit.collider.gameObject;
            OnShootAndHit.Invoke(hitObject);

            Ghost ghost = hit.collider.GetComponent<Ghost>();
            if (ghost == null) ghost = hit.collider.GetComponentInParent<Ghost>();
            if (ghost != null) ghost.Kill();
        }
        else
        {
            OnShootAndMiss.Invoke();
        }

        if (useProjectileMode)
        {
            Vector3 endPoint = hasHit ? hit.point : shootingPoint.position + shootingPoint.forward * maxLineDistance;
            Vector3 direction = (endPoint - shootingPoint.position).normalized;
            CreateProjectile(shootingPoint.position, direction, maxLineDistance);
        }
    }

    private void CreateProjectile(Vector3 start, Vector3 direction, float maxDistance)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[GunScript] Projectile prefab is not assigned! Cannot shoot projectile.");
            return;
        }

        // Spawn projectile slightly forward to avoid immediate collision with gun/player
        float spawnOffset = 0.15f; // 15cm in front of shooting point
        Vector3 spawnPosition = start + direction * spawnOffset;

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        // Set projectile layer
        int layer = LayerMask.NameToLayer(projectileLayer);
        if (layer == -1) layer = LayerMask.NameToLayer("Default");
        projectile.layer = layer;
        
        if (direction != Vector3.zero)
        {
            projectile.transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Ensure projectile has a collider
        SphereCollider collider = projectile.GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = projectile.AddComponent<SphereCollider>();
            collider.radius = 0.05f; // 5cm radius for precision
        }
        collider.isTrigger = false;

        // Add Rigidbody
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        rb.mass = 0.01f; // Very light (10 grams)
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.None;
        
        // No bounce physics material
        PhysicsMaterial projectilePhysicsMaterial = new PhysicsMaterial("ProjectileMaterial");
        projectilePhysicsMaterial.bounciness = 0f;
        projectilePhysicsMaterial.dynamicFriction = 0f;
        projectilePhysicsMaterial.staticFriction = 0f;
        projectilePhysicsMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
        projectilePhysicsMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
        collider.material = projectilePhysicsMaterial;
        
        rb.useGravity = false; // Custom gravity in ProjectileController
        rb.linearVelocity = direction * projectileSpeed;

        if (enableDebugLogging)
        {
            Debug.Log($"[GunScript] Projectile created:");
            Debug.Log($"  - Mass: {rb.mass}kg");
            Debug.Log($"  - Velocity: {rb.linearVelocity.magnitude}m/s");
            Debug.Log($"  - Gravity: {gravityMultiplier}x");
        }
        
        ProjectileController controller = projectile.AddComponent<ProjectileController>();
        controller.Initialize(projectileSpeed, projectileLifetime, gravityMultiplier, rayImpactPrefab, this, enableDebugLogging, gameObject);
        
        if (enableDebugLogging) Debug.Log($"[GunScript] Projectile at {spawnPosition}, Direction: {direction}, Speed: {projectileSpeed}");
    }
}

/// <summary>
/// Controller component for projectiles - handles physics, gravity, and collision detection
/// </summary>
public class ProjectileController : MonoBehaviour
{
    // OPTIMIZATION: Cache skybox reference to avoid expensive Find() calls on every bullet spawn
    public static GameObject cachedSkybox = null;
    
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

        // Set up collision ignores FIRST before enabling collider
        SetupCollisionIgnores();

        // Keep collider ENABLED from start - Physics.IgnoreCollision handles gun/player
        if (projectileCollider != null)
        {
            projectileCollider.enabled = true;
        }

        initialized = true;

        if (debugLogging) Debug.Log($"[ProjectileController] Initialized. Collider enabled immediately (using Physics.IgnoreCollision)");
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
            
            // Ignore parent hierarchy (hand, controller, etc.)
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
        
        // Ignore VR camera rig colliders
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
        
        // CRITICAL FIX: Ignore skybox colliders - bullets should pass through to hit walls
        // OPTIMIZATION: Cache skybox reference to avoid expensive Find() calls on every bullet spawn
        if (ProjectileController.cachedSkybox == null)
        {
            ProjectileController.cachedSkybox = GameObject.Find("NightSkybox");
            if (ProjectileController.cachedSkybox == null) ProjectileController.cachedSkybox = GameObject.Find("Skybox");
            if (ProjectileController.cachedSkybox == null) ProjectileController.cachedSkybox = GameObject.Find("Sky");
        }
        
        if (ProjectileController.cachedSkybox != null)
        {
            Collider[] skyboxColliders = ProjectileController.cachedSkybox.GetComponentsInChildren<Collider>();
            foreach (Collider skyCol in skyboxColliders)
            {
                if (skyCol != null)
                {
                    Physics.IgnoreCollision(projectileCollider, skyCol, true);
                    if (debugLogging) Debug.Log($"[ProjectileController] Ignoring skybox collider: {ProjectileController.cachedSkybox.name}");
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!initialized || rb == null) return;

        // Apply custom gravity
        Vector3 gravityForce = Physics.gravity * gravityMultiplier;
        rb.AddForce(gravityForce, ForceMode.Acceleration);

        // Check lifetime
        if (Time.time - spawnTime >= lifetime)
        {
            if (debugLogging) Debug.Log($"[ProjectileController] Lifetime exceeded, destroying");
            Destroy(gameObject);
        }

        // Debug visualization
        if (debugLogging && rb != null)
        {
            Debug.DrawRay(transform.position, rb.linearVelocity.normalized * 0.5f, Color.yellow);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;

        // Ignore gun and player
        if (gunObject != null && (collision.transform.IsChildOf(gunObject.transform) || collision.gameObject == gunObject))
        {
            if (debugLogging) Debug.Log($"[ProjectileController] Ignoring gun collision: {collision.gameObject.name}");
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (debugLogging) Debug.Log($"[ProjectileController] Ignoring player collision: {collision.gameObject.name}");
            return;
        }

        // CRITICAL FIX: Ignore skybox collisions - bullets should pass through to hit walls
        string hitName = collision.gameObject.name.ToLower();
        if (hitName.Contains("skybox") || hitName.Contains("sky"))
        {
            if (debugLogging) Debug.Log($"[ProjectileController] Ignoring skybox collision: {collision.gameObject.name} - bullet passes through");
            return;
        }

        Vector3 impactPosition = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
        Vector3 impactNormal = collision.contacts.Length > 0 ? collision.contacts[0].normal : Vector3.up;

        if (debugLogging)
        {
            Debug.Log($"[ProjectileController] ===== COLLISION =====");
            Debug.Log($"  Hit: {collision.gameObject.name}");
            Debug.Log($"  Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
            Debug.Log($"  Position: {impactPosition}");
        }

        HandleImpact(collision.gameObject, impactPosition, impactNormal);
    }
    
    private void HandleImpact(GameObject hitObject, Vector3 impactPosition, Vector3 impactNormal)
    {
        if (hasCollided) return;
        hasCollided = true;

        if (debugLogging) Debug.Log($"[ProjectileController] Impact with: {hitObject.name}");

        // Check for Ghost (priority target)
        Ghost ghost = hitObject.GetComponent<Ghost>();
        if (ghost == null) ghost = hitObject.GetComponentInParent<Ghost>();
        if (ghost == null) ghost = hitObject.GetComponentInChildren<Ghost>();

        if (ghost != null)
        {
            if (debugLogging) Debug.Log($"[ProjectileController] Ghost detected! Killing: {ghost.gameObject.name}");
            ghost.Kill();
            CreateImpactEffect(impactPosition, impactNormal);
            Destroy(gameObject);
            return;
        }

        // Not a ghost - trigger wall destruction
        CreateImpactEffect(impactPosition, impactNormal);

        if (gunScript != null)
        {
            if (debugLogging) Debug.Log($"[ProjectileController] Invoking OnShootAndHit for: {hitObject.name}");
            gunScript.OnShootAndHit.Invoke(hitObject);
        }
        else
        {
            if (debugLogging) Debug.LogWarning("[ProjectileController] GunScript is null!");
        }

        Destroy(gameObject);
    }

    private void CreateImpactEffect(Vector3 position, Vector3 normal)
    {
        if (impactEffectPrefab != null)
        {
            Quaternion rotation = Quaternion.LookRotation(-normal);
            GameObject impact = Instantiate(impactEffectPrefab, position, rotation);
            impact.layer = LayerMask.NameToLayer("Default");
            Destroy(impact, 1f);
        }
    }
}
