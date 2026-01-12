using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;

/// <summary>
/// Ghost AI that wanders around the room, explores furniture, and reacts to player attention.
/// 
/// Key Behaviors:
/// - Wanders independently around the room using NavMesh
/// - Explores furniture and objects (beds, tables, couches)
/// - Freezes for 2 seconds when player looks directly at it
/// - After freezing, runs away and hides behind objects or away from player
/// - Performs lively behaviors when near player (circling, darting)
/// - Uses spatial audio for idle, movement, shocked, and death sounds
/// 
/// Setup in Unity:
/// 1. Assign playerEyeTransform to the CenterEyeAnchor (or camera) for accurate eye contact detection
/// 2. Set lineOfSightMask to exclude UI layers and other non-blocking layers
/// 3. Tune detectionFreezeTime (default 2 seconds) for desired freeze duration
/// 4. Adjust playerFOVAngle (default 45 degrees) for detection sensitivity
/// </summary>
public class Ghost : MonoBehaviour, IPooledObject
{
    [Header("Ghost Settings")]
    public Animator animator;
    [Tooltip("Base movement speed - increase for faster ghosts")]
    public float speed = 2.5f;
    public float roamRadius = 5f;
    public float roamInterval = 2f;
    public float detectionRange = 5f;
    [Tooltip("Height offset to make ghost appear to hover above ground")]
    public float hoverHeight = 0.3f;
    
    [Header("Room Exploration")]
    [Tooltip("How much ghosts prefer to explore furniture/objects (0-1, higher = more curious)")]
    [Range(0f, 1f)]
    public float explorationCuriosity = 0.7f;
    [Tooltip("Minimum time to spend near an object of interest")]
    public float minExploreTime = 2f;
    [Tooltip("Maximum time to spend near an object of interest")]
    public float maxExploreTime = 5f;
    
    [Header("Hiding Behavior")]
    [Tooltip("Distance at which ghost will react when player looks at it")]
    public float hideDistance = 8f;
    [Tooltip("Field of view angle (degrees) - ghost detects if player is looking within this angle")]
    public float playerFOVAngle = 45f;
    [Tooltip("How long ghost freezes when player makes eye contact (seconds)")]
    public float detectionFreezeTime = 2f;
    [Tooltip("Speed multiplier when running away after being detected")]
    public float hideSpeedMultiplier = 2f;
    [Tooltip("Minimum distance to maintain from player when hiding")]
    public float minHideDistance = 4f;
    [Tooltip("Layer mask for line-of-sight detection (set in Unity to exclude UI, etc.)")]
    public LayerMask lineOfSightMask = -1;
    
    [Header("Lively Behavior")]
    [Tooltip("When near player, ghost will circle around at this radius")]
    public float circleRadius = 3f;
    [Tooltip("Speed multiplier when being lively near player")]
    public float livelySpeedMultiplier = 1.3f;
    [Tooltip("Time between lively behavior actions")]
    public float livelyActionInterval = 2f;

    [Header("Debug")]
    [Tooltip("Enable detailed logging for build testing (shows detection events, freeze timing, etc.)")]
    public bool enableDebugLogging = false;
    
    [Header("Audio")]
    [Tooltip("Idle loop (plays when the ghost is not moving)")]
    public AudioClip idleSound;

    [Tooltip("Movement loop (plays when the ghost is moving).")]
    public AudioClip movingSound;

    [Tooltip("Plays once when the ghost dies")]
    public AudioClip deathSound;

    [Tooltip("Shocked sound when player makes eye contact (one-shot, optional)")]
    public AudioClip shockedSound;

    [Header("Audio Levels")]
    [Tooltip("Volume for idle loop (0-1)")]
    [Range(0f, 1f)]
    public float idleVolume = 0.18f;

    [Tooltip("Volume for movement loop (0-1).")]
    [Range(0f, 1f)]
    public float movingVolume = 0.45f;

    [Tooltip("Volume for shocked sound (0-1)")]
    [Range(0f, 1f)]
    public float shockedVolume = 0.7f;

