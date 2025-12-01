using UnityEngine;

/// <summary>
/// Simple component to add to virtual environment objects.
/// Automatically configures them to be occluded by the room mesh.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class VirtualObjectOccluder : MonoBehaviour
{
    [Header("Occlusion Settings")]
    [Tooltip("Reference to RoomOcclusionManager (auto-finds if not set)")]
    public RoomOcclusionManager occlusionManager;
    
    [Tooltip("Stencil value for room interior (should match RoomOcclusionManager)")]
    [SerializeField] private int roomInteriorStencilValue = 1;
    
    [Tooltip("Use stencil-based occlusion")]
    [SerializeField] private bool useStencilOcclusion = true;

    private Material[] originalMaterials;
    private Material[] occlusionMaterials;
    private Renderer objectRenderer;

    void Start()
    {
        // CRITICAL: Ensure cameras can see VirtualEnvironment layer
        FixCameraCullingMasks();
        
        // Find occlusion manager if not set
        if (occlusionManager == null)
        {
            occlusionManager = FindFirstObjectByType<RoomOcclusionManager>();
        }

        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError($"[VirtualObjectOccluder] No Renderer found on {gameObject.name}!");
            return;
        }

        // Store original materials
        originalMaterials = objectRenderer.materials;
        occlusionMaterials = new Material[originalMaterials.Length];

        // Create occlusion-configured materials
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            if (originalMaterials[i] != null)
            {
                occlusionMaterials[i] = new Material(originalMaterials[i]);
                ConfigureMaterialForOcclusion(occlusionMaterials[i]);
            }
        }

        // Apply occlusion materials
        objectRenderer.materials = occlusionMaterials;

        // Set layer to VirtualEnvironment if it exists, otherwise Default
        int virtualEnvLayer = LayerMask.NameToLayer("VirtualEnvironment");
        if (virtualEnvLayer != -1)
        {
            gameObject.layer = virtualEnvLayer;
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            Debug.LogWarning($"[VirtualObjectOccluder] VirtualEnvironment layer not found, using Default layer");
        }

        Debug.Log($"[VirtualObjectOccluder] Configured {gameObject.name} for room occlusion");
    }

    private void ConfigureMaterialForOcclusion(Material mat)
    {
        if (mat == null) return;

        // CRITICAL: Ensure depth testing is enabled and correct
        // Virtual objects should be hidden when behind room mesh (depth test)
        if (mat.HasProperty("_ZTest"))
        {
            // LessEqual means: render if depth is less than or equal to existing depth
            // This ensures objects behind the room mesh are hidden
            mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
        }
        
        // Ensure depth writing is enabled for proper depth sorting
        if (mat.HasProperty("_ZWrite"))
        {
            mat.SetFloat("_ZWrite", 1f);
        }
        
        // CRITICAL: Ensure render queue is after room mesh (so room mesh depth is written first)
        // Room mesh renders at 2000 (opaque geometry), virtual objects should render after
        if (mat.renderQueue < 2500) // If it's in geometry queue
        {
            mat.renderQueue = 2500; // Render after room mesh (which renders at ~2000)
        }
        
        // Ensure material respects depth buffer even if transparent
        if (mat.HasProperty("_Surface"))
        {
            float surfaceType = mat.GetFloat("_Surface");
            if (surfaceType > 0.5f) // Transparent
            {
                // Transparent materials can still test depth
                if (mat.HasProperty("_ZTest"))
                {
                    mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
                }
            }
        }

        if (useStencilOcclusion)
        {
            // Configure stencil test to hide when inside room
            if (mat.HasProperty("_StencilRef"))
            {
                mat.SetInt("_StencilRef", 0);
                mat.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.NotEqual);
                mat.SetInt("_StencilReadMask", roomInteriorStencilValue);
            }
        }
    }

    /// <summary>
    /// Ensures all cameras can see the VirtualEnvironment layer
    /// </summary>
    private void FixCameraCullingMasks()
    {
        int virtualEnvLayer = LayerMask.NameToLayer("VirtualEnvironment");
        if (virtualEnvLayer == -1)
        {
            // Layer doesn't exist, skip
            return;
        }
        
        int virtualEnvLayerMask = 1 << virtualEnvLayer;
        
        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        
        foreach (Camera cam in allCameras)
        {
            if (cam == null) continue;
            
            // Add VirtualEnvironment layer to culling mask if not already included
            if ((cam.cullingMask & virtualEnvLayerMask) == 0)
            {
                cam.cullingMask |= virtualEnvLayerMask;
            }
        }
    }

    void OnDestroy()
    {
        // Clean up materials
        if (occlusionMaterials != null)
        {
            foreach (Material mat in occlusionMaterials)
            {
                if (mat != null)
                    DestroyImmediate(mat);
            }
        }
    }
}

