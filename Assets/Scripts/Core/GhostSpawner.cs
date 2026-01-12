using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Meta.XR.MRUtilityKit;

public class GhostSpawner : MonoBehaviour
{
    public float spawnTimer = 1f;
    public GameObject prefabToSpawn;

    public float minEdgeDistance = 0.3f;
    public MRUKAnchor.SceneLabels spawnLabels;
    public float normalOffset;
    public int spawnTry = 1000;

    [Header("Spawn Position")]
    [Tooltip("If true, overrides the generated position's Y with Spawn Y. Keep this ON if your ghosts use NavMesh on the floor.")]
    public bool overrideSpawnY = true;

    [Tooltip("Y to spawn at when Override Spawn Y is enabled. For NavMesh-on-floor setups, this is usually 0.")]
    public float spawnY = 0f;

    [Tooltip("Clamp the absolute value of Normal Offset to this max (prevents accidentally pushing spawns far into/away from surfaces).")]
    public float maxAbsNormalOffset = 0.5f;

    [Tooltip("If true, we snap the spawn position onto the NavMesh (recommended).")]
    public bool snapToNavMesh = true;

    [Tooltip("Radius used when snapping to NavMesh.")]
    public float navMeshSampleRadius = 2f;

    [Header("Fallback (if MRUK can't provide a position)")]
    [Tooltip("If true, spawns near the player on the NavMesh when MRUK isn't ready or no surface positions are found.")]
    public bool allowFallbackSpawnNearPlayer = true;

    public float fallbackRadius = 4f;

    [Header("Debug")]
    public bool debugLogging = false;

    private float timer;
    private bool isSpawning = false;
    private int ghostsSpawned = 0;
    private int ghostsToSpawn = 0;
    private bool loggedWaitingForMRUK = false;

    void Update()
    {
        if (!isSpawning) return;
        
        // Check MRUK directly with fallback to MRUKSetupManager
        if (MRUK.Instance == null || !MRUK.Instance.IsInitialized)
        {
            if (debugLogging && !loggedWaitingForMRUK)
            {
                Debug.LogWarning("[GhostSpawner] Waiting for MRUK to initialize. No ghosts will spawn until room setup is available.");
                loggedWaitingForMRUK = true;
            }

            // Optional fallback so the game is playable even if MRUK isn't ready in a build yet.
            if (allowFallbackSpawnNearPlayer)
            {
                timer += Time.deltaTime;
                if (timer >= spawnTimer && ghostsSpawned < ghostsToSpawn)
                {
                    SpawnGhostFallbackNearPlayer();
                    ghostsSpawned++;
                    timer = 0f;
                }
            }

            return;
        }

        loggedWaitingForMRUK = false;

        timer += Time.deltaTime;
        if (timer >= spawnTimer && ghostsSpawned < ghostsToSpawn)
        {
            SpawnGhost();
            ghostsSpawned++;
            timer = 0f;
        }
    }

    public void BeginRound(int roundNumber)
    {
        isSpawning = true;
        ghostsSpawned = 0;
        ghostsToSpawn = roundNumber * 15;
        timer = 0f;

        // Adjust spawn rate based on round (faster each round)
        spawnTimer = Mathf.Max(0.2f, 1f - (roundNumber * 0.1f));
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void SpawnGhost()
    {
        // Use MRUK directly with fallback to MRUKSetupManager
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            if (debugLogging) Debug.LogWarning("[GhostSpawner] No current room available for ghost spawning.");
            if (allowFallbackSpawnNearPlayer) SpawnGhostFallbackNearPlayer();
            return;
        }

        int currentTry = 0;

        while (currentTry < spawnTry)
        {
            bool hasFoundPosition = room.GenerateRandomPositionOnSurface(
                MRUK.SurfaceType.VERTICAL,
                minEdgeDistance,
                new LabelFilter(spawnLabels),
                out Vector3 pos,
                out Vector3 norm);

            if (hasFoundPosition)
            {
                float absOffset = Mathf.Abs(normalOffset);
                if (maxAbsNormalOffset > 0f) absOffset = Mathf.Min(absOffset, maxAbsNormalOffset);

                Vector3 spawnPos = pos + norm * absOffset;

                if (overrideSpawnY) spawnPos.y = spawnY;

                if (snapToNavMesh)
                {
                    NavMeshHit navHit;
                    if (NavMesh.SamplePosition(spawnPos, out navHit, navMeshSampleRadius, NavMesh.AllAreas))
                    {
                        spawnPos = navHit.position;
                    }
                    else if (debugLogging)
                    {
                        Debug.LogWarning($"[GhostSpawner] Found MRUK surface point but couldn't snap to NavMesh (radius {navMeshSampleRadius}).");
                    }
                }

                // Use object pooling if available
                if (ObjectPool.Instance != null)
                {
                    GameObject spawned = ObjectPool.Instance.SpawnFromPool("Ghost", spawnPos, Quaternion.identity);
                    if (spawned == null)
                    {
                        // Pool exists but doesn't have the Ghost tag configured.
                        if (prefabToSpawn != null)
                        {
                            Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                        }
                        else if (debugLogging)
                        {
                            Debug.LogError("[GhostSpawner] ObjectPool exists but has no 'Ghost' pool, and Prefab To Spawn is not set.");
                        }
                    }
                }
                else
                {
                    if (prefabToSpawn != null)
                    {
                        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                    }
                    else if (debugLogging)
                    {
                        Debug.LogError("[GhostSpawner] Prefab To Spawn is not set; cannot spawn ghost.");
                    }
                }
                return;
            }
            currentTry++;
        }

        if (debugLogging) Debug.LogWarning("[GhostSpawner] Failed to find valid MRUK spawn location for ghost after max tries.");
        if (allowFallbackSpawnNearPlayer) SpawnGhostFallbackNearPlayer();
    }

    // OPTIMIZATION: Cache player reference to avoid expensive FindGameObjectWithTag() on every fallback spawn
    private static GameObject cachedPlayer = null;
    
    private void SpawnGhostFallbackNearPlayer()
    {
        // OPTIMIZATION: Cache player reference (only find once)
        if (cachedPlayer == null)
        {
            cachedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (cachedPlayer != null && debugLogging)
                Debug.Log($"[PERF] GhostSpawner: Cached Player reference ({cachedPlayer.name})");
        }
        
        GameObject player = cachedPlayer;
        if (player == null)
        {
            if (debugLogging) Debug.LogWarning("[GhostSpawner] Fallback spawn failed: Player tag not found.");
            return;
        }

        Vector3 basePos = player.transform.position;
        Vector3 randomOffset = Random.insideUnitSphere * fallbackRadius;
        randomOffset.y = 0f;
        Vector3 desiredPos = basePos + randomOffset;

        Vector3 spawnPos = desiredPos;
        if (overrideSpawnY) spawnPos.y = spawnY;

        if (snapToNavMesh)
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(desiredPos, out navHit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                spawnPos = navHit.position;
            }
        }

        if (ObjectPool.Instance != null)
        {
            GameObject spawned = ObjectPool.Instance.SpawnFromPool("Ghost", spawnPos, Quaternion.identity);
            if (spawned != null) return;
        }

        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        }
        else if (debugLogging)
        {
            Debug.LogError("[GhostSpawner] Fallback spawn failed: Prefab To Spawn is not set and pool spawn failed.");
        }
    }
}