    [Tooltip("Volume for death sound (0-1)")]
    [Range(0f, 1f)]
    public float deathVolume = 0.85f;

    [Tooltip("Minimum time between shocked sounds (prevents spam)")]
    public float shockedSoundCooldown = 2f;

    [Tooltip("How fast the agent must be moving before we consider the ghost 'moving' for audio")]
    public float movementSpeedThreshold = 0.12f;

    [Header("Animation")]
    [Tooltip("If enabled, the script will drive locomotion bools (IsMoving/IsIdle). Leave OFF if you only use Idle + Death for now.")]
    public bool driveLocomotionBools = false;

    // Auto-detected references (no manual assignment needed)
    private GameObject player;
    private Transform playerEyeTransform;
    private Camera playerCamera;
    
    // Component references
    private NavMeshAgent agent;
    private AudioSource sfxSource;
    
    // Behavior state
    private float lastShockedSoundTime = 0f;
    private float roamTimer = 0f;
    private bool isDead = false;
    private bool isHiding = false;
    private bool isReactingToDetection = false;
    private Coroutine reactionCoroutine = null;
    private float freezeStartTime = 0f;
    
    // Room exploration
    private MRUKRoom currentRoom;
    private Vector3 roomCenter;
    private Bounds roomBounds;
    private List<MRUKAnchor> obstacleAnchors;
    private float lastLivelyActionTime = 0f;
    private Vector3 circleTargetPosition;
    private MRUKAnchor currentExplorationTarget;
    private float explorationTimer = 0f;
    private bool isExploring = false;
    private string currentAnimationState = "Idle";

    private enum LoopState
    {
        None,
        Idle,
        Moving
    }

