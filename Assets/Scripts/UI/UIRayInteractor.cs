using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles UI interaction through raycasting from the right controller
/// Works with OVRInputModule to enable clicking on UI elements
/// Attach this to RightControllerAnchor
/// </summary>
public class UIRayInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("The button used to click/select UI elements")]
    public OVRInput.Button clickButton = OVRInput.Button.PrimaryIndexTrigger;
    [Tooltip("Controller to use for input")]
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;
    
    [Header("Ray Visualization")]
    [Tooltip("Show the interaction ray")]
    public bool showRay = true;
    [Tooltip("Maximum ray distance")]
    public float rayDistance = 10f;
    [Tooltip("Line renderer for visualization")]
    public LineRenderer lineRenderer;
    
    [Header("Ray Appearance")]
    public Color rayColor = new Color(0.2f, 0.8f, 1f, 0.8f);
    public float rayWidth = 0.002f;
    
    [Header("References")]
    [Tooltip("Will be found automatically if not assigned")]
    public OVRInputModule inputModule;
    [Tooltip("Canvas to interact with")]
    public Canvas targetCanvas;
    
    private Camera eventCamera;
    private WristMenuController wristMenuController;
    private bool wasClickingLastFrame = false;
    
    void Start()
    {
        InitializeComponents();
    }
    
    void InitializeComponents()
    {
        // Find input module
        if (inputModule == null)
        {
            inputModule = FindFirstObjectByType<OVRInputModule>();
        }
        
        // Find wrist menu
        if (wristMenuController == null)
        {
            wristMenuController = FindFirstObjectByType<WristMenuController>();
        }
        
        // Find or create line renderer
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }
        
        SetupLineRenderer();
        
        // Find event camera
        OVRCameraRig cameraRig = FindFirstObjectByType<OVRCameraRig>();
        if (cameraRig != null && cameraRig.centerEyeAnchor != null)
        {
            eventCamera = cameraRig.centerEyeAnchor.GetComponent<Camera>();
        }
        
        Debug.Log("[UIRayInteractor] Initialized successfully");
    }
    
    void SetupLineRenderer()
    {
        if (lineRenderer == null) return;
        
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        
        // Create material if needed
        if (lineRenderer.material == null || lineRenderer.material.shader.name == "Standard")
        {
            Material lineMat = new Material(Shader.Find("Unlit/Color"));
            lineMat.color = rayColor;
            lineRenderer.material = lineMat;
        }
        
        lineRenderer.startColor = rayColor;
        lineRenderer.endColor = rayColor;
        lineRenderer.enabled = false;
    }
    
    void Update()
    {
        // Only active when menu is visible
        bool menuVisible = wristMenuController != null && wristMenuController.IsMenuVisible();
        
        if (!menuVisible)
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
            return;
        }
        
        UpdateRayVisualization();
        HandleClickInput();
    }
    
    void UpdateRayVisualization()
    {
        if (!showRay || lineRenderer == null) return;
        
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;
        Vector3 rayEnd = rayOrigin + rayDirection * rayDistance;
        
        // Check for UI hits using raycast
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
        {
            // Check if we hit a UI canvas
            Canvas canvas = hit.collider.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                rayEnd = hit.point;
            }
        }
        
        // Update line renderer
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, rayOrigin);
        lineRenderer.SetPosition(1, rayEnd);
    }
    
    void HandleClickInput()
    {
        bool isClicking = OVRInput.Get(clickButton, controller);
        bool clickDown = OVRInput.GetDown(clickButton, controller);
        bool clickUp = OVRInput.GetUp(clickButton, controller);
        
        if (clickDown)
        {
            Debug.Log("[UIRayInteractor] Click DOWN detected!");
            SimulateClick();
        }
        
        if (clickUp)
        {
            Debug.Log("[UIRayInteractor] Click UP detected!");
        }
        
        wasClickingLastFrame = isClicking;
    }
    
    void SimulateClick()
    {
        // Direct raycast approach for world-space UI
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;
        
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
        {
            Debug.Log($"[UIRayInteractor] Ray hit: {hit.collider.gameObject.name} at distance {hit.distance}");
            
            // Check if we hit a UI element
            UnityEngine.UI.Button button = hit.collider.GetComponent<UnityEngine.UI.Button>();
            if (button != null && button.interactable)
            {
                Debug.Log($"[UIRayInteractor] ✓ Clicking button: {button.gameObject.name}");
                button.onClick.Invoke();
                return;
            }
            
            // Try to find button in parent
            button = hit.collider.GetComponentInParent<UnityEngine.UI.Button>();
            if (button != null && button.interactable)
            {
                Debug.Log($"[UIRayInteractor] ✓ Clicking button (from parent): {button.gameObject.name}");
                button.onClick.Invoke();
                return;
            }
            
            // Try slider
            UnityEngine.UI.Slider slider = hit.collider.GetComponent<UnityEngine.UI.Slider>();
            if (slider == null)
            {
                slider = hit.collider.GetComponentInParent<UnityEngine.UI.Slider>();
            }
            
            if (slider != null)
            {
                Debug.Log($"[UIRayInteractor] Hit slider: {slider.gameObject.name}");
                // Slider interaction would need drag handling
            }
            
            Debug.Log($"[UIRayInteractor] Hit object but no interactive UI component found");
        }
        else
        {
            Debug.Log("[UIRayInteractor] Ray didn't hit anything");
        }
    }
    
    void OnDisable()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}

