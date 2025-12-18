using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Helps with menu interaction by managing the pointer ray from the right controller
/// Attach this to the RightControllerAnchor
/// </summary>
public class MenuPointerHelper : MonoBehaviour
{
    [Header("Ray Visualization")]
    [Tooltip("Show a visual ray when pointing at UI")]
    public bool showRay = true;
    [Tooltip("LineRenderer for the ray (will be created if not assigned)")]
    public LineRenderer lineRenderer;
    [Tooltip("Ray length")]
    public float rayLength = 10f;
    [Tooltip("Ray color")]
    public Color rayColor = new Color(0f, 0.8f, 1f, 0.8f);
    [Tooltip("Ray width")]
    public float rayWidth = 0.005f;
    
    [Header("Input")]
    [Tooltip("Button to click on UI (Index Trigger)")]
    public OVRInput.Button clickButton = OVRInput.Button.PrimaryIndexTrigger;
    [Tooltip("Controller to use")]
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;
    
    [Header("References")]
    [Tooltip("OVRRaycaster for UI interaction (will be found if not assigned)")]
    public OVRRaycaster ovrRaycaster;
    
    private bool isInitialized = false;
    
    void Start()
    {
        InitializeComponents();
    }
    
    void InitializeComponents()
    {
        // Find or create LineRenderer
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }
        
        // Configure LineRenderer
        ConfigureLineRenderer();
        
        // Find OVRRaycaster if not assigned
        if (ovrRaycaster == null)
        {
            ovrRaycaster = FindFirstObjectByType<OVRRaycaster>();
        }
        
        if (ovrRaycaster == null)
        {
            Debug.LogWarning("[MenuPointerHelper] OVRRaycaster not found. UI interaction may not work properly.");
        }
        
        isInitialized = true;
        Debug.Log("[MenuPointerHelper] Initialized successfully");
    }
    
    void ConfigureLineRenderer()
    {
        if (lineRenderer == null) return;
        
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startColor = rayColor;
        lineRenderer.endColor = rayColor;
        
        // Set material if needed
        if (lineRenderer.material == null)
        {
            // Use a simple unlit material
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.material.color = rayColor;
        }
        
        // Start with ray hidden
        lineRenderer.enabled = false;
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        UpdateRay();
        HandleInput();
    }
    
    void UpdateRay()
    {
        if (!showRay || lineRenderer == null) return;
        
        Vector3 rayStart = transform.position;
        Vector3 rayEnd = transform.position + transform.forward * rayLength;
        bool hitUI = false;
        
        // Check for UI hits
        if (ovrRaycaster != null && EventSystem.current != null)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = new Vector2(Screen.width / 2, Screen.height / 2);
            
            var results = new System.Collections.Generic.List<RaycastResult>();
            ovrRaycaster.Raycast(eventData, results);
            
            if (results.Count > 0)
            {
                rayEnd = results[0].worldPosition;
                hitUI = true;
            }
        }
        
        // Check for physical objects
        RaycastHit hit;
        if (Physics.Raycast(rayStart, transform.forward, out hit, rayLength))
        {
            if (!hitUI || hit.distance < Vector3.Distance(rayStart, rayEnd))
            {
                rayEnd = hit.point;
            }
        }
        
        // Only show ray when menu is visible
        WristMenuController wristMenu = FindFirstObjectByType<WristMenuController>();
        bool menuVisible = wristMenu != null && wristMenu.IsMenuVisible();
        
        // Always show ray when menu is open, even if not hitting anything
        lineRenderer.enabled = menuVisible;
        
        if (lineRenderer.enabled)
        {
            lineRenderer.SetPosition(0, rayStart);
            lineRenderer.SetPosition(1, rayEnd);
        }
    }
    
    void HandleInput()
    {
        if (OVRInput.GetDown(clickButton, controller))
        {
            // The OVRInputModule should handle the click automatically
            Debug.Log("[MenuPointerHelper] Click detected on right controller");
        }
    }
}