    private LoopState currentLoopState = LoopState.None;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Allow animator to live on a child "visual" object so we can animate bobbing/materials
        // without fighting the NavMeshAgent-driven root transform.
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }
        
        // IMPORTANT: This ghost is driven by NavMeshAgent + script movement.
        // Root motion can override transform movement and make the ghost look "stuck".
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
        
        // Setup audio (reuse existing AudioSource on prefab if present)
        SetupAudioSource();
    }
    
    void SetupAudioSource()
    {
        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        // Default 3D settings for the ghost's audio
        sfxSource.playOnAwake = false;
        sfxSource.loop = false; // we’ll enable loop only for idle/move loops
        sfxSource.spatialBlend = 1f; // 3D sound
        sfxSource.rolloffMode = AudioRolloffMode.Logarithmic;
        sfxSource.minDistance = 2f;
        sfxSource.maxDistance = 15f;
        sfxSource.dopplerLevel = 0f;
    }

    void Start()
    {
        FindPlayerReferences();
        InitializeAgent();
        InitializeRoomData();
        PickNewRoamPosition();
    }
    
    void FindPlayerReferences()
    {
        // Auto-find player using tag (no manual assignment needed)
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[Ghost] No GameObject with 'Player' tag found! Ghost will not function properly.");
                return;
            }
            
            if (enableDebugLogging)
                Debug.Log($"[Ghost] Found player: {player.name}");
        }

        // Auto-find camera (try multiple common locations)
        if (playerCamera == null && player != null)
        {
            playerCamera = player.GetComponentInChildren<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;
                
            if (enableDebugLogging && playerCamera != null)
                Debug.Log($"[Ghost] Found camera: {playerCamera.name}");
        }

        // Auto-find eye transform for accurate detection
        if (playerEyeTransform == null && player != null)
        {
            // Try common VR eye anchor names
            playerEyeTransform = FindTransformByName(player.transform, "CenterEyeAnchor");
            
            if (playerEyeTransform == null)
                playerEyeTransform = FindTransformByName(player.transform, "Main Camera");
                
            if (playerEyeTransform == null)
                playerEyeTransform = FindTransformByName(player.transform, "Camera");
            
            // Fallback to camera transform
            if (playerEyeTransform == null && playerCamera != null)
                playerEyeTransform = playerCamera.transform;
            
            if (enableDebugLogging && playerEyeTransform != null)
                Debug.Log($"[Ghost] Auto-detected eye transform: {playerEyeTransform.name}");
        }
    }
    
    void InitializeAgent()
    {
        if (agent != null)
        {
            agent.speed = speed;
            agent.updateRotation = false; // Manual rotation control
            agent.updatePosition = false; // Manual position control for smoothness
            agent.baseOffset = hoverHeight; // Make ghost hover above ground
            
            if (enableDebugLogging)
                Debug.Log($"[Ghost] Agent initialized - Speed: {speed}, Hover: {hoverHeight}");
        }
    }
    
    // Helper function to recursively find a transform by name
    Transform FindTransformByName(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;
            
        foreach (Transform child in parent)
        {
            Transform result = FindTransformByName(child, name);
            if (result != null)
                return result;
        }
        
        return null;
    }
    
    void InitializeRoomData()
    {
        // Get current room from MRUK
        if (MRUK.Instance != null && MRUK.Instance.IsInitialized)
        {
            currentRoom = MRUK.Instance.GetCurrentRoom();
            if (currentRoom != null)
            {
                // Calculate room bounds
                CalculateRoomBounds();
                
                // Find obstacle anchors (beds, tables, etc.)
                FindObstacleAnchors();
            }
        }
    }
    
    void CalculateRoomBounds()
    {
        if (currentRoom == null) return;
        
        Vector3 min = Vector3.one * float.MaxValue;
        Vector3 max = Vector3.one * float.MinValue;
        
        foreach (var anchor in currentRoom.Anchors)
        {
            if (anchor.Label.HasFlag(MRUKAnchor.SceneLabels.FLOOR))
            {
                Bounds bounds = GetAnchorBounds(anchor);
                if (bounds.size.magnitude > 0.01f)
                {
                    min = Vector3.Min(min, bounds.min);
                    max = Vector3.Max(max, bounds.max);
                }
            }
        }
        
        if (min != Vector3.one * float.MaxValue)
        {
            roomBounds = new Bounds((min + max) * 0.5f, max - min);
            roomCenter = roomBounds.center;
            roomCenter.y = transform.position.y; // Keep at ghost's height
        }
        else
        {
            // Fallback: use player position as center
            roomCenter = player != null ? player.transform.position : transform.position;
            roomBounds = new Bounds(roomCenter, Vector3.one * 10f);
        }
    }
    
    Bounds GetAnchorBounds(MRUKAnchor anchor)
    {
        // Try to get bounds from VolumeBounds first
        if (anchor.VolumeBounds.HasValue)
        {
            Bounds localBounds = anchor.VolumeBounds.Value;
            Vector3 worldCenter = anchor.transform.TransformPoint(localBounds.center);
            Vector3 worldSize = anchor.transform.TransformVector(localBounds.size);
            return new Bounds(worldCenter, worldSize);
        }
        // Fallback to PlaneRect
        else if (anchor.PlaneRect.HasValue)
        {
            Rect rect = anchor.PlaneRect.Value;
            Vector3 planeCenter = anchor.transform.position;
            Vector3 planeSize = new Vector3(rect.width, rect.height, 0.1f);
            return new Bounds(planeCenter, planeSize);
        }
        // Default bounds around anchor position
        else
        {
            return new Bounds(anchor.transform.position, Vector3.one * 0.5f);
        }
    }
    
    void FindObstacleAnchors()
    {
        obstacleAnchors = new List<MRUKAnchor>();
        if (currentRoom == null) return;
        
        // Look for furniture anchors that can be used as hiding spots
        foreach (var anchor in currentRoom.Anchors)
        {
            MRUKAnchor.SceneLabels label = anchor.Label;
            // Include beds, tables, couches, screens, and other furniture
            if (label.HasFlag(MRUKAnchor.SceneLabels.BED) ||
                label.HasFlag(MRUKAnchor.SceneLabels.TABLE) ||
                label.HasFlag(MRUKAnchor.SceneLabels.COUCH) ||
                label.HasFlag(MRUKAnchor.SceneLabels.SCREEN))
            {
                obstacleAnchors.Add(anchor);
            }
        }
    }

    // Performance optimization: Cache player distance check interval
    private float lastPlayerCheckTime = 0f;
    private float playerCheckInterval = 0.1f; // Check every 0.1 seconds instead of every frame
    private float cachedDistanceToPlayer = 0f;
    private bool cachedPlayerLookingAtGhost = false;

    void Update()
    {
        if (agent == null || !agent.enabled || player == null || isDead) return;

        // If something (often animation/root motion) nudges the ghost off the NavMesh,
        // the agent stops updating and the ghost appears frozen. Try to recover gracefully.
        // OPTIMIZATION: Cache NavMesh check - this can be expensive with many ghosts
        if (!agent.isOnNavMesh)
        {
            var sw = enableDebugLogging ? System.Diagnostics.Stopwatch.StartNew() : null;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                
                if (sw != null)
                {
                    sw.Stop();
                    if (sw.ElapsedMilliseconds > 1)
                    {
                        UnityEngine.Debug.Log($"[PERF] Ghost {gameObject.name}: NavMesh recovery took {sw.ElapsedMilliseconds}ms");
                    }
                }
            }
            else
            {
                if (enableDebugLogging) UnityEngine.Debug.LogWarning($"[Ghost] {gameObject.name} could not recover NavMesh position");
                return;
            }
        }

        roamTimer += Time.deltaTime;

        // Performance optimization: Only check player distance/looking periodically, not every frame
        if (Time.time - lastPlayerCheckTime >= playerCheckInterval)
        {
            cachedDistanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            cachedPlayerLookingAtGhost = IsPlayerLookingAtGhost();
            lastPlayerCheckTime = Time.time;
        }
        
        float distanceToPlayer = cachedDistanceToPlayer;
        bool playerLookingAtGhost = cachedPlayerLookingAtGhost;
        
        // React to being looked at - freeze, face player, then run away
        if (playerLookingAtGhost && distanceToPlayer <= hideDistance && !isReactingToDetection && !isHiding && reactionCoroutine == null)
        {
            if (enableDebugLogging)
                Debug.Log($"[Ghost] Player detected! Distance: {distanceToPlayer:F1}m, Starting freeze reaction");
            
            // Play shocked sound when eye contact is made
            PlayShockedSound();
            reactionCoroutine = StartCoroutine(ReactToDetection());
        }
        // Hiding behavior: continue hiding after reaction
        else if (isHiding)
        {
            agent.speed = speed * hideSpeedMultiplier;
            
            // Check if we've reached hiding destination or far enough from player
            if (agent.remainingDistance <= 0.5f || distanceToPlayer > hideDistance * 1.5f)
            {
                // Successfully hidden, resume normal behavior
                isHiding = false;
                agent.speed = speed;
                
                if (enableDebugLogging)
                    Debug.Log($"[Ghost] Finished hiding, resuming normal behavior");
            }
            // Continue hiding if still being looked at and close
            else if (playerLookingAtGhost && distanceToPlayer < hideDistance)
            {
                HideFromPlayer();
            }
            else if (distanceToPlayer < hideDistance * 0.8f)
            {
                // Still too close, keep hiding
                HideFromPlayer();
            }
            else
            {
                // Player stopped looking and we're far enough, resume normal behavior
                isHiding = false;
                agent.speed = speed;
                
                if (enableDebugLogging)
                    Debug.Log($"[Ghost] Player no longer looking, resuming normal behavior");
            }
        }
        // Lively behavior when near player
        else if (distanceToPlayer <= detectionRange && !isHiding && !isReactingToDetection)
        {
            agent.speed = speed * livelySpeedMultiplier;
            PerformLivelyBehavior();
        }
        // Normal wandering behavior
        else
        {
            isHiding = false;
            agent.speed = speed;
            
            // Handle exploration behavior
            if (isExploring)
            {
                explorationTimer -= Time.deltaTime;
                
                // Stay near the exploration target
                if (agent.remainingDistance <= 0.5f && explorationTimer > 0)
                {
                    // Occasionally move slightly around the object
                    if (Random.value < 0.1f * Time.deltaTime)
                    {
                        Vector3 offset = Random.insideUnitSphere * 1f;
                        offset.y = 0;
                        Vector3 newPos = currentExplorationTarget.transform.position + offset;
                        
                        NavMeshHit hit;
                        if (NavMesh.SamplePosition(newPos, out hit, 2f, NavMesh.AllAreas))
                        {
                            agent.SetDestination(hit.position);
                        }
                    }
                }
                else if (explorationTimer <= 0)
                {
                    // Done exploring, pick new destination
                    isExploring = false;
                    PickNewRoamPosition();
                    roamTimer = 0f;
                }
            }
            else
            {
                // Normal roaming behavior
                if (agent.remainingDistance <= 0.2f && roamTimer >= roamInterval)
                {
                    PickNewRoamPosition();
                    roamTimer = 0f;
                }
            }
        }

        // Update idle/move loop audio based on current motion/state
        UpdateLoopAudio();

        // Smooth follow agent path
        Vector3 targetPos = agent.nextPosition;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);

        // Smooth rotation - but pause rotation when reacting to detection
        if (!isReactingToDetection && agent.desiredVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.desiredVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        
        // Update animation state based on movement
        UpdateAnimationState();

        // Keep the agent in sync with our transform since agent.updatePosition=false.
        // This prevents drift/desync that can eventually cause !agent.isOnNavMesh and freezing.
        agent.nextPosition = transform.position;
    }
    
    void UpdateAnimationState()
    {
        if (animator == null) return;
        if (!driveLocomotionBools) return;
        
        // Determine current state
        string newState = "Idle";
        
        if (isReactingToDetection)
        {
            // Shocked animation is handled separately via trigger
            return;
        }
        else if (agent.velocity.sqrMagnitude > 0.1f)
        {
            newState = "Moving";
        }
        else
        {
            newState = "Idle";
        }
        
        // Only update if state changed
        if (newState != currentAnimationState)
        {
            currentAnimationState = newState;
            
            if (animator != null)
            {
                animator.SetBool("IsMoving", newState == "Moving");
                animator.SetBool("IsIdle", newState == "Idle");
            }
        }
    }
    
    bool IsPlayerLookingAtGhost()
    {
        if (player == null) return false;
        
        // Use playerEyeTransform if assigned, otherwise fall back to playerCamera
        Transform eyeTransform = playerEyeTransform != null ? playerEyeTransform : (playerCamera != null ? playerCamera.transform : null);
        
        if (eyeTransform == null) return false;
        
        // Calculate direction from player's eye to ghost
        Vector3 eyePosition = eyeTransform.position;
        Vector3 directionToGhost = (transform.position - eyePosition).normalized;
        Vector3 playerForward = eyeTransform.forward;
        
        // Calculate angle between player's view direction and direction to ghost
        float angle = Vector3.Angle(playerForward, directionToGhost);
        
        // Check if ghost is within player's field of view
        if (angle > playerFOVAngle * 0.5f)
            return false;
        
        // Perform raycast to check for direct line of sight
        float distanceToGhost = Vector3.Distance(eyePosition, transform.position);
        RaycastHit hit;
        
        // Raycast from player's eye to ghost
        if (Physics.Raycast(eyePosition, directionToGhost, out hit, distanceToGhost, lineOfSightMask))
        {
            // Check if the raycast hit the ghost
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                return true;
            }
        }
        
        return false;
    }
    
    IEnumerator ReactToDetection()
    {
        if (enableDebugLogging)
            Debug.Log($"[Ghost] FREEZE START - Duration: {detectionFreezeTime}s");
        
        freezeStartTime = Time.time;
        isReactingToDetection = true;
        agent.isStopped = true;
        
        // Silence loops briefly so the shocked sound reads clearly
        StopLoopAudio();
        
        // Play shocked animation if available
        if (animator != null)
        {
            animator.SetTrigger("Shocked");
        }
        
        // Use WaitForSeconds for accurate timing instead of manual timer
        float elapsed = 0f;
        while (elapsed < detectionFreezeTime)
        {
            // Face the player's eye position during freeze
            if (player != null)
            {
                Transform eyeTransform = playerEyeTransform != null ? playerEyeTransform : 
                                       (playerCamera != null ? playerCamera.transform : player.transform);
                Vector3 targetPosition = eyeTransform.position;
                
                Vector3 directionToPlayer = (targetPosition - transform.position);
                directionToPlayer.y = 0; // Keep horizontal
                
                if (directionToPlayer.sqrMagnitude > 0.01f)
                {
                    Quaternion lookAtPlayer = Quaternion.LookRotation(directionToPlayer.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookAtPlayer, Time.deltaTime * 10f);
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Double-check timing for accuracy
        float actualFreezeTime = Time.time - freezeStartTime;
        if (enableDebugLogging)
            Debug.Log($"[Ghost] FREEZE END - Actual duration: {actualFreezeTime:F2}s, Started running away");
        
        // Freeze time is over - now run away and hide!
        isReactingToDetection = false;
        isHiding = true;
        agent.isStopped = false;
        agent.speed = speed * hideSpeedMultiplier;
        reactionCoroutine = null; // Clear coroutine reference
        HideFromPlayer();
    }

    void UpdateLoopAudio()
    {
        if (sfxSource == null) return;

        // Don’t run loops while reacting; let the one-shot read clearly
        if (isDead || isReactingToDetection)
        {
            StopLoopAudio();
            return;
        }

        bool isMovingNow = agent != null && agent.enabled && agent.desiredVelocity.sqrMagnitude > (movementSpeedThreshold * movementSpeedThreshold);

        AudioClip desiredClip = null;
        float desiredVolume = 0f;
        LoopState desiredState = LoopState.None;

        if (isMovingNow)
        {
            desiredClip = movingSound;
            desiredState = desiredClip != null ? LoopState.Moving : LoopState.None;
            desiredVolume = movingVolume;
        }
        else
        {
            desiredClip = idleSound;
            desiredState = desiredClip != null ? LoopState.Idle : LoopState.None;
            desiredVolume = idleVolume;
        }

        // Stop if we have nothing to play
        if (desiredState == LoopState.None || desiredClip == null)
        {
            StopLoopAudio();
            return;
        }

        // If already playing the right loop, just ensure volume
        if (currentLoopState == desiredState && sfxSource.isPlaying && sfxSource.clip == desiredClip && sfxSource.loop)
        {
            sfxSource.volume = desiredVolume;
            return;
        }

        // Switch loops
        sfxSource.Stop();
        sfxSource.clip = desiredClip;
        sfxSource.loop = true;
        sfxSource.volume = desiredVolume;
        sfxSource.Play();

        currentLoopState = desiredState;
    }

    void StopLoopAudio()
    {
        if (sfxSource == null) return;

        if (sfxSource.isPlaying && sfxSource.loop)
        {
            sfxSource.Stop();
        }
        if (sfxSource.loop)
        {
            sfxSource.loop = false;
        }
        sfxSource.clip = null;
        currentLoopState = LoopState.None;
    }
    
    void PlayShockedSound()
    {
        // Prevent spam of shocked sounds
        if (Time.time - lastShockedSoundTime < shockedSoundCooldown)
            return;
            
        if (sfxSource != null && shockedSound != null)
        {
            sfxSource.PlayOneShot(shockedSound, shockedVolume);
            lastShockedSoundTime = Time.time;
        }
    }

    void PlayDeathSound()
    {
        if (sfxSource != null && deathSound != null)
        {
            sfxSource.PlayOneShot(deathSound, deathVolume);
        }
    }
    
    void HideFromPlayer()
    {
        if (player == null || !agent.isOnNavMesh) return;
        
        // First, try to find a hiding spot behind an obstacle
        Vector3 bestHidePosition = FindHidingSpotBehindObstacle();
        
        if (bestHidePosition != Vector3.zero)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(bestHidePosition, out hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
        
        // Fallback: calculate direction away from player
        Vector3 directionFromPlayer = (transform.position - player.transform.position).normalized;
        
        // Try to find a hiding position away from player, maintaining minimum distance
        Vector3 hidePosition = transform.position + directionFromPlayer * (minHideDistance + Random.Range(1f, 3f));
        
        // Add some randomness to make hiding less predictable
        Vector3 randomOffset = new Vector3(
            Random.Range(-2f, 2f),
            0f,
            Random.Range(-2f, 2f)
        );
        hidePosition += randomOffset;
        
        // Sample navmesh to find valid hiding position
        NavMeshHit hit2;
        if (NavMesh.SamplePosition(hidePosition, out hit2, roamRadius * 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit2.position);
        }
        else
        {
            // Fallback: try to move perpendicular to player's view
            Vector3 perpendicular = Vector3.Cross(directionFromPlayer, Vector3.up).normalized;
            Vector3 alternativeHidePos = transform.position + perpendicular * minHideDistance;
            if (NavMesh.SamplePosition(alternativeHidePos, out hit2, roamRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit2.position);
            }
        }
    }
    
    Vector3 FindHidingSpotBehindObstacle()
    {
        if (obstacleAnchors == null || obstacleAnchors.Count == 0) return Vector3.zero;
        
        Vector3 playerPos = player.transform.position;
        Vector3 ghostPos = transform.position;
        Vector3 bestPosition = Vector3.zero;
        float bestScore = 0f;
        
        foreach (var anchor in obstacleAnchors)
        {
            Bounds obstacleBounds = GetAnchorBounds(anchor);
            Vector3 obstacleCenter = obstacleBounds.center;
            obstacleCenter.y = ghostPos.y; // Keep at ghost height
            
            // Check if obstacle is between player and ghost (good hiding spot)
            Vector3 toObstacle = (obstacleCenter - playerPos).normalized;
            Vector3 toGhost = (ghostPos - playerPos).normalized;
            float dot = Vector3.Dot(toObstacle, toGhost);
            
            // If obstacle is in the direction we want to hide, use it
            if (dot > 0.3f) // Obstacle is somewhat in the hiding direction
            {
                // Find position behind obstacle (away from player)
                Vector3 directionFromPlayer = (obstacleCenter - playerPos).normalized;
                Vector3 hidePosition = obstacleCenter + directionFromPlayer * (obstacleBounds.size.magnitude * 0.5f + 1f);
                
                // Score based on distance from player and how well it hides
                float distanceFromPlayer = Vector3.Distance(hidePosition, playerPos);
                float score = distanceFromPlayer * (1f + dot);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPosition = hidePosition;
                }
            }
        }
        
        return bestPosition;
    }
    
    void PerformLivelyBehavior()
    {
        if (Time.time - lastLivelyActionTime < livelyActionInterval) return;
        
        lastLivelyActionTime = Time.time;
        
        // Randomly choose between circling, darting, or quick repositioning
        float actionChoice = Random.value;
        
        if (actionChoice < 0.4f) // 40% chance: Circle around player
        {
            CircleAroundPlayer();
        }
        else if (actionChoice < 0.7f) // 30% chance: Quick dart to side
        {
            DartToSide();
        }
        else // 30% chance: Quick reposition
        {
            QuickReposition();
        }
    }
    
    void CircleAroundPlayer()
    {
        if (player == null) return;
        
        Vector3 playerPos = player.transform.position;
        Vector3 toGhost = (transform.position - playerPos).normalized;
        
        // Rotate around player
        Vector3 perpendicular = Vector3.Cross(toGhost, Vector3.up).normalized;
        Vector3 circlePos = playerPos + (toGhost + perpendicular * Random.Range(-0.5f, 0.5f)).normalized * circleRadius;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(circlePos, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
    
    void DartToSide()
    {
        if (player == null) return;
        
        Vector3 playerPos = player.transform.position;
        Vector3 toGhost = (transform.position - playerPos).normalized;
        Vector3 perpendicular = Vector3.Cross(toGhost, Vector3.up).normalized;
        
        // Dart to one side
        Vector3 dartPos = transform.position + perpendicular * Random.Range(2f, 4f);
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(dartPos, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
    
    void QuickReposition()
    {
        if (player == null) return;
        
        // Quick move to a nearby position
        Vector3 randomOffset = Random.insideUnitSphere * 3f;
        randomOffset.y = 0;
        Vector3 newPos = transform.position + randomOffset;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newPos, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void PickNewRoamPosition()
    {
        // Use room bounds for independent wandering instead of player position
        Vector3 targetPos;
        
        if (currentRoom != null && roomBounds.size.magnitude > 0.1f)
        {
            // Wander independently around the room
            Vector3 randomPoint = new Vector3(
                Random.Range(roomBounds.min.x, roomBounds.max.x),
                transform.position.y,
                Random.Range(roomBounds.min.z, roomBounds.max.z)
            );
            targetPos = randomPoint;
        }
        else
        {
            // Fallback: use player position if room data not available
            if (!player) return;
            Vector3 playerPos = player.transform.position;
            Vector3 randomOffset = Random.insideUnitSphere * roamRadius;
            randomOffset.y = 0;
            targetPos = playerPos + randomOffset;
        }
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, roamRadius * 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }



    // This is the function the Animation Event will call!
    public void DestroyGhost()
    {
        // Use object pooling if available
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnToPool("Ghost", gameObject);
        }
        else
        {
            // Otherwise just destroy it normally
            Destroy(gameObject);
        }
    }

    public void Kill()
    {
        if (isDead) return;
        
        isDead = true;
        if (agent != null) agent.enabled = false;
        
        // Stop loops + play death one-shot
        StopLoopAudio();
        PlayDeathSound();

        if (animator != null)
            animator.SetTrigger("Death");

        if (GameManager.instance != null)
        {
            GameManager.instance.AddScore(1);
            GameManager.instance.GhostKilled();
        }

        // --- OLD CODE (DELETE OR COMMENT THIS OUT) ---
        // if (ObjectPool.Instance != null)
        // {
        //    StartCoroutine(ReturnToPoolAfterDelay());
        // }
        // else
        // {
        //    Destroy(gameObject);
        // }
        // ---------------------------------------------

        // We do NOTHING here now. 
        // We wait for the Animation Event to call DestroyGhost()!
    }

    private IEnumerator ReturnToPoolAfterDelay()
    {
        yield return new WaitForSeconds(2f); // Wait for death animation
        ObjectPool.Instance.ReturnToPool("Ghost", gameObject);
    }

    public void OnObjectSpawn()
    {
        // Reset ghost state when spawned from pool
        isDead = false;
        isHiding = false;
        isReactingToDetection = false;
        reactionCoroutine = null;
        freezeStartTime = 0f;
        roamTimer = 0f;
        lastLivelyActionTime = 0f;
        lastShockedSoundTime = 0f;
        currentLoopState = LoopState.None;
        
        // Stop any audio that might be playing
        StopLoopAudio();
        
        // Re-find player references (in case they changed)
        FindPlayerReferences();
        
        // Re-initialize agent
        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = speed;
            agent.isStopped = false;
            agent.baseOffset = hoverHeight;
            
            if (enableDebugLogging)
                Debug.Log($"[Ghost] Spawned/Reset at {transform.position} - Speed: {speed}");
        }
        
        // Reinitialize room data
        InitializeRoomData();
        PickNewRoamPosition();
    }
}
