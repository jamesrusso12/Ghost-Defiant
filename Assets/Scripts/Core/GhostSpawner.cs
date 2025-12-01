using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class GhostSpawner : MonoBehaviour
{
    public float spawnTimer = 1f;
    public GameObject prefabToSpawn;

    public float minEdgeDistance = 0.3f;
    public MRUKAnchor.SceneLabels spawnLabels;
    public float normalOffset;
    public int spawnTry = 1000;

    private float timer;
    private bool isSpawning = false;
    private int ghostsSpawned = 0;
    private int ghostsToSpawn = 0;

    void Update()
    {
        if (!isSpawning) return;
        
        // Check MRUK directly with fallback to MRUKSetupManager
        if (MRUK.Instance == null || !MRUK.Instance.IsInitialized) return;

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
            Debug.LogWarning("No current room available for ghost spawning.");
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
                Vector3 spawnPos = pos + norm * normalOffset;
                spawnPos.y = 0;

                // Use object pooling if available
                if (ObjectPool.Instance != null)
                {
                    ObjectPool.Instance.SpawnFromPool("Ghost", spawnPos, Quaternion.identity);
                }
                else
                {
                    Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                }
                return;
            }
            currentTry++;
        }

        Debug.LogWarning("Failed to find valid spawn location for ghost after max tries.");
    }
}
