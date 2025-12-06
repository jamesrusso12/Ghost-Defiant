using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Configures UI elements to work properly with Mixed Reality.
/// Ensures Canvas components are set up correctly for MR rendering.
/// </summary>
public class MRUIConfigurator : MonoBehaviour
{
    [Header("UI Configuration")]
    [Tooltip("If true, automatically configure all Canvas components in the scene for MR")]
    public bool autoConfigureOnStart = true;

    [Tooltip("Preferred render mode for MR UI (World Space works best for MR)")]
    public RenderMode preferredRenderMode = RenderMode.WorldSpace;

    [Tooltip("Scale for World Space Canvas (adjust based on your scene scale). Use 0.01 for normal VR scale.")]
    public float worldSpaceScale = 0.01f;

    [Tooltip("Distance from camera for World Space UI")]
    public float uiDistanceFromCamera = 0.8f;

    [Tooltip("Continuously enforce correct layer configuration (fixes Unity auto-sync issues)")]
    public bool continuouslyEnforceLayers = true;

    [Header("HUD Configuration")]
    [Tooltip("If true, will add GameUILazyFollow component to make UI follow camera smoothly")]
    public bool useLazyFollowForHUD = true;

    [Tooltip("If useLazyFollowForHUD is false, UI will be attached directly to camera")]
    public bool attachUIToCamera = false;

    [Tooltip("If both are false, UI will update position every frame to follow camera")]
    public bool updatePositionEveryFrame = false;

    private void Start()
    {
        if (autoConfigureOnStart)
        {
            ConfigureAllCanvases();
        }
    }

    private float layerCheckInterval = 0.5f; // Check layers every 0.5 seconds instead of every frame
    private float lastLayerCheckTime = 0f;

    private void Update()
    {
        // Continuously enforce correct layer configuration to prevent Unity auto-sync issues
        // But only check periodically to avoid flickering/glitching
        if (continuouslyEnforceLayers && Time.time - lastLayerCheckTime >= layerCheckInterval)
        {
            EnforceCorrectLayers();
            lastLayerCheckTime = Time.time;
        }

        // Update UI position every frame if needed (for HUD behavior)
        // Only if NOT using LazyFollow (LazyFollow handles its own updates)
        if (updatePositionEveryFrame && !attachUIToCamera && !useLazyFollowForHUD)
        {
            UpdateUIPosition();
        }
    }

    /// <summary>
    /// Configures all Canvas components in the scene for Mixed Reality
    /// </summary>
    public void ConfigureAllCanvases()
    {
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        foreach (Canvas canvas in allCanvases)
        {
            ConfigureCanvasForMR(canvas);
        }

        // Debug.Log($"[MRUIConfigurator] Configured {allCanvases.Length} Canvas components for Mixed Reality");
    }

