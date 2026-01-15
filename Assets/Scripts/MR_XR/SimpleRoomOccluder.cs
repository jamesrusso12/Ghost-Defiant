using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Meta.XR.MRUtilityKit;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Simpler alternative: Configures room mesh to write depth and virtual objects to respect it.
/// Add this to your scene and assign your virtual environment parent object.
/// </summary>
public class SimpleRoomOccluder : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Parent GameObject containing all virtual environment objects")]
    public GameObject virtualEnvironmentParent;
    
    [Tooltip("DestructibleGlobalMeshSpawner reference")]
    public DestructibleGlobalMeshSpawner meshSpawner;
    
    [Header("Settings")]
    [Tooltip("Force all virtual objects to respect depth occlusion")]
    [SerializeField] private bool configureOnStart = true;
    
    [Tooltip("Debug mode")]
    [SerializeField] private bool debugMode = false;
    
    [Header("On-Screen Debug Display")]
    [Tooltip("Show debug info on-screen in VR (no external tools needed)")]
    [SerializeField] private bool useOnScreenDebug = true;
    
    [Header("Depth Writing Fix")]
    [Tooltip("Add depth-only renderer if passthrough shader doesn't support depth writing")]
    [SerializeField] private bool addDepthOnlyRenderer = true;

    [Header("Occlusion Fine-Tuning")]
    [Tooltip("Depth bias to prevent z-fighting and environment peeking (0.0001-0.001 recommended)")]
    [SerializeField] private float depthBias = 0.0005f;

    [Tooltip("How often to verify mesh configuration is intact (seconds, 0 = disable, 5-10 recommended for performance)")]
    [SerializeField] private float verificationInterval = 10f;

    [Tooltip("Enable camera depth texture (required for proper occlusion)")]
    [SerializeField] private bool ensureCameraDepthTexture = true;

    [Header("Performance")]
    [Tooltip("Skip verification entirely - best performance, but won't auto-fix if mesh changes")]
    [SerializeField] private bool disableVerification = true;

    private OnScreenDebugDisplay debugDisplay;
    private DestructibleMeshComponent currentMeshComponent;
    private Dictionary<MeshRenderer, Material> configuredMaterials = new Dictionary<MeshRenderer, Material>();
    private float lastVerificationTime;
    private bool isConfigured = false;

    void Start()
    {
        // Setup on-screen debug display
        if (useOnScreenDebug)
        {
            SetupDebugDisplay();
        }

        LogToDisplay("===== STARTING DEPTH OCCLUSION SETUP =====");
        Debug.Log("[SimpleRoomOccluder] ===== STARTING DEPTH OCCLUSION SETUP =====");

        // CRITICAL: Ensure cameras can see VirtualEnvironment layer and have depth texture
        FixCameraCullingMasks();
        if (ensureCameraDepthTexture)
        {
            EnsureCameraDepthTexture();
        }

        // CRITICAL: Ensure URP renderer includes VirtualEnvironment layer
        FixURPRendererLayerMasks();

        if (configureOnStart && virtualEnvironmentParent != null)
        {
            ConfigureAllVirtualObjects();
        }
        else if (virtualEnvironmentParent == null)
        {
            LogToDisplay("ERROR: Virtual Environment Parent is NULL!", Color.red);
            Debug.LogError("[SimpleRoomOccluder] Virtual Environment Parent is NULL! Virtual objects will not be configured for occlusion.");
        }

        if (meshSpawner != null)
        {
            meshSpawner.OnDestructibleMeshCreated.AddListener(OnRoomMeshCreated);
            LogToDisplay("✓ Subscribed to mesh creation event");
            Debug.Log("[SimpleRoomOccluder] ✓ Subscribed to OnDestructibleMeshCreated event");

            // Also try to configure existing mesh if it already exists
            StartCoroutine(CheckForExistingMesh());
        }
        else
        {
            LogToDisplay("ERROR: Mesh Spawner is NULL!", Color.red);
            Debug.LogError("[SimpleRoomOccluder] ✗ Mesh Spawner is NULL! Room mesh depth configuration will not work automatically. Assign it in Inspector!");
        }

        lastVerificationTime = Time.time;

        LogToDisplay("===== SETUP COMPLETE =====");
        Debug.Log("[SimpleRoomOccluder] ===== DEPTH OCCLUSION SETUP COMPLETE =====");
    }

    void Update()
    {
        // Periodic verification to ensure configuration hasn't been lost
        // PERFORMANCE: This can cause lag spikes, consider disabling if not needed
        if (!disableVerification && verificationInterval > 0 && Time.time - lastVerificationTime >= verificationInterval)
        {
            lastVerificationTime = Time.time;
            VerifyAndReapplyConfiguration();
        }
    }
    
    private void SetupDebugDisplay()
    {
        // Try to find existing display
        debugDisplay = FindFirstObjectByType<OnScreenDebugDisplay>();
        
        if (debugDisplay == null)
        {
            // Create new display
            GameObject displayObject = new GameObject("OnScreenDebugDisplay");
            debugDisplay = displayObject.AddComponent<OnScreenDebugDisplay>();
            debugDisplay.showDisplay = true;
            debugDisplay.fontSize = 20;
            debugDisplay.maxLines = 15;
        }
    }
    
    private void LogToDisplay(string message, Color? color = null)
    {
        if (debugDisplay != null)
        {
            debugDisplay.AddLog(message, color);
        }
    }
    
    /// <summary>
    /// Ensures all cameras have depth texture enabled for proper occlusion
    /// </summary>
    private void EnsureCameraDepthTexture()
    {
        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        int fixedCount = 0;

        foreach (Camera cam in allCameras)
        {
            if (cam == null) continue;

            if (cam.depthTextureMode == DepthTextureMode.None)
            {
                cam.depthTextureMode = DepthTextureMode.Depth;
                fixedCount++;
                Debug.Log($"[SimpleRoomOccluder] ✓ Enabled depth texture on camera: {cam.name}");
            }
            else if ((cam.depthTextureMode & DepthTextureMode.Depth) == 0)
            {
                cam.depthTextureMode |= DepthTextureMode.Depth;
                fixedCount++;
                Debug.Log($"[SimpleRoomOccluder] ✓ Added depth mode to camera: {cam.name}");
            }
        }

        if (fixedCount > 0)
        {
            LogToDisplay($"✓ Enabled depth texture on {fixedCount} camera(s)", Color.green);
        }
    }

    /// <summary>
    /// Periodically verifies mesh configuration and re-applies if needed
    /// PERFORMANCE: This method is expensive and can cause lag spikes
    /// </summary>
    private void VerifyAndReapplyConfiguration()
    {
        // PERFORMANCE: Early exit if disabled
        if (disableVerification) return;

        if (!isConfigured || currentMeshComponent == null)
        {
            // Only search for mesh if we don't have one yet
            // PERFORMANCE: Avoid FindFirstObjectByType during gameplay
            return;
        }

        // PERFORMANCE: Check a small subset of materials instead of all
        // Only check if any renderer became null (destroyed)
        int nullCount = 0;
        foreach (var kvp in configuredMaterials)
        {
            if (kvp.Key == null)
            {
                nullCount++;
                if (nullCount > 3) break; // Early exit after finding a few nulls
            }
        }

        // If segments were destroyed, don't reconfigure - just let them be gone
        // Reconfiguring is too expensive and causes lag spikes
        if (nullCount > 0 && debugMode)
        {
            Debug.Log($"[SimpleRoomOccluder] {nullCount} segments destroyed (expected during gameplay)");
        }
    }

    /// <summary>
    /// Checks for existing room mesh after a short delay (in case mesh was created before Start)
    /// </summary>
    private System.Collections.IEnumerator CheckForExistingMesh()
    {
        // Wait a frame to let everything initialize
        yield return null;

        // Wait a bit more for mesh to potentially be created
        yield return new WaitForSeconds(0.5f);

        DestructibleMeshComponent existingMesh = FindFirstObjectByType<DestructibleMeshComponent>();
        if (existingMesh != null)
        {
            Debug.Log("[SimpleRoomOccluder] Found existing room mesh, configuring now...");
            OnRoomMeshCreated(existingMesh);
        }
        else
        {
            Debug.Log("[SimpleRoomOccluder] No existing room mesh found. Will configure when mesh is created.");
        }
    }
    
    /// <summary>
    /// Ensures URP renderer includes VirtualEnvironment layer in its layer masks
    /// </summary>
    [ContextMenu("Fix URP Renderer Layer Masks")]
    public void FixURPRendererLayerMasks()
    {
        int virtualEnvLayer = LayerMask.NameToLayer("VirtualEnvironment");
        if (virtualEnvLayer == -1)
        {
            Debug.LogError("[SimpleRoomOccluder] VirtualEnvironment layer not found! Please create it in Project Settings > Tags and Layers");
            return;
        }
        
        int virtualEnvLayerMask = 1 << virtualEnvLayer;
        
        #if UNITY_EDITOR
        // In Editor: Find all URP Renderer Assets (including inactive ones)
        var rendererAssets = UnityEditor.AssetDatabase.FindAssets("t:UniversalRendererData");
        int fixedCount = 0;
        
        foreach (var guid in rendererAssets)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var rendererData = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRendererData>(path);
            
            if (rendererData == null) continue;
            
            bool needsFix = false;
            
            // Check and fix Opaque Layer Mask
            LayerMask opaqueMask = rendererData.opaqueLayerMask;
            if ((opaqueMask.value & virtualEnvLayerMask) == 0)
            {
                opaqueMask.value |= virtualEnvLayerMask;
                rendererData.opaqueLayerMask = opaqueMask;
                needsFix = true;
                Debug.Log($"[SimpleRoomOccluder] Added VirtualEnvironment to Opaque Layer Mask in {rendererData.name}");
            }
            
            // Check and fix Transparent Layer Mask
            LayerMask transparentMask = rendererData.transparentLayerMask;
            if ((transparentMask.value & virtualEnvLayerMask) == 0)
            {
                transparentMask.value |= virtualEnvLayerMask;
                rendererData.transparentLayerMask = transparentMask;
                needsFix = true;
                Debug.Log($"[SimpleRoomOccluder] Added VirtualEnvironment to Transparent Layer Mask in {rendererData.name}");
            }
            
            if (needsFix)
            {
                fixedCount++;
                UnityEditor.EditorUtility.SetDirty(rendererData);
            }
        }
        
        if (fixedCount > 0)
        {
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"[SimpleRoomOccluder] ✓ Fixed {fixedCount} URP Renderer(s) to include VirtualEnvironment layer. Please check the Renderer assets in Assets/Settings/");
        }
        else
        {
            Debug.Log($"[SimpleRoomOccluder] All URP Renderer(s) already include VirtualEnvironment layer. VirtualEnvironment layer index: {virtualEnvLayer}, mask value: {virtualEnvLayerMask}");
        }
        #else
        // At Runtime: Check active renderer
        var pipeline = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
        if (pipeline != null)
        {
            Debug.Log("[SimpleRoomOccluder] Runtime: URP Renderer layer masks should be configured in Editor. Check that VirtualEnvironment layer is included in your Renderer asset's layer masks.");
        }
        else
        {
            Debug.LogWarning("[SimpleRoomOccluder] URP is not active! Check Graphics Settings.");
        }
        #endif
    }
    
    /// <summary>
    /// Ensures all cameras can see the VirtualEnvironment layer
    /// </summary>
    [ContextMenu("Fix Camera Culling Masks")]
    public void FixCameraCullingMasks()
    {
        int virtualEnvLayer = LayerMask.NameToLayer("VirtualEnvironment");
        if (virtualEnvLayer == -1)
        {
            Debug.LogError("[SimpleRoomOccluder] VirtualEnvironment layer not found! Please create it in Project Settings > Tags and Layers");
            return;
        }
        
        int virtualEnvLayerMask = 1 << virtualEnvLayer; // Bit mask for VirtualEnvironment layer
        
        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        int fixedCount = 0;
        
        Debug.Log($"[SimpleRoomOccluder] Found {allCameras.Length} camera(s). VirtualEnvironment layer index: {virtualEnvLayer}, mask: {virtualEnvLayerMask}");
        
        foreach (Camera cam in allCameras)
        {
            if (cam == null) continue;
            
            int originalMask = cam.cullingMask;
            
            // Check if camera can see VirtualEnvironment layer
            if ((cam.cullingMask & virtualEnvLayerMask) == 0)
            {
                // Add VirtualEnvironment layer to culling mask
                cam.cullingMask |= virtualEnvLayerMask;
                fixedCount++;
                
                Debug.Log($"[SimpleRoomOccluder] ✓ Fixed camera: {cam.name} (was: {originalMask}, now: {cam.cullingMask})");
            }
            else
            {
                if (debugMode)
                    Debug.Log($"[SimpleRoomOccluder] Camera {cam.name} already has VirtualEnvironment layer (mask: {cam.cullingMask})");
            }
        }
        
        if (fixedCount > 0)
        {
            Debug.Log($"[SimpleRoomOccluder] ✓ Fixed {fixedCount} camera(s) to see VirtualEnvironment layer");
        }
        else
        {
            Debug.Log($"[SimpleRoomOccluder] All {allCameras.Length} camera(s) already configured correctly");
        }
    }

    void OnDestroy()
    {
        if (meshSpawner != null)
        {
            meshSpawner.OnDestructibleMeshCreated.RemoveListener(OnRoomMeshCreated);
        }
    }

    private void OnRoomMeshCreated(DestructibleMeshComponent component)
    {
        if (component == null)
        {
            Debug.LogWarning("[SimpleRoomOccluder] OnRoomMeshCreated called with null component!");
            return;
        }

        // Store reference to current mesh component
        currentMeshComponent = component;

        // Clear previous material tracking
        configuredMaterials.Clear();

        LogToDisplay("Room mesh created! Configuring...", Color.cyan);
        Debug.Log($"[SimpleRoomOccluder] OnRoomMeshCreated called! Configuring room mesh for depth occlusion...");

        // Ensure room mesh writes to depth
        List<GameObject> segments = new List<GameObject>();
        component.GetDestructibleMeshSegments(segments);

        LogToDisplay($"Found {segments.Count} mesh segments");
        Debug.Log($"[SimpleRoomOccluder] Found {segments.Count} room mesh segments to configure");

        int configuredCount = 0;
        int failedCount = 0;
        
        foreach (GameObject segment in segments)
        {
            if (segment == null)
            {
                failedCount++;
                continue;
            }
            
            MeshRenderer renderer = segment.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                Debug.LogWarning($"[SimpleRoomOccluder] Segment {segment.name} has no MeshRenderer!");
                failedCount++;
                continue;
            }
            
            if (renderer.material == null)
            {
                Debug.LogWarning($"[SimpleRoomOccluder] Segment {segment.name} has no material!");
                failedCount++;
                continue;
            }
            
            // CRITICAL: Create a new material instance to avoid modifying shared material
            Material mat = renderer.material; // This creates an instance automatically

            // Track this material for verification
            configuredMaterials[renderer] = mat;

            // PERFORMANCE: Cache property checks to avoid repeated lookups
            bool hasZWrite = mat.HasProperty("_ZWrite");
            bool hasSceneMeshZWrite = mat.HasProperty("_SceneMeshZWrite");
            bool needsDepthRenderer = false;

            // PERFORMANCE: Only log in debug mode to avoid string allocations
            if (debugMode)
            {
                Debug.Log($"[SimpleRoomOccluder] Configuring {segment.name}: Shader={mat.shader.name}, HasZWrite={hasZWrite}, CurrentQueue={mat.renderQueue}");
            }

            // Force depth writing - CRITICAL for occlusion
            if (hasZWrite)
            {
                mat.SetFloat("_ZWrite", 1f);
            }
            else
            {
                // If passthrough shader doesn't support depth writing, add a depth-only renderer
                if (addDepthOnlyRenderer && mat.shader.name.Contains("Passthrough"))
                {
                    needsDepthRenderer = true;
                }
                else if (debugMode)
                {
                    Debug.LogWarning($"[SimpleRoomOccluder] Material {mat.name} has no _ZWrite property!");
                }
            }

            // Also check for _SceneMeshZWrite property (used by some passthrough shaders)
            if (hasSceneMeshZWrite)
            {
                mat.SetFloat("_SceneMeshZWrite", 1f);
            }

            // Ensure proper depth testing
            if (mat.HasProperty("_ZTest"))
            {
                mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
            }

            // CRITICAL: Add depth bias to prevent z-fighting and peeking
            if (mat.HasProperty("_Offset") && depthBias > 0)
            {
                mat.SetFloat("_Offset", -depthBias);
            }

            // Set render queue early so it renders BEFORE virtual objects
            if (mat.renderQueue >= 2500)
            {
                mat.renderQueue = 2000; // Force into opaque geometry queue
            }

            // Ensure material is not set to transparent blend mode
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 0f); // Force opaque for depth writing
            }

            // Disable color mask if material is fully transparent (depth-only rendering)
            if (mat.HasProperty("_Color"))
            {
                Color col = mat.GetColor("_Color");
                if (col.a < 0.01f && mat.HasProperty("_ColorMask"))
                {
                    mat.SetInt("_ColorMask", 0); // No color channels
                }
            }

            // Add depth renderer AFTER all material configuration
            if (needsDepthRenderer)
            {
                AddDepthOnlyRenderer(segment);
            }

            configuredCount++;
        }
        
        if (failedCount > 0)
        {
            LogToDisplay($"Config complete: {configuredCount} OK, {failedCount} FAILED", Color.yellow);
        }
        else
        {
            LogToDisplay($"Config complete: {configuredCount} segments OK", Color.green);
        }
        Debug.Log($"[SimpleRoomOccluder] ✓ Room mesh configuration complete: {configuredCount} configured, {failedCount} failed");

        // Mark as configured for verification
        isConfigured = true;
    }
    
    /// <summary>
    /// Adds a depth-only renderer to a segment that uses a passthrough shader
    /// This ensures depth is written even if the passthrough shader doesn't support it
    /// </summary>
    private void AddDepthOnlyRenderer(GameObject segment)
    {
        if (segment == null) return;
        
        // Check if depth renderer already exists
        if (segment.transform.Find($"{segment.name}_DepthOnly") != null)
        {
            if (debugMode)
                Debug.Log($"[SimpleRoomOccluder] Depth-only renderer already exists for {segment.name}");
            return;
        }
        
        // Create depth-only helper object
        GameObject depthHelper = new GameObject($"{segment.name}_DepthOnly");
        depthHelper.transform.SetParent(segment.transform);
        depthHelper.transform.localPosition = Vector3.zero;
        depthHelper.transform.localRotation = Quaternion.identity;
        depthHelper.transform.localScale = Vector3.one;
        depthHelper.layer = segment.layer;
        
        // Copy mesh
        MeshFilter originalFilter = segment.GetComponent<MeshFilter>();
        if (originalFilter != null && originalFilter.sharedMesh != null)
        {
            MeshFilter depthFilter = depthHelper.AddComponent<MeshFilter>();
            depthFilter.sharedMesh = originalFilter.sharedMesh;
        }
        else
        {
            Debug.LogWarning($"[SimpleRoomOccluder] Cannot add depth renderer to {segment.name} - no mesh found");
            Destroy(depthHelper);
            return;
        }
        
        // Add renderer with depth-only material
        MeshRenderer depthRenderer = depthHelper.AddComponent<MeshRenderer>();
        Material depthMat = CreateDepthOnlyMaterial();
        depthRenderer.material = depthMat;
        depthRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        depthRenderer.receiveShadows = false;

        // Track this depth renderer for verification
        configuredMaterials[depthRenderer] = depthMat;

        // Make it persistent but hidden in hierarchy (DontSave means it won't be in scene file)
        // Remove DontSave to make it survive scene reloads if needed
        depthHelper.hideFlags = HideFlags.HideInHierarchy;
        
        LogToDisplay($"✓ Added depth renderer to {segment.name}", Color.green);
        Debug.Log($"[SimpleRoomOccluder] ✓ Added depth-only renderer to {segment.name}");
    }
    
    /// <summary>
    /// Creates a material that only writes to depth buffer (invisible)
    /// </summary>
    private Material CreateDepthOnlyMaterial()
    {
        // Use standard URP Unlit shader (supports depth writing)
        Shader depthShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (depthShader == null)
        {
            depthShader = Shader.Find("Unlit/Color");
        }
        
        if (depthShader == null)
        {
            Debug.LogError("[SimpleRoomOccluder] Cannot find Unlit shader for depth-only material!");
            return null;
        }
        
        // Create material WITHOUT copying shader keywords (avoids keyword space conflicts)
        Material depthMat = new Material(depthShader);
        depthMat.name = "RoomDepthOccluder";
        
        // Clear any shader keywords that might cause conflicts
        depthMat.shaderKeywords = new string[0];
        
        // Make it completely invisible
        if (depthMat.HasProperty("_BaseColor"))
        {
            depthMat.SetColor("_BaseColor", new Color(0, 0, 0, 0)); // Fully transparent
        }
        else if (depthMat.HasProperty("_Color"))
        {
            depthMat.SetColor("_Color", new Color(0, 0, 0, 0));
        }
        
        // CRITICAL: Enable depth writing
        if (depthMat.HasProperty("_ZWrite"))
        {
            depthMat.SetFloat("_ZWrite", 1f);
        }
        if (depthMat.HasProperty("_ZTest"))
        {
            depthMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
        }

        // Apply depth bias using polygon offset to prevent z-fighting
        if (depthBias > 0)
        {
            // Negative factor pushes depth away from camera (renders "behind")
            // This prevents virtual objects from poking through
            depthMat.SetFloat("unity_PolygonOffsetFactor", -1f);
            depthMat.SetFloat("unity_PolygonOffsetUnits", -depthBias * 10000f);
        }

        // Set to render early in geometry queue (before virtual objects)
        depthMat.renderQueue = 1999; // Just before standard opaque geometry

        // Disable color writing (depth only)
        if (depthMat.HasProperty("_ColorMask"))
        {
            depthMat.SetInt("_ColorMask", 0); // No color channels
        }

        return depthMat;
    }
    
    /// <summary>
    /// Manually fix room mesh depth configuration (call this if OnRoomMeshCreated didn't fire)
    /// Works in both Editor and Runtime
    /// </summary>
    [ContextMenu("Fix Room Mesh Depth Now")]
    public void FixRoomMeshDepthNow()
    {
        Debug.Log("[SimpleRoomOccluder] ===== MANUAL FIX REQUESTED =====");
        Debug.Log("[SimpleRoomOccluder] Searching for room mesh...");
        
        // Try to find existing mesh component
        DestructibleMeshComponent meshComponent = FindFirstObjectByType<DestructibleMeshComponent>();
        if (meshComponent == null)
        {
            Debug.LogError("[SimpleRoomOccluder] ✗ No DestructibleMeshComponent found! Make sure room mesh has been created.");
            Debug.LogError("[SimpleRoomOccluder] This might be normal if mesh hasn't been generated yet.");
            return;
        }
        
        Debug.Log("[SimpleRoomOccluder] ✓ Found DestructibleMeshComponent, configuring...");
        OnRoomMeshCreated(meshComponent);
        Debug.Log("[SimpleRoomOccluder] ===== MANUAL FIX COMPLETE =====");
    }
    
    /// <summary>
    /// Editor-time verification - checks configuration without entering Play Mode
    /// </summary>
    #if UNITY_EDITOR
    [ContextMenu("Verify Setup (Editor Only)")]
    public void VerifySetupEditor()
    {
        Debug.Log("[SimpleRoomOccluder] ===== EDITOR VERIFICATION =====");
        
        // Check references
        if (virtualEnvironmentParent == null)
        {
            Debug.LogError("[SimpleRoomOccluder] ✗ Virtual Environment Parent is NULL!");
        }
        else
        {
            Debug.Log($"[SimpleRoomOccluder] ✓ Virtual Environment Parent: {virtualEnvironmentParent.name}");
            
            // Check layer
            int layer = virtualEnvironmentParent.layer;
            string layerName = LayerMask.LayerToName(layer);
            if (layerName == "VirtualEnvironment")
            {
                Debug.Log($"[SimpleRoomOccluder] ✓ Virtual Environment Parent is on correct layer: {layerName}");
            }
            else
            {
                Debug.LogWarning($"[SimpleRoomOccluder] ⚠ Virtual Environment Parent is on layer '{layerName}' (should be 'VirtualEnvironment')");
            }
        }
        
        if (meshSpawner == null)
        {
            Debug.LogError("[SimpleRoomOccluder] ✗ Mesh Spawner is NULL!");
        }
        else
        {
            Debug.Log($"[SimpleRoomOccluder] ✓ Mesh Spawner: {meshSpawner.name}");
        }
        
        // Check VirtualEnvironment layer exists
        int virtualEnvLayer = LayerMask.NameToLayer("VirtualEnvironment");
        if (virtualEnvLayer == -1)
        {
            Debug.LogError("[SimpleRoomOccluder] ✗ VirtualEnvironment layer does not exist! Create it in Project Settings > Tags and Layers");
        }
        else
        {
            Debug.Log($"[SimpleRoomOccluder] ✓ VirtualEnvironment layer exists at index {virtualEnvLayer}");
        }
        
        // Check URP Renderer
        FixURPRendererLayerMasks();
        
        Debug.Log("[SimpleRoomOccluder] ===== EDITOR VERIFICATION COMPLETE =====");
        Debug.Log("[SimpleRoomOccluder] Note: Room mesh configuration can only be verified at runtime (build and run)");
    }
    #endif

    /// <summary>
    /// Configures all virtual objects in the parent hierarchy
    /// </summary>
    [ContextMenu("Configure All Virtual Objects")]
    public void ConfigureAllVirtualObjects()
    {
        if (virtualEnvironmentParent == null)
        {
            Debug.LogError("[SimpleRoomOccluder] Virtual Environment Parent is NULL! Please assign it in the Inspector.");
            return;
        }

        // Check layer first
        int virtualEnvLayer = LayerMask.NameToLayer("VirtualEnvironment");
        int currentLayer = virtualEnvironmentParent.layer;
        
        Debug.Log($"[SimpleRoomOccluder] Virtual Environment Parent: {virtualEnvironmentParent.name}");
        Debug.Log($"[SimpleRoomOccluder] Current layer: {LayerMask.LayerToName(currentLayer)} (index: {currentLayer})");
        Debug.Log($"[SimpleRoomOccluder] VirtualEnvironment layer index: {virtualEnvLayer}");

        Renderer[] allRenderers = virtualEnvironmentParent.GetComponentsInChildren<Renderer>(true);
        Debug.Log($"[SimpleRoomOccluder] Found {allRenderers.Length} renderer(s) in virtual environment");
        
        int configuredCount = 0;
        int skippedCount = 0;

        foreach (Renderer renderer in allRenderers)
        {
            if (renderer == null)
            {
                skippedCount++;
                continue;
            }
            
            if (renderer.material == null)
            {
                Debug.LogWarning($"[SimpleRoomOccluder] Renderer {renderer.name} has no material, skipping");
                skippedCount++;
                continue;
            }
            
            // Check if object is on correct layer
            if (renderer.gameObject.layer != virtualEnvLayer && virtualEnvLayer != -1)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[SimpleRoomOccluder] Object {renderer.name} on wrong layer, fixing...");
                }
                renderer.gameObject.layer = virtualEnvLayer;
            }

            // CRITICAL: Create material instance to avoid modifying shared materials
            Material mat = renderer.material; // This creates an instance automatically

            // Ensure depth testing - virtual objects should be hidden when behind room mesh
            if (mat.HasProperty("_ZTest"))
            {
                mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
            }

            // PERFORMANCE: Determine if material is opaque or transparent
            bool isTransparent = false;
            if (mat.HasProperty("_Surface"))
            {
                isTransparent = mat.GetFloat("_Surface") > 0.5f;
            }
            else if (mat.renderQueue >= 3000)
            {
                isTransparent = true;
            }

            // Virtual objects should write depth if opaque
            if (mat.HasProperty("_ZWrite") && !isTransparent)
            {
                mat.SetFloat("_ZWrite", 1f);
            }

            // Set render queue based on material type
            if (isTransparent)
            {
                if (mat.renderQueue < 3000)
                {
                    mat.renderQueue = 3000;
                }
            }
            else
            {
                if (mat.renderQueue < 2450)
                {
                    mat.renderQueue = 2450;
                }
            }

            configuredCount++;
        }

        Debug.Log($"[SimpleRoomOccluder] ✓ Configured {configuredCount} renderer(s), skipped {skippedCount}");
        
        // Also verify cameras can see the layer
        FixCameraCullingMasks();
    }
    
    /// <summary>
    /// Verifies that room mesh materials are properly configured for depth writing
    /// </summary>
    [ContextMenu("Verify Room Mesh Depth Configuration")]
    public void VerifyRoomMeshDepthConfiguration()
    {
        if (meshSpawner == null)
        {
            Debug.LogWarning("[SimpleRoomOccluder] MeshSpawner not assigned!");
            return;
        }
        
        // Try to find existing mesh component
        DestructibleMeshComponent meshComponent = FindFirstObjectByType<DestructibleMeshComponent>();
        if (meshComponent == null)
        {
            Debug.LogWarning("[SimpleRoomOccluder] No DestructibleMeshComponent found in scene. Mesh may not be created yet.");
            return;
        }
        
        System.Collections.Generic.List<GameObject> segments = new System.Collections.Generic.List<GameObject>();
        meshComponent.GetDestructibleMeshSegments(segments);
        
        int correctCount = 0;
        int incorrectCount = 0;
        
        foreach (GameObject segment in segments)
        {
            if (segment == null) continue;
            
            MeshRenderer renderer = segment.GetComponent<MeshRenderer>();
            if (renderer != null && renderer.material != null)
            {
                Material mat = renderer.material;
                bool isCorrect = true;
                string issues = "";
                
                // Check depth writing
                if (mat.HasProperty("_ZWrite"))
                {
                    float zWrite = mat.GetFloat("_ZWrite");
                    if (zWrite < 0.5f)
                    {
                        isCorrect = false;
                        issues += $" ZWrite={zWrite} (should be 1)";
                    }
                }
                
                // Check render queue
                if (mat.renderQueue >= 2500)
                {
                    isCorrect = false;
                    issues += $" Queue={mat.renderQueue} (should be <2500)";
                }
                
                if (isCorrect)
                {
                    correctCount++;
                    if (debugMode)
                        Debug.Log($"[SimpleRoomOccluder] ✓ {segment.name}: Correct");
                }
                else
                {
                    incorrectCount++;
                    Debug.LogWarning($"[SimpleRoomOccluder] ✗ {segment.name}:{issues}");
                }
            }
        }
        
        Debug.Log($"[SimpleRoomOccluder] Verification complete: {correctCount} correct, {incorrectCount} need fixing");
        
        if (incorrectCount > 0)
        {
            Debug.Log("[SimpleRoomOccluder] Run 'OnRoomMeshCreated' again or use 'Fix Room Mesh Depth' context menu");
        }
    }
}

