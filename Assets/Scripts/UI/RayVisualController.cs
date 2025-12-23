using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Draws a visible ray from the controller for UI interaction.
/// Uses GraphicRaycaster to properly detect UI elements.
/// Attach this to the UIRayInteractor GameObject.
/// </summary>
public class RayVisualController : MonoBehaviour
{
    [Header("Ray Settings")]
    [Tooltip("The transform to cast the ray from (e.g., RightControllerAnchor)")]
    public Transform rayOrigin;
    
    [Tooltip("Maximum length of the ray")]
    public float rayLength = 5f;
    
    [Header("Canvas Reference")]
    [Tooltip("The WristMenuCanvas to interact with")]
    public Canvas targetCanvas;
    
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
    
    [Header("Input")]
    [Tooltip("Which controller to use for input")]
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;
    
    [Tooltip("Button to click/select")]
    public OVRInput.Button selectButton = OVRInput.Button.PrimaryIndexTrigger;
    
    // Raycasting
    private GraphicRaycaster graphicRaycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;
    private Camera uiCamera;
    
    // Current UI target
    private GameObject currentHitObject;
    private Button currentButton;
    private Slider currentSlider;
    private Vector3 currentHitPoint;
    private bool isHittingUI = false;
    
    private void Start()
    {
        // Auto-find ray origin if not set
        if (rayOrigin == null)
        {
            OVRCameraRig cameraRig = FindFirstObjectByType<OVRCameraRig>();
            if (cameraRig != null)
            {
                Transform rightHand = cameraRig.rightHandAnchor;
                if (rightHand != null)
                {
                    Transform controllerAnchor = rightHand.Find("RightControllerAnchor");
                    rayOrigin = controllerAnchor != null ? controllerAnchor : rightHand;
                }
            }
            
            if (rayOrigin == null)
            {
                Debug.LogError("[RayVisual] Could not find ray origin! Please assign manually.");
                enabled = false;
                return;
            }
        }
        
        // Find canvas and raycaster
        if (targetCanvas == null)
        {
            // Try to find WristMenuCanvas
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                if (c.name.Contains("WristMenu") || c.name.Contains("Wrist"))
                {
                    targetCanvas = c;
                    break;
                }
            }
        }
        
        if (targetCanvas != null)
        {
            graphicRaycaster = targetCanvas.GetComponent<GraphicRaycaster>();
            uiCamera = targetCanvas.worldCamera;
            
            if (graphicRaycaster == null)
            {
                Debug.LogWarning("[RayVisual] No GraphicRaycaster on canvas, adding one...");
                graphicRaycaster = targetCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }
        
        // Get event system
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            eventSystem = FindFirstObjectByType<EventSystem>();
        }
        
        // Create pointer event data
        if (eventSystem != null)
        {
            pointerEventData = new PointerEventData(eventSystem);
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
        
        Debug.Log($"[RayVisual] Initialized. Canvas: {targetCanvas?.name}, Raycaster: {graphicRaycaster != null}");
    }
    
    private void ConfigureLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.startColor = defaultColor;
        lineRenderer.endColor = defaultColor;
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingOrder = 5000;
        
