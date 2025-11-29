using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Meta.XR.MRUtilityKit;

public class RuntimeNavmeshBuilder : MonoBehaviour
{
    private NavMeshSurface navmeshSurface;

    // Start is called before the first frame update
    void Start()
    {
        navmeshSurface = GetComponent<NavMeshSurface>();
        
        // Use direct MRUK access
        if (MRUK.Instance != null)
        {
            MRUK.Instance.RegisterSceneLoadedCallback(BuildNavmesh);
        }
        else
        {
            // Start a coroutine to wait for MRUK to be available
            StartCoroutine(WaitForMRUKAndBuild());
        }
    }
    
    private System.Collections.IEnumerator WaitForMRUKAndBuild()
    {
        // Wait for MRUK to be available
        yield return new WaitUntil(() => MRUK.Instance != null);
        
        // Register the callback
        MRUK.Instance.RegisterSceneLoadedCallback(BuildNavmesh);
    }

    public void BuildNavmesh()
    {
        StartCoroutine(BuildNavmeshRoutine());
    }

    public IEnumerator BuildNavmeshRoutine()
    {
        yield return new WaitForEndOfFrame();
        navmeshSurface.BuildNavMesh();
    }
}
