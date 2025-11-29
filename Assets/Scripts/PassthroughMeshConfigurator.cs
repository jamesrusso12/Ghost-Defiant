using UnityEngine;
using Meta.XR.MRUtilityKit;

/// <summary>
/// Configures the destructible mesh for mixed reality passthrough gameplay
/// Makes mesh transparent so passthrough shows through, with skybox revealed when destroyed
/// </summary>
public class PassthroughMeshConfigurator : MonoBehaviour
{
    [Header("Mesh Configuration")]
    [Tooltip("Material to use for the destructible mesh (should be transparent)")]
    public Material destructibleMeshMaterial;
    
    [Tooltip("Make mesh completely transparent (0) or slightly visible for debugging (0.1)")]
    [Range(0f, 1f)]
    public float meshAlpha = 0f;
    
    [Header("References")]
    public DestructibleGlobalMeshSpawner meshSpawner;
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    public bool showMeshForDebugging = false;
    
    void Start()
    {
        if (meshSpawner != null)
        {
            meshSpawner.OnDestructibleMeshCreated.AddListener(OnMeshCreated);
            
            if (enableDebugLogs)
                Debug.Log("[PassthroughMeshConfigurator] Listening for mesh creation...");
        }
        else
        {
            Debug.LogError("[PassthroughMeshConfigurator] Mesh Spawner reference not set!");
        }
    }
    
    void Update()
    {
        // Runtime debugging toggle
        if (showMeshForDebugging)
        {
            SetMeshAlpha(0.3f);
        }
        else
        {
            SetMeshAlpha(meshAlpha);
        }
    }
    
    private void OnMeshCreated(DestructibleMeshComponent meshComponent)
    {
        if (enableDebugLogs)
            Debug.Log("[PassthroughMeshConfigurator] Destructible mesh created, configuring...");
        
        // Configure the mesh for transparency
        ConfigureMeshTransparency(meshComponent);
        
        // Set up proper rendering
        ConfigureRendering(meshComponent);
    }
    
    private void ConfigureMeshTransparency(DestructibleMeshComponent meshComponent)
    {
        if (meshComponent == null) return;
        
        // Get all mesh renderers in the destructible mesh
        MeshRenderer[] renderers = meshComponent.GetComponentsInChildren<MeshRenderer>();
        
        if (enableDebugLogs)
            Debug.Log($"[PassthroughMeshConfigurator] Found {renderers.Length} mesh renderers");
        
        foreach (MeshRenderer renderer in renderers)
        {
            if (destructibleMeshMaterial != null)
            {
                // Assign the transparent material
                renderer.material = destructibleMeshMaterial;
                
                // Set alpha value
                if (renderer.material.HasProperty("_Alpha"))
                {
                    renderer.material.SetFloat("_Alpha", meshAlpha);
                }
                
                // Also set color alpha if material has _Color or _BaseColor
                if (renderer.material.HasProperty("_Color"))
                {
                    Color col = renderer.material.GetColor("_Color");
                    col.a = meshAlpha;
                    renderer.material.SetColor("_Color", col);
                }
                else if (renderer.material.HasProperty("_BaseColor"))
                {
                    Color col = renderer.material.GetColor("_BaseColor");
                    col.a = meshAlpha;
                    renderer.material.SetColor("_BaseColor", col);
                }
                
                if (enableDebugLogs)
                    Debug.Log($"[PassthroughMeshConfigurator] Configured renderer: {renderer.name}");
            }
            else
            {
                Debug.LogWarning("[PassthroughMeshConfigurator] No destructible mesh material assigned!");
            }
        }
    }
    
    private void ConfigureRendering(DestructibleMeshComponent meshComponent)
    {
        if (meshComponent == null) return;
        
        // Ensure proper sorting order for mixed reality
        MeshRenderer[] renderers = meshComponent.GetComponentsInChildren<MeshRenderer>();
        
        foreach (MeshRenderer renderer in renderers)
        {
            // Set rendering layer mask (optional, for advanced setups)
            // renderer.renderingLayerMask = 1; // Default layer
            
            // Ensure shadows are off for transparent mesh
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }
    
    /// <summary>
    /// Runtime method to change mesh alpha (useful for debugging or effects)
    /// </summary>
    public void SetMeshAlpha(float alpha)
    {
        meshAlpha = Mathf.Clamp01(alpha);
        
        if (destructibleMeshMaterial != null)
        {
            if (destructibleMeshMaterial.HasProperty("_Alpha"))
            {
                destructibleMeshMaterial.SetFloat("_Alpha", meshAlpha);
            }
            
            if (destructibleMeshMaterial.HasProperty("_Color"))
            {
                Color col = destructibleMeshMaterial.GetColor("_Color");
                col.a = meshAlpha;
                destructibleMeshMaterial.SetColor("_Color", col);
            }
            else if (destructibleMeshMaterial.HasProperty("_BaseColor"))
            {
                Color col = destructibleMeshMaterial.GetColor("_BaseColor");
                col.a = meshAlpha;
                destructibleMeshMaterial.SetColor("_BaseColor", col);
            }
        }
    }
    
    /// <summary>
    /// Get diagnostic information about current mesh state
    /// </summary>
    public string GetDiagnostics()
    {
        string info = "[PassthroughMeshConfigurator]\n";
        info += $"  Mesh Alpha: {meshAlpha}\n";
        info += $"  Material: {(destructibleMeshMaterial != null ? destructibleMeshMaterial.name : "None")}\n";
        info += $"  Mesh Spawner: {(meshSpawner != null ? "Connected" : "Missing")}\n";
        return info;
    }
}