        // Use unlit shader to always be visible
        Shader unlitShader = Shader.Find("Unlit/Color");
        if (unlitShader != null)
        {
            lineRenderer.material = new Material(unlitShader);
        }
        else
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        lineRenderer.material.color = defaultColor;
        lineRenderer.material.renderQueue = 5000;
    }
    
    private void Update()
    {
        if (rayOrigin == null || lineRenderer == null) return;
        
        Vector3 startPos = rayOrigin.position;
        Vector3 direction = rayOrigin.forward;
        Vector3 endPos = startPos + direction * rayLength;
        
        // Reset state
        isHittingUI = false;
        currentButton = null;
        currentSlider = null;
        currentHitObject = null;
        
        // Method 1: Direct plane intersection with canvas
        if (targetCanvas != null)
        {
            RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
            Plane canvasPlane = new Plane(canvasRect.forward, canvasRect.position);
            
            Ray ray = new Ray(startPos, direction);
            float enter;
            
            if (canvasPlane.Raycast(ray, out enter) && enter <= rayLength && enter > 0)
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                
                // Check if hit point is within canvas bounds
                Vector3 localPoint = canvasRect.InverseTransformPoint(hitPoint);
                Rect rect = canvasRect.rect;
                
                if (rect.Contains(new Vector2(localPoint.x, localPoint.y)))
                {
                    endPos = hitPoint;
                    currentHitPoint = hitPoint;
                    isHittingUI = true;
                    
                    // Find UI element at this position using world position raycast
                    FindUIElementAtPoint(hitPoint);
                }
            }
        }
        
        // Update visual
        SetRayColor(isHittingUI ? hitColor : defaultColor);
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
        
        // Handle input
        if (isHittingUI)
        {
            HandleInput();
        }
    }
    
    private void FindUIElementAtPoint(Vector3 worldPoint)
    {
        if (targetCanvas == null) return;
        
        // Get all graphics in canvas
        Graphic[] graphics = targetCanvas.GetComponentsInChildren<Graphic>();
        
        float closestDist = float.MaxValue;
        Graphic closestGraphic = null;
        
        foreach (Graphic graphic in graphics)
        {
            if (!graphic.raycastTarget) continue;
            
            RectTransform rectTransform = graphic.rectTransform;
            
            // Check if point is within this graphic's bounds
            Vector3 localPoint = rectTransform.InverseTransformPoint(worldPoint);
            Rect rect = rectTransform.rect;
            
            if (rect.Contains(new Vector2(localPoint.x, localPoint.y)))
            {
                // Calculate distance (use z depth for sorting)
                float dist = Vector3.Distance(rayOrigin.position, rectTransform.position);
                
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestGraphic = graphic;
                }
            }
        }
        
        if (closestGraphic != null)
        {
            currentHitObject = closestGraphic.gameObject;
            currentButton = closestGraphic.GetComponent<Button>();
            if (currentButton == null)
            {
                currentButton = closestGraphic.GetComponentInParent<Button>();
            }
            currentSlider = closestGraphic.GetComponent<Slider>();
            if (currentSlider == null)
            {
                currentSlider = closestGraphic.GetComponentInParent<Slider>();
            }
        }
    }
    
    private void HandleInput()
    {
        bool triggerDown = OVRInput.GetDown(selectButton, controller);
        bool triggerHeld = OVRInput.Get(selectButton, controller);
        
        // Handle button click
        if (triggerDown && currentButton != null && currentButton.interactable)
        {
            Debug.Log($"[RayVisual] Clicking button: {currentButton.name}");
            
            // Visual feedback
            currentButton.Select();
            
            // Invoke click
            currentButton.onClick.Invoke();
        }
        
        // Handle slider - works while pointing at slider area (even without holding trigger)
        // Hold trigger to "grab" and drag, or just point to adjust
        if (currentSlider != null)
        {
            // Update slider when trigger is held OR when just hovering (optional - remove triggerHeld check for hover-adjust)
            if (triggerHeld)
            {
                UpdateSliderValue(currentSlider);
            }
        }
    }
    
    private void UpdateSliderValue(Slider slider)
    {
        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        if (sliderRect == null) return;
        
        // Try to find the background or fill area for more accurate positioning
        RectTransform targetRect = null;
        
        // Look for common slider child names
        string[] possibleNames = { "Background", "Fill Area", "Handle Slide Area", "Slider" };
        foreach (string name in possibleNames)
        {
            Transform child = sliderRect.Find(name);
            if (child != null)
            {
                targetRect = child as RectTransform;
                break;
            }
        }
        
        // Fallback to slider rect itself
        if (targetRect == null)
        {
            targetRect = sliderRect;
        }
        
        // Convert hit point to local space of the slider
        Vector3 localPoint = targetRect.InverseTransformPoint(currentHitPoint);
        Rect rect = targetRect.rect;
        
        // Calculate normalized position (0-1) based on slider direction
        float normalizedValue;
        
        if (slider.direction == Slider.Direction.LeftToRight || slider.direction == Slider.Direction.RightToLeft)
        {
            // Horizontal slider
            normalizedValue = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
            
            if (slider.direction == Slider.Direction.RightToLeft)
            {
                normalizedValue = 1f - normalizedValue;
            }
        }
        else
        {
            // Vertical slider
            normalizedValue = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);
            
            if (slider.direction == Slider.Direction.TopToBottom)
            {
                normalizedValue = 1f - normalizedValue;
            }
        }
        
        // Clamp and apply
        normalizedValue = Mathf.Clamp01(normalizedValue);
        
        // Smoothly interpolate to new value for less jittery movement
        float targetValue = Mathf.Lerp(slider.minValue, slider.maxValue, normalizedValue);
        slider.value = Mathf.Lerp(slider.value, targetValue, Time.deltaTime * 15f);
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
            lineRenderer.enabled = true;
    }
    
    private void OnDisable()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }
}

