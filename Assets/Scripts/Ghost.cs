using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;

public class Ghost : MonoBehaviour, IPooledObject
{
    [Header("Ghost Settings")]
    public Animator animator;
    public float speed = 1f;
    public float roamRadius = 5f;
    public float roamInterval = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public int damage = 10;
    
    [Header("Hiding Behavior")]
    [Tooltip("Distance at which ghost will try to hide from player")]
    public float hideDistance = 8f;
    [Tooltip("Field of view angle (degrees) - ghost hides if player is looking at it within this angle")]
    public float playerFOVAngle = 60f;
    [Tooltip("Speed multiplier when hiding")]
    public float hideSpeedMultiplier = 2f;
    [Tooltip("Minimum distance to maintain from player when hiding")]
    public float minHideDistance = 4f;
    [Tooltip("How long ghost pauses when detected (seconds)")]
    public float detectionPauseTime = 0.5f;
    
    [Header("Lively Behavior")]
    [Tooltip("When near player, ghost will circle around at this radius")]
    public float circleRadius = 3f;
    [Tooltip("Speed multiplier when being lively near player")]
    public float livelySpeedMultiplier = 1.3f;
    [Tooltip("Time between lively behavior actions")]
    public float livelyActionInterval = 2f;

    [Header("References")]
    public GameObject player;
    public Transform attackPoint;

    private NavMeshAgent agent;
    private float roamTimer = 0f;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isHiding = false;
    private bool isReactingToDetection = false;
    private Camera playerCamera;
    private MRUKRoom currentRoom;
    private Vector3 roomCenter;
    private Bounds roomBounds;
    private List<MRUKAnchor> obstacleAnchors;
    private float lastLivelyActionTime = 0f;
    private Vector3 circleTargetPosition;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        // Find player camera (usually MainCamera or CameraRig)
        if (player != null)
        {
            playerCamera = player.GetComponentInChildren<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        if (agent != null)
        {
            agent.speed = speed;
            agent.updateRotation = false; // We'll handle rotation
            agent.updatePosition = false; // We'll handle smooth movement
        }

        // Initialize room data for wandering
        InitializeRoomData();
        PickNewRoamPosition();
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

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh || !agent.enabled || player == null || isDead) return;

        roamTimer += Time.deltaTime;

        // Check if player is in range for attack
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        
        // Check if player is looking at ghost (for hiding behavior)
        bool playerLookingAtGhost = IsPlayerLookingAtGhost();
        
        // React to being looked at - pause and face player, then run away
        if (playerLookingAtGhost && distanceToPlayer <= hideDistance && distanceToPlayer > attackRange && !isReactingToDetection && !isAttacking)
        {
            StartCoroutine(ReactToDetection());
        }
        // Hiding behavior: continue hiding after reaction
        else if (isHiding && distanceToPlayer > attackRange)
        {
            agent.speed = speed * hideSpeedMultiplier;
            // Continue hiding if still being looked at
            if (playerLookingAtGhost)
            {
                HideFromPlayer();
            }
            else
            {
                // Player stopped looking, resume normal behavior
                isHiding = false;
            }
        }
        // Attack behavior when very close
        else if (distanceToPlayer <= attackRange && !isAttacking && !isReactingToDetection)
        {
            isHiding = false;
            agent.speed = speed;
            StartCoroutine(AttackPlayer());
        }
        // Lively behavior when near player but not attacking
        else if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange && !isHiding && !isAttacking && !isReactingToDetection)
        {
            agent.speed = speed * livelySpeedMultiplier;
            PerformLivelyBehavior();
        }
        // Normal wandering behavior
        else
        {
            isHiding = false;
            agent.speed = speed;
            // Normal roaming behavior
            if (agent.remainingDistance <= 0.2f && roamTimer >= roamInterval)
            {
                PickNewRoamPosition();
                roamTimer = 0f;
            }
        }

        // Smooth follow agent path
        Vector3 targetPos = agent.nextPosition;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);

        // Smooth rotation - but pause rotation when reacting to detection
        if (!isReactingToDetection && agent.desiredVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.desiredVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    
    bool IsPlayerLookingAtGhost()
    {
        if (playerCamera == null || player == null) return false;
        
        // Calculate direction from player camera to ghost
        Vector3 directionToGhost = (transform.position - playerCamera.transform.position).normalized;
        Vector3 playerForward = playerCamera.transform.forward;
        
        // Calculate angle between player's view direction and direction to ghost
        float angle = Vector3.Angle(playerForward, directionToGhost);
        
        // Check if ghost is within player's field of view
        return angle <= playerFOVAngle * 0.5f;
    }
    
    IEnumerator ReactToDetection()
    {
        isReactingToDetection = true;
        agent.isStopped = true;
        
        // Face the player
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        directionToPlayer.y = 0; // Keep horizontal
        Quaternion lookAtPlayer = Quaternion.LookRotation(directionToPlayer);
        
        float pauseTimer = 0f;
        while (pauseTimer < detectionPauseTime)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookAtPlayer, Time.deltaTime * 8f);
            pauseTimer += Time.deltaTime;
            yield return null;
        }
        
        // Now run away and hide
        isReactingToDetection = false;
        isHiding = true;
        agent.isStopped = false;
        agent.speed = speed * hideSpeedMultiplier;
        HideFromPlayer();
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


    private IEnumerator AttackPlayer()
    {
        isAttacking = true;
        agent.isStopped = true;

        // Face player before attacking
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        directionToPlayer.y = 0;
        Quaternion lookAtPlayer = Quaternion.LookRotation(directionToPlayer);
        
        float faceTimer = 0f;
        while (faceTimer < 0.3f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookAtPlayer, Time.deltaTime * 6f);
            faceTimer += Time.deltaTime;
            yield return null;
        }

        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Wait for attack animation
        yield return new WaitForSeconds(0.5f);

        // Deal damage to player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        // After attack, be more lively - dart away or circle
        yield return new WaitForSeconds(0.3f);
        
        isAttacking = false;
        agent.isStopped = false;
        
        // Quick reposition after attack to be more dynamic
        if (Random.value > 0.5f)
        {
            DartToSide();
        }
        else
        {
            QuickReposition();
        }
    }

    public void Kill()
    {
        if (isDead) return;
        
        isDead = true;
        if (agent != null) agent.enabled = false;

        if (animator != null)
            animator.SetTrigger("Death");

        if (GameManager.instance != null)
        {
            GameManager.instance.AddScore(1);
            GameManager.instance.GhostKilled();
        }

        // Use object pooling if available, otherwise destroy
        if (ObjectPool.Instance != null)
        {
            StartCoroutine(ReturnToPoolAfterDelay());
        }
        else
        {
            Destroy(gameObject);
        }
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
        isAttacking = false;
        isHiding = false;
        isReactingToDetection = false;
        roamTimer = 0f;
        lastLivelyActionTime = 0f;
        
        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = speed;
            agent.isStopped = false;
        }

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
        
        // Find player camera
        if (player != null)
        {
            playerCamera = player.GetComponentInChildren<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;
        }
        
        // Reinitialize room data
        InitializeRoomData();
        PickNewRoamPosition();
    }
}
