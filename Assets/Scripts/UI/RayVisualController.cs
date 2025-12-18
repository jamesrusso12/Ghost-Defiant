using UnityEngine;

/// <summary>
/// Draws a visible ray from the controller for UI interaction.
/// Attach this to the UIRayInteractor GameObject.
/// </summary>
public class RayVisualController : MonoBehaviour
{
    [Header("Ray Settings")]
    [Tooltip("The transform to cast the ray from (e.g., RightControllerAnchor)")]
    public Transform rayOrigin;
    
    [Tooltip("Maximum length of the ray")]
    public float rayLength = 5f;
    
    [Tooltip("Layers the ray can hit")]
    public LayerMask raycastLayers = -1; // Everything by default
    
    [Header("Visual Settings")]
    [Tooltip("Line Renderer to draw the ray (will create one if not assigned)")]
    public LineRenderer lineRenderer;
    
    [Tooltip("Color of the ray when not hitting anything")]
    public Color defaultColor = Color.cyan;
    
    [Tooltip("Color of the ray when hitting UI")]
    public Color hitColor = Color.green;
    
    [Tooltip("Width of the ray at the start")]
    public float startWidth = 0.005f;
    
    [Tooltip("Width of the ray at the end")]
    public float endWidth = 0.002f;
    
    private void Start()
    {
        // Auto-find ray origin if not set
        if (rayOrigin == null)
        {
            // Try to find RightControllerAnchor
            OVRCameraRig cameraRig = FindFirstObjectByType<OVRCameraRig>();
            if (cameraRig != null)
            {
                // Look for RightControllerAnchor in the hierarchy
                Transform rightHand = cameraRig.rightHandAnchor;
                if (rightHand != null)
                {
                    Transform controllerAnchor = rightHand.Find("RightControllerAnchor");
                    if (controllerAnchor != null)
                    {
                        rayOrigin = controllerAnchor;
                    }
                    else
                    {
                        rayOrigin = rightHand;
                    }
                }
            }
            
            if (rayOrigin == null)
            {
                Debug.LogError("[RayVisual] Could not find ray origin! Please assign manually.");
                enabled = false;
                return;
            }
        }
        
        // Create or configure Line Renderer
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }
        
        ConfigureLineRenderer();
    }
    
    private void ConfigureLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.startColor = defaultColor;
        lineRenderer.endColor = defaultColor;
        lineRenderer.useWorldSpace = true;
        
        // Use a simple material if none assigned
        if (lineRenderer.material == null || lineRenderer.material.name.Contains("Default"))
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        lineRenderer.material.color = defaultColor;
    }
    
    private void Update()
    {
        if (rayOrigin == null || lineRenderer == null) return;
        
        // Get ray start position and direction
        Vector3 startPos = rayOrigin.position;
        Vector3 direction = rayOrigin.forward;
        
        // Perform raycast
        RaycastHit hit;
        Vector3 endPos;
        bool didHit = Physics.Raycast(startPos, direction, out hit, rayLength, raycastLayers);
        
        if (didHit)
        {
            endPos = hit.point;
            SetRayColor(hitColor);
        }
        else
        {
            endPos = startPos + direction * rayLength;
            SetRayColor(defaultColor);
        }
        
        // Update line renderer positions
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }
    
    private void SetRayColor(Color color)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            if (lineRenderer.material != null)
            {
                lineRenderer.material.color = color;
            }
        }
    }
    
    private void OnEnable()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
    }
    
    private void OnDisable()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}

