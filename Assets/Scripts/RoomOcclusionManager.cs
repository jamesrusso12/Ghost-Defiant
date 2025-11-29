using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Meta.XR.MRUtilityKit;

/// <summary>
/// Manages occlusion so virtual objects outside the room don't show inside the real room.
/// Uses the destructible room mesh as an occlusion mesh.
/// </summary>
public class RoomOcclusionManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The DestructibleGlobalMeshSpawner that creates the room mesh")]
    public DestructibleGlobalMeshSpawner meshSpawner;
    
    [Header("Occlusion Settings")]
    [Tooltip("Layer for virtual environment objects that should be occluded by room mesh")]
    [SerializeField] private string virtualEnvironmentLayer = "VirtualEnvironment";
    
    [Tooltip("Enable stencil-based occlusion (more reliable but requires shader support)")]
    [SerializeField] private bool useStencilOcclusion = false; // Disabled by default - use depth-based instead
    
    [Tooltip("Stencil reference value for room interior (objects inside this value are hidden)")]
    [SerializeField] private int roomInteriorStencilValue = 1;
    
    [Tooltip("Don't replace room mesh materials - use depth-based occlusion only")]
    [SerializeField] private bool preserveRoomMeshMaterials = true;
    
    [Header("Debug")]
    [Tooltip("Show debug info in console")]
    [SerializeField] private bool debugMode = false;

    private DestructibleMeshComponent currentMeshComponent;
    private Material occlusionMaterial;
    private int virtualEnvironmentLayerIndex;

    void Start()
    {
        virtualEnvironmentLayerIndex = LayerMask.NameToLayer(virtualEnvironmentLayer);
        
        if (meshSpawner == null)
        {
            Debug.LogError("[RoomOcclusionManager] MeshSpawner is NULL! Cannot set up occlusion.");
            return;
        }

        // Subscribe to mesh creation events
        meshSpawner.OnDestructibleMeshCreated.AddListener(SetupOcclusionMesh);
        
        if (debugMode)
            Debug.Log($"[RoomOcclusionManager] Initialized. Virtual Environment Layer: {virtualEnvironmentLayer}");
    }

    void OnDestroy()
    {
        if (meshSpawner != null)
            meshSpawner.OnDestructibleMeshCreated.RemoveListener(SetupOcclusionMesh);
    }

    /// <summary>
    /// Sets up the room mesh segments to act as occlusion meshes
    /// </summary>
    private void SetupOcclusionMesh(DestructibleMeshComponent component)
    {
        if (component == null)
        {
            Debug.LogError("[RoomOcclusionManager] Component is NULL!");
            return;
        }

        currentMeshComponent = component;
        
        // Get all mesh segments
        System.Collections.Generic.List<GameObject> segments = new System.Collections.Generic.List<GameObject>();
        component.GetDestructibleMeshSegments(segments);
        segments.RemoveAll(s => s == null);

        if (debugMode)
            Debug.Log($"[RoomOcclusionManager] Setting up occlusion for {segments.Count} mesh segments");

        // Create occlusion material if needed
        if (occlusionMaterial == null)
        {
            CreateOcclusionMaterial();
        }

        // Configure each segment for occlusion
        foreach (GameObject segment in segments)
        {
            if (segment == null) continue;
            
            SetupSegmentOcclusion(segment);
        }

        if (debugMode)
            Debug.Log("[RoomOcclusionManager] Occlusion setup complete!");
    }

    /// <summary>
    /// Creates a material for the occlusion mesh (only if not preserving original materials)
    /// </summary>
    private void CreateOcclusionMaterial()
    {
        // Only create if we're not preserving original materials
        if (preserveRoomMeshMaterials)
        {
            if (debugMode)
                Debug.Log("[RoomOcclusionManager] Preserving room mesh materials - using depth-based occlusion only");
            return;
        }

        // Try to use URP's SpatialMappingOcclusion shader
        Shader occlusionShader = Shader.Find("Universal Render Pipeline/SpatialMappingOcclusion");
        
        if (occlusionShader == null)
        {
            // Fallback to a simple shader that writes to depth/stencil
            occlusionShader = Shader.Find("Universal Render Pipeline/Unlit");
            Debug.LogWarning("[RoomOcclusionManager] SpatialMappingOcclusion shader not found, using Unlit fallback");
        }

        occlusionMaterial = new Material(occlusionShader);
        occlusionMaterial.name = "RoomOcclusionMaterial";
        
        // Configure for occlusion rendering
        if (useStencilOcclusion)
        {
            // Write to stencil buffer to mark room interior
            occlusionMaterial.SetFloat("_StencilWriteMask", roomInteriorStencilValue);
            occlusionMaterial.SetFloat("_StencilRef", roomInteriorStencilValue);
            occlusionMaterial.SetInt("_StencilComp", (int)CompareFunction.Always);
            occlusionMaterial.SetInt("_StencilPass", (int)StencilOp.Replace);
        }
        
        // Make it write to depth buffer for depth-based occlusion
        occlusionMaterial.SetFloat("_ZWrite", 1f);
        occlusionMaterial.SetInt("_ZTest", (int)CompareFunction.LessEqual);
        
        // Make it invisible (occlusion only, no visual rendering)
        if (occlusionMaterial.HasProperty("_Color"))
        {
            Color occluderColor = new Color(0, 0, 0, 0); // Fully transparent
            occlusionMaterial.SetColor("_Color", occluderColor);
        }
        
        // Set render queue to render before transparent objects
        occlusionMaterial.renderQueue = (int)RenderQueue.Geometry;

        if (debugMode)
            Debug.Log("[RoomOcclusionManager] Created occlusion material");
    }

    /// <summary>
    /// Configures a single mesh segment for occlusion
    /// </summary>
    private void SetupSegmentOcclusion(GameObject segment)
    {
        if (segment == null) return;

        MeshRenderer renderer = segment.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = segment.AddComponent<MeshRenderer>();
        }

        // CRITICAL: Add a second renderer with depth-only material for occlusion
        // This ensures depth writing even if the original material is transparent
        GameObject occlusionHelper = new GameObject($"{segment.name}_OcclusionHelper");
        occlusionHelper.transform.SetParent(segment.transform);
        occlusionHelper.transform.localPosition = Vector3.zero;
        occlusionHelper.transform.localRotation = Quaternion.identity;
        occlusionHelper.transform.localScale = Vector3.one;
        occlusionHelper.layer = segment.layer; // Same layer as segment
        
        MeshFilter occlusionFilter = occlusionHelper.AddComponent<MeshFilter>();
        MeshFilter originalFilter = segment.GetComponent<MeshFilter>();
        if (originalFilter != null && originalFilter.sharedMesh != null)
        {
            occlusionFilter.sharedMesh = originalFilter.sharedMesh;
        }
        
        MeshRenderer occlusionRenderer = occlusionHelper.AddComponent<MeshRenderer>();
        
        // Create depth-only material
        Material depthOnlyMat = CreateDepthOnlyMaterial();
        occlusionRenderer.material = depthOnlyMat;
        occlusionRenderer.shadowCastingMode = ShadowCastingMode.Off;
        occlusionRenderer.receiveShadows = false;
        
        // Hide the helper (it's only for depth writing)
        occlusionHelper.hideFlags = HideFlags.HideAndDontSave;

        // CRITICAL: Configure original material to write depth for occlusion
        Material originalMat = renderer.material; // Creates instance automatically
        if (originalMat != null)
        {
            // Force depth writing - CRITICAL for occlusion
            if (originalMat.HasProperty("_ZWrite"))
            {
                originalMat.SetFloat("_ZWrite", 1f);
            }
            
            // Also check for _SceneMeshZWrite property (used by passthrough shaders)
            if (originalMat.HasProperty("_SceneMeshZWrite"))
            {
                originalMat.SetFloat("_SceneMeshZWrite", 1f);
            }
            
            // Ensure proper depth testing
            if (originalMat.HasProperty("_ZTest"))
            {
                originalMat.SetInt("_ZTest", (int)CompareFunction.LessEqual);
            }
            
            // Set render queue to render before virtual objects
            if (originalMat.renderQueue >= 2500)
            {
                originalMat.renderQueue = 2000; // Force into opaque geometry queue
            }
            
            // If material is transparent, we still need depth writing
            // Disable color writing if fully transparent (depth-only)
            if (originalMat.HasProperty("_Color"))
            {
                Color col = originalMat.GetColor("_Color");
                if (col.a < 0.01f && originalMat.HasProperty("_ColorMask"))
                {
                    originalMat.SetInt("_ColorMask", 0); // No color, depth only
                }
            }
        }

        // Set to a layer that won't interfere with other rendering
        if (segment.layer == LayerMask.NameToLayer("Ignore Raycast"))
        {
            segment.layer = LayerMask.NameToLayer("Default");
        }

        if (debugMode)
            Debug.Log($"[RoomOcclusionManager] Configured occlusion for segment: {segment.name} (added depth-only helper)");
    }
    
    /// <summary>
    /// Creates a material that only writes to depth buffer (invisible)
    /// </summary>
    private Material CreateDepthOnlyMaterial()
    {
        // Use a simple shader that writes depth
        Shader depthShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (depthShader == null)
        {
            depthShader = Shader.Find("Hidden/InternalDepthNormalsTexture");
        }
        
        Material depthMat = new Material(depthShader);
        depthMat.name = "RoomDepthOccluder";
        
        // Make it completely invisible
        if (depthMat.HasProperty("_Color"))
        {
            depthMat.SetColor("_Color", new Color(0, 0, 0, 0)); // Fully transparent
        }
        
        // CRITICAL: Enable depth writing
        depthMat.SetFloat("_ZWrite", 1f);
        depthMat.SetInt("_ZTest", (int)CompareFunction.LessEqual);
        
        // Set to render early in geometry queue
        depthMat.renderQueue = (int)RenderQueue.Geometry - 100; // Render before other geometry
        
        // Disable color writing (depth only)
        depthMat.SetInt("_ColorMask", 0); // No color channels
        
        return depthMat;
    }

    /// <summary>
    /// Call this method to configure a virtual object to be occluded by the room mesh
    /// </summary>
    public void ConfigureVirtualObjectForOcclusion(GameObject virtualObject)
    {
        if (virtualObject == null) return;

        // Set layer
        virtualObject.layer = virtualEnvironmentLayerIndex;

        // Configure all renderers in the object hierarchy
        Renderer[] renderers = virtualObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material != null)
            {
                Material mat = renderer.material;
                
                if (useStencilOcclusion)
                {
                    // Configure stencil test to hide when inside room
                    if (mat.HasProperty("_StencilRef"))
                    {
                        mat.SetInt("_StencilRef", 0);
                        mat.SetInt("_StencilComp", (int)CompareFunction.NotEqual); // Don't render if stencil matches room interior
                        mat.SetInt("_StencilReadMask", roomInteriorStencilValue);
                    }
                }
                
                // Ensure depth test works correctly
                if (mat.HasProperty("_ZTest"))
                {
                    mat.SetInt("_ZTest", (int)CompareFunction.LessEqual); // Standard depth test
                }
            }
        }

        if (debugMode)
            Debug.Log($"[RoomOcclusionManager] Configured virtual object for occlusion: {virtualObject.name}");
    }

    /// <summary>
    /// Updates occlusion when a segment is destroyed (reconfigures remaining segments)
    /// </summary>
    public void OnSegmentDestroyed(GameObject destroyedSegment)
    {
        if (currentMeshComponent != null)
        {
            // Re-setup occlusion for remaining segments
            SetupOcclusionMesh(currentMeshComponent);
        }
    }
}