    /// <summary>
    /// Configures a single Canvas component for Mixed Reality
    /// </summary>
    public void ConfigureCanvasForMR(Canvas canvas)
    {
        if (canvas == null) return;

        // Check for OVROverlayCanvas using reflection to avoid compilation errors if not available
        System.Type ovrCanvasType = System.Type.GetType("OVROverlayCanvas");

        // Configure Canvas for World Space rendering (works well in MR)
        if (canvas.renderMode != preferredRenderMode)
        {
            canvas.renderMode = preferredRenderMode;

            if (preferredRenderMode == RenderMode.WorldSpace)
            {
                // Set up World Space Canvas
                if (canvas.worldCamera == null)
                {
                    // Try to find main camera or XR camera
                    Camera mainCam = Camera.main;
                    if (mainCam == null)
                    {
                        mainCam = FindFirstObjectByType<Camera>();
                    }
                    canvas.worldCamera = mainCam;
                }

                // Set appropriate scale for VR/MR
                // Use the configured scale (typically 0.01 for normal VR scale)
                // Only fix invalid/NaN scales, don't force a minimum
                float currentScale = canvas.transform.localScale.x;
                if (float.IsNaN(currentScale) || currentScale == 0)
                {
                    canvas.transform.localScale = Vector3.one * worldSpaceScale;
                }
                else if (Mathf.Approximately(currentScale, 0.01f) || currentScale > 0.001f)
                {
                    // Keep existing valid scale (0.01 is fine)
                    // Only update if user wants a different scale
                    if (!Mathf.Approximately(currentScale, worldSpaceScale))
                    {
                        canvas.transform.localScale = Vector3.one * worldSpaceScale;
                    }
                }

                // Configure UI to follow camera (HUD behavior)
                ConfigureHUDBehavior(canvas);

                // Add Canvas Scaler if not present
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler == null)
                {
                    scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                }

                // Ensure proper sorting
                canvas.sortingOrder = 100; // High sorting order to appear on top
                
                // CRITICAL: Set render queue to render ABOVE passthrough walls
                // UI should render after everything else (including passthrough geometry)
                // This prevents UI from being occluded by real-world walls
                CanvasRenderer[] allRenderers = canvas.GetComponentsInChildren<CanvasRenderer>(true);
                foreach (CanvasRenderer renderer in allRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.cullTransparentMesh = false; // Ensure UI is always visible
                        
                        // Set high render queue for UI materials to render above passthrough
                        Material mat = renderer.GetMaterial();
                        if (mat != null)
                        {
                            // Passthrough geometry renders at ~2000-3000, UI should render after
                            // Render queue 4000+ ensures UI renders on top
                            if (mat.renderQueue < 4000)
                            {
                                mat.renderQueue = 4000;
                            }
                        }
                    }
                }
            }
            else if (preferredRenderMode == RenderMode.ScreenSpaceOverlay)
            {
                // For Screen Space Overlay, try to add OVROverlayCanvas component
                // This provides better MR integration
                // Reuse the ovrCanvasType variable from the outer scope
                if (ovrCanvasType != null)
                {
                    Component existingOvrCanvas = canvas.GetComponent(ovrCanvasType);
                    if (existingOvrCanvas == null)
                    {
                        canvas.gameObject.AddComponent(ovrCanvasType);
                        // Debug.Log($"[MRUIConfigurator] Added OVROverlayCanvas to '{canvas.name}'");
                    }
                }
            }
        }

        // Configure layers for OVROverlayCanvas
        // CRITICAL: GameObject layer must be DIFFERENT from Overlay Layer
        // - GameObject layer: "UI" (for camera visibility control)
        // - Canvas Layer: "Overlay UI" (what OVROverlayCanvas renders to)
        // - Overlay Layer: "Overlay UI" (what OVROverlayCanvas uses for overlay)
        // NOTE: Unity auto-syncs GameObject layer when Canvas Layer changes, so we must
        // set GameObject layer AFTER Canvas Layer, and may need to do it repeatedly
        int overlayUILayer = LayerMask.NameToLayer("Overlay UI");
        int uiLayer = LayerMask.NameToLayer("UI");

        // Also update OVROverlayCanvas Canvas Layer if present (reuse ovrCanvasType from above)
        if (ovrCanvasType != null)
        {
            Component ovrCanvas = canvas.GetComponent(ovrCanvasType);
            if (ovrCanvas != null)
            {
                // CRITICAL: Set GameObject layer to "UI" FIRST (before Canvas Layer)
                // This prevents Unity from auto-syncing it to "Overlay UI" when we set Canvas Layer
                if (uiLayer != -1)
                {
                    SetLayerRecursively(canvas.gameObject, uiLayer);
                }

                // Set Canvas Layer to Overlay UI - this is what OVROverlayCanvas wants
                // Unity may auto-sync GameObject layer, so we'll fix it again after
                var canvasLayerField = ovrCanvasType.GetField("canvasLayer",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (canvasLayerField != null)
                {
                    canvasLayerField.SetValue(ovrCanvas, overlayUILayer);
                    // Debug.Log($"[MRUIConfigurator] Set OVROverlayCanvas Canvas Layer to Overlay UI (Layer {overlayUILayer})");
                }

                // Set Overlay Layer to Overlay UI (must match Canvas Layer)
                var overlayLayerField = ovrCanvasType.GetField("overlayLayer",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (overlayLayerField != null)
                {
                    overlayLayerField.SetValue(ovrCanvas, overlayUILayer);
                    // Debug.Log($"[MRUIConfigurator] Set OVROverlayCanvas Overlay Layer to Overlay UI (Layer {overlayUILayer})");
                }

                // CRITICAL: Force GameObject layer back to "UI" AFTER setting Canvas Layer
                // Unity auto-syncs GameObject layer when Canvas Layer changes, so we must override it
                // OVROverlayCanvas requires GameObject layer to be DIFFERENT from Overlay Layer
                if (uiLayer != -1)
                {
                    // Use a coroutine to set it after a frame to ensure it sticks
                    StartCoroutine(ForceGameObjectLayerAfterFrame(canvas.gameObject, uiLayer));
                    // Also set it immediately
                    SetLayerRecursively(canvas.gameObject, uiLayer);
                    // Debug.Log($"[MRUIConfigurator] Set GameObject to UI layer (Layer {uiLayer}) - must be different from Overlay Layer");
                }

                // Fix Canvas transform to avoid singular matrix errors with OVROverlayCanvas
                FixCanvasTransformForOVROverlay(canvas);

                // Ensure Canvas is enabled and visible (don't disable it)
                if (!canvas.gameObject.activeSelf)
                {
                    canvas.gameObject.SetActive(true);
                }
                if (!canvas.enabled)
                {
                    canvas.enabled = true;
                }
                
                // CRITICAL: Disable culling on all Canvas Renderers (on child UI elements) to prevent UI from disappearing
                // "Cull Transparent Mesh" causes UI to disappear when not directly facing camera
                // Canvas Renderers are on child elements like TextMeshPro, not on the Canvas itself
                CanvasRenderer[] allRenderers = canvas.GetComponentsInChildren<CanvasRenderer>(true);
                foreach (CanvasRenderer renderer in allRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.cullTransparentMesh = false;
                    }
                }

                // Ensure camera can see Overlay UI layer (check all cameras)
                EnsureCameraSeesOverlayUI(canvas.worldCamera);
            }
            else
            {
                // If no OVROverlayCanvas, set GameObject to UI layer
                if (uiLayer != -1 && canvas.gameObject.layer != uiLayer)
                {
                    SetLayerRecursively(canvas.gameObject, uiLayer);
                }
                canvas.gameObject.SetActive(true);
                canvas.enabled = true;
                EnsureCameraSeesOverlayUI(canvas.worldCamera);
            }
        }
        else
        {
            // If no OVROverlayCanvas, set GameObject to UI layer
            if (uiLayer != -1 && canvas.gameObject.layer != uiLayer)
            {
                SetLayerRecursively(canvas.gameObject, uiLayer);
            }
            canvas.gameObject.SetActive(true);
            canvas.enabled = true;
            EnsureCameraSeesOverlayUI(canvas.worldCamera);
        }

        // Debug.Log($"[MRUIConfigurator] Configured Canvas '{canvas.name}' for Mixed Reality (RenderMode: {canvas.renderMode})");
    }

    /// <summary>
    /// Sets the layer recursively for a GameObject and all its children
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int layer, bool skipRoot = false)
    {
        if (!skipRoot)
        {
            obj.layer = layer;
        }
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    /// <summary>
    /// Fixes Canvas transform to avoid OVROverlayCanvas matrix determinant errors
    /// </summary>
    private void FixCanvasTransformForOVROverlay(Canvas canvas)
    {
        if (canvas == null) return;

        Transform canvasTransform = canvas.transform;

        // Fix invalid scales (NaN, zero, or infinity) but don't force a minimum
        // Scale of 0.01 is perfectly valid for VR UI
        float currentScale = canvasTransform.localScale.x;
        if (float.IsNaN(currentScale) || currentScale == 0 || float.IsInfinity(currentScale))
        {
            canvasTransform.localScale = Vector3.one * worldSpaceScale;
            // Debug.Log($"[MRUIConfigurator] Fixed invalid Canvas scale to {worldSpaceScale}");
        }

        // Ensure uniform scale (all axes same)
        if (Mathf.Abs(canvasTransform.localScale.x - canvasTransform.localScale.y) > 0.001f ||
            Mathf.Abs(canvasTransform.localScale.x - canvasTransform.localScale.z) > 0.001f)
        {
            float uniformScale = canvasTransform.localScale.x;
            canvasTransform.localScale = Vector3.one * uniformScale;
        }

        // Ensure position is valid (not NaN or Infinity)
        if (float.IsNaN(canvasTransform.position.x) || float.IsInfinity(canvasTransform.position.x) ||
            float.IsNaN(canvasTransform.position.y) || float.IsInfinity(canvasTransform.position.y) ||
            float.IsNaN(canvasTransform.position.z) || float.IsInfinity(canvasTransform.position.z))
        {
            canvasTransform.position = new Vector3(0, 0, 2f); // Default position in front
            // Debug.LogWarning($"[MRUIConfigurator] Fixed invalid Canvas position for '{canvas.name}'");
        }

        // Ensure rotation is valid
        if (float.IsNaN(canvasTransform.rotation.x) || float.IsNaN(canvasTransform.rotation.y) ||
            float.IsNaN(canvasTransform.rotation.z) || float.IsNaN(canvasTransform.rotation.w))
        {
            canvasTransform.rotation = Quaternion.identity;
            // Debug.LogWarning($"[MRUIConfigurator] Fixed invalid Canvas rotation for '{canvas.name}'");
        }
    }

    /// <summary>
    /// Ensures the camera can see the Overlay UI layer (fixes visibility issues)
    /// </summary>
    private void EnsureCameraSeesOverlayUI(Camera cam)
    {
        int overlayUILayer = LayerMask.NameToLayer("Overlay UI");
        if (overlayUILayer == -1)
        {
            // Debug.LogWarning("[MRUIConfigurator] Overlay UI layer not found!");
            return;
        }

        // Calculate layer bit once
        int overlayUILayerBit = 1 << overlayUILayer;

        // If specific camera provided, update it
        if (cam != null)
        {
            if ((cam.cullingMask & overlayUILayerBit) == 0)
            {
                cam.cullingMask |= overlayUILayerBit;
                // Debug.Log($"[MRUIConfigurator] Added Overlay UI layer to camera '{cam.name}' culling mask");
            }
        }

        // Also update ALL cameras in the scene to ensure visibility
        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera camera in allCameras)
        {
            if ((camera.cullingMask & overlayUILayerBit) == 0)
            {
                camera.cullingMask |= overlayUILayerBit;
                // Debug.Log($"[MRUIConfigurator] Added Overlay UI layer to camera '{camera.name}' culling mask");
            }
        }
    }

    /// <summary>
    /// Continuously enforces correct layer configuration to prevent Unity auto-sync issues
    /// Checks all Canvases with OVROverlayCanvas and ensures correct layer setup
    /// </summary>
    private void EnforceCorrectLayers()
    {
        System.Type ovrCanvasType = System.Type.GetType("OVROverlayCanvas");
        if (ovrCanvasType == null) return;

        int overlayUILayer = LayerMask.NameToLayer("Overlay UI");
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer == -1 || overlayUILayer == -1) return;

        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas == null) continue;

            Component ovrCanvas = canvas.GetComponent(ovrCanvasType);
            if (ovrCanvas != null)
            {
                // Fix Canvas Layer if it's wrong (should be "Overlay UI", not "UI")
                var canvasLayerField = ovrCanvasType.GetField("canvasLayer",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (canvasLayerField != null)
                {
                    int currentCanvasLayer = (int)canvasLayerField.GetValue(ovrCanvas);
                    if (currentCanvasLayer != overlayUILayer)
                    {
                        canvasLayerField.SetValue(ovrCanvas, overlayUILayer);
                    }
                }

                // Check if GameObject layer matches Overlay Layer (this causes the error)
                var overlayLayerField = ovrCanvasType.GetField("overlayLayer",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (overlayLayerField != null)
                {
                    int overlayLayerValue = (int)overlayLayerField.GetValue(ovrCanvas);
                    // If GameObject layer matches Overlay Layer, fix it immediately
                    if (canvas.gameObject.layer == overlayLayerValue && overlayLayerValue == overlayUILayer)
                    {
                        SetLayerRecursively(canvas.gameObject, uiLayer);
                    }
                }

                // Ensure GameObject layer is "UI" (not "Overlay UI")
                // Only change if actually wrong to avoid unnecessary updates
                if (canvas.gameObject.layer == overlayUILayer && uiLayer != -1)
                {
                    SetLayerRecursively(canvas.gameObject, uiLayer);
                }
            }
        }

        // Also ensure cameras can see Overlay UI layer
        EnsureCameraSeesOverlayUI(null);
    }

    /// <summary>
    /// Forces GameObject layer to UI after a frame delay to override Unity's auto-sync behavior
    /// Unity auto-syncs GameObject layer when Canvas Layer changes in OVROverlayCanvas
    /// </summary>
    private IEnumerator ForceGameObjectLayerAfterFrame(GameObject obj, int layer)
    {
        yield return null; // Wait one frame
        if (obj != null)
        {
            SetLayerRecursively(obj, layer);
        }
    }

    /// <summary>
    /// Configures UI to follow camera like a HUD
    /// </summary>
    private void ConfigureHUDBehavior(Canvas canvas)
    {
        if (canvas == null) return;

        Camera targetCamera = canvas.worldCamera;
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindFirstObjectByType<Camera>();
            }
        }

        if (targetCamera == null) return;

        if (useLazyFollowForHUD)
        {
            // Use GameUILazyFollow component for smooth following (like Start Menu)
            GameUILazyFollow lazyFollow = canvas.GetComponent<GameUILazyFollow>();
            if (lazyFollow == null)
            {
                lazyFollow = canvas.gameObject.AddComponent<GameUILazyFollow>();
                // Configure the lazy follow component
                lazyFollow.followActive = true;
                lazyFollow.m_SnapOnEnable = true;
                lazyFollow.m_TargetOffset = Vector3.forward * uiDistanceFromCamera;

                // Find the camera rig's center eye anchor
                var ovrRig = FindFirstObjectByType<OVRCameraRig>();
                if (ovrRig != null && ovrRig.centerEyeAnchor != null)
                {
                    lazyFollow.SetTarget(ovrRig.centerEyeAnchor);
                }
            }
            else
            {
                // Component already exists - just ensure it's configured correctly
                if (!lazyFollow.followActive)
                {
                    lazyFollow.followActive = true;
                }
                // Update target if it's null
                if (lazyFollow.m_Target == null)
                {
                    var ovrRig = FindFirstObjectByType<OVRCameraRig>();
                    if (ovrRig != null && ovrRig.centerEyeAnchor != null)
                    {
                        lazyFollow.SetTarget(ovrRig.centerEyeAnchor);
                    }
                }
            }
        }
        else
        {
            // If LazyFollow is disabled but component exists, remove it
            GameUILazyFollow existingLazyFollow = canvas.GetComponent<GameUILazyFollow>();
            if (existingLazyFollow != null)
            {
                Destroy(existingLazyFollow);
            }
        }

        // Handle attach to camera option (only if not using LazyFollow)
        if (!useLazyFollowForHUD && attachUIToCamera)
        {
            // Attach UI to camera as child (best for HUD - follows automatically)
            if (canvas.transform.parent != targetCamera.transform)
            {
                // Store original world position/rotation before parenting
                Vector3 worldPos = canvas.transform.position;
                Quaternion worldRot = canvas.transform.rotation;

                // Parent to camera
                canvas.transform.SetParent(targetCamera.transform, false);

                // Set local position in front of camera
                canvas.transform.localPosition = Vector3.forward * uiDistanceFromCamera;
                canvas.transform.localRotation = Quaternion.identity;

                // If UI was already positioned, try to maintain relative position
                if (worldPos != Vector3.zero)
                {
                    // Calculate relative position
                    Vector3 relativePos = targetCamera.transform.InverseTransformPoint(worldPos);
                    if (relativePos.magnitude > 0.1f)
                    {
                        canvas.transform.localPosition = relativePos;
                    }
                }
            }
            else
            {
                // Already parented, just ensure position is correct
                if (canvas.transform.localPosition.magnitude < 0.1f)
                {
                    canvas.transform.localPosition = Vector3.forward * uiDistanceFromCamera;
                }
            }
        }
        else
        {
            // Don't attach, but position in front of camera initially
            if (canvas.transform.position == Vector3.zero || canvas.transform.parent == null)
            {
                canvas.transform.position = targetCamera.transform.position + targetCamera.transform.forward * uiDistanceFromCamera;
                canvas.transform.LookAt(targetCamera.transform);
                canvas.transform.Rotate(0, 180, 0); // Face the camera
            }
        }
    }

    /// <summary>
    /// Updates UI position every frame to follow camera (if not attached)
    /// </summary>
    private void UpdateUIPosition()
    {
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas == null || canvas.renderMode != RenderMode.WorldSpace) continue;
            if (canvas.transform.parent != null) continue; // Skip if already parented

            Camera targetCamera = canvas.worldCamera;
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null) continue;
            }

            // Update position and rotation to follow camera
            canvas.transform.position = targetCamera.transform.position + targetCamera.transform.forward * uiDistanceFromCamera;
            canvas.transform.rotation = targetCamera.transform.rotation;
        }
    }

    /// <summary>
    /// Manually configure a specific Canvas by name
    /// </summary>
    public void ConfigureCanvasByName(string canvasName)
    {
        Canvas canvas = GameObject.Find(canvasName)?.GetComponent<Canvas>();
        if (canvas != null)
        {
            ConfigureCanvasForMR(canvas);
        }
        else
        {
            // Debug.LogWarning($"[MRUIConfigurator] Canvas '{canvasName}' not found");
        }
    }
}

