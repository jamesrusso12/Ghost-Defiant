using UnityEngine;

/// <summary>
/// Simple runtime script to hide Scene Mesh wireframe overlay on Quest device.
/// Makes the mesh material transparent instead of disabling renderer (which breaks passthrough).
/// Waits for mesh to load, then makes it transparent.
/// </summary>
public class RuntimeMeshHider : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How long to wait before hiding mesh (in seconds). Increase if passthrough breaks.")]
    [Range(3f, 10f)]
    public float waitTime = 5f;
    
    [Tooltip("Enable to hide the mesh overlay")]
    public bool hideMesh = true;
    
    private bool meshHidden = false;
    private float timer = 0f;

    private void Update()
    {
        if (!hideMesh || meshHidden) return;
        
        timer += Time.deltaTime;
        
        // Wait for specified time to ensure passthrough is fully initialized
        if (timer >= waitTime)
        {
            HideMeshOverlay();
            meshHidden = true;
        }
    }
    
    private void HideMeshOverlay()
    {
        // Find all MeshVolume or RoomMesh objects
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        int hiddenCount = 0;
        foreach (GameObject obj in allObjects)
        {
            // Look for Scene Mesh objects (created by RoomMeshController)
            if (obj.name.Contains("MeshVolume") || 
                obj.name.Contains("RoomMesh") || 
                obj.name.Contains("SceneMesh"))
            {
                MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                if (renderer != null && renderer.enabled)
                {
                    // Instead of disabling renderer (breaks passthrough), make material transparent
                    MakeMaterialTransparent(renderer);
                    hiddenCount++;
                    Debug.Log($"[RuntimeMeshHider] Made mesh transparent on {obj.name} (renderer still active for passthrough)");
                }
            }
        }
        
        if (hiddenCount > 0)
        {
            Debug.Log($"[RuntimeMeshHider] Successfully made {hiddenCount} mesh(es) transparent after {waitTime}s delay");
        }
        else
        {
            Debug.LogWarning("[RuntimeMeshHider] No mesh renderers found. Mesh may not be loaded yet.");
        }
    }
    
    private void MakeMaterialTransparent(MeshRenderer renderer)
    {
        if (renderer == null) return;
        
        // Get the material (create instance to avoid modifying shared material)
        Material mat = renderer.material;
        if (mat == null) return;
        
        // Try to make it transparent by setting alpha to 0
        // Check for different material property names
        if (mat.HasProperty("_Color"))
        {
            Color color = mat.GetColor("_Color");
            color.a = 0f; // Fully transparent
            mat.SetColor("_Color", color);
        }
        else if (mat.HasProperty("_BaseColor"))
        {
            Color color = mat.GetColor("_BaseColor");
            color.a = 0f;
            mat.SetColor("_BaseColor", color);
        }
        else if (mat.HasProperty("_TintColor"))
        {
            Color color = mat.GetColor("_TintColor");
            color.a = 0f;
            mat.SetColor("_TintColor", color);
        }
        
        // Try to enable transparency mode if shader supports it
        if (mat.HasProperty("_Surface"))
        {
            // URP Surface Type
            mat.SetFloat("_Surface", 1); // 1 = Transparent
        }
        
        // Set render queue to transparent
        mat.renderQueue = 3000; // Transparent queue
        
        // Enable alpha blending keywords if available
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        
        Debug.Log($"[RuntimeMeshHider] Made material transparent: {mat.name}");
    }
    
    /// <summary>
    /// Call this manually if you want to hide mesh immediately (risky - may break passthrough)
    /// </summary>
    [ContextMenu("Hide Mesh Now")]
    public void HideMeshNow()
    {
        HideMeshOverlay();
        meshHidden = true;
    }
}

