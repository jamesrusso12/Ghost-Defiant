using UnityEngine;
using Meta.XR.BuildingBlocks;

// Suppress obsolete warnings for RoomMeshEvent - still functional but deprecated
// TODO: Migrate to MRUK Effect Mesh building block in future update
#pragma warning disable CS0618

/// <summary>
/// Disables the MeshRenderer on Scene Mesh Building Block's Mesh Volume Prefab
/// This makes the scene mesh invisible while keeping the Building Block functional
/// NOTE: RoomMeshEvent is deprecated - consider migrating to MRUK Effect Mesh building block
/// </summary>
public class DisableSceneMeshRenderer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Disable the mesh renderer on Scene Mesh prefabs")]
    public bool disableMeshRenderer = true;
    
    [Tooltip("Also disable MeshFilter to completely remove mesh")]
    public bool disableMeshFilter = false;
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    private void Start()
    {
        // Find all RoomMeshAnchor objects (created by Scene Mesh Building Block)
        DisableSceneMeshRenderers();
        
        // Also listen for when new meshes are created
        var roomMeshEvent = FindAnyObjectByType<RoomMeshEvent>();
        if (roomMeshEvent != null)
        {
            roomMeshEvent.OnRoomMeshLoadCompleted.AddListener(OnRoomMeshLoaded);
        }
    }
    
    private void OnRoomMeshLoaded(UnityEngine.MeshFilter meshFilter)
    {
        if (enableDebugLogs)
            Debug.Log("[DisableSceneMeshRenderer] Room mesh loaded, disabling renderer...");
        
        DisableRendererOnObject(meshFilter.gameObject);
    }
    
    private void DisableSceneMeshRenderers()
    {
        // Find all RoomMeshAnchor objects in the scene
        var roomMeshAnchors = FindObjectsByType<RoomMeshAnchor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        if (enableDebugLogs)
            Debug.Log($"[DisableSceneMeshRenderer] Found {roomMeshAnchors.Length} RoomMeshAnchor objects");
        
        foreach (var anchor in roomMeshAnchors)
        {
            DisableRendererOnObject(anchor.gameObject);
        }
        
        // OPTIMIZATION: Use FindObjectsByType<MeshRenderer> instead of FindObjectsByType<GameObject>
        // FindObjectsByType<GameObject> searches ALL GameObjects - extremely expensive!
        // Since we only care about objects with MeshRenderers, search for those directly
        var allRenderers = FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var renderer in allRenderers)
        {
            if (renderer == null) continue;
            var obj = renderer.gameObject;
            if (obj.name.Contains("RoomMesh") || obj.name.Contains("SceneMesh") || obj.name.Contains("MeshVolume"))
            {
                DisableRendererOnObject(obj);
            }
        }
    }
    
    private void DisableRendererOnObject(GameObject obj)
    {
        if (obj == null) return;
        
        var meshRenderer = obj.GetComponent<MeshRenderer>();
        if (meshRenderer != null && disableMeshRenderer)
        {
            meshRenderer.enabled = false;
            if (enableDebugLogs)
                Debug.Log($"[DisableSceneMeshRenderer] Disabled MeshRenderer on {obj.name}");
        }
        
        var meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter != null && disableMeshFilter)
        {
            meshFilter.sharedMesh = null;
            if (enableDebugLogs)
                Debug.Log($"[DisableSceneMeshRenderer] Cleared MeshFilter on {obj.name}");
        }
        
        // Also check children
        foreach (Transform child in obj.transform)
        {
            DisableRendererOnObject(child.gameObject);
        }
    }
    
    // Update method to catch meshes that spawn later
    private void Update()
    {
        // Periodically check for new scene meshes (only if we haven't found any yet)
        // This is a fallback in case meshes spawn after Start()
        if (Time.frameCount % 60 == 0) // Check every 60 frames (~1 second at 60fps)
        {
            var roomMeshAnchors = FindObjectsByType<RoomMeshAnchor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var anchor in roomMeshAnchors)
            {
                var mr = anchor.GetComponent<MeshRenderer>();
                if (mr != null && mr.enabled)
                {
                    DisableRendererOnObject(anchor.gameObject);
                }
            }
        }
    }
}

#pragma warning restore CS0618

