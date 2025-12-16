using System.Collections;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Makes the GameUI Canvas follow the camera with smooth movement.
/// Includes wall avoidance to prevent UI from clipping through walls.
/// </summary>
public class GameUILazyFollow : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The object being followed (usually CenterEyeAnchor). If null, will auto-find.")]
    public Transform m_Target;

    [SerializeField]
    [Tooltip("Whether to only follow when out of view.")]
    bool m_FOV = false;

    [SerializeField]
    [Tooltip("Whether rotation is locked to z-axis (keeps UI upright).")]
    bool m_ZRot = true;

    [SerializeField]
    [Tooltip("Offset from the target position (in front of camera). REDUCED to 0.25m to prevent clipping.")]
    public Vector3 m_TargetOffset = Vector3.forward * 0.25f;

    [SerializeField]
    [Tooltip("Snap to target on enable.")]
    public bool m_SnapOnEnable = true;

    [Header("Collision Settings")]
    [SerializeField]
    [Tooltip("Layers that obstruct the view (walls, scene mesh, etc). Include Default, SceneMesh, and any wall layers.")]
    public LayerMask collisionMask = ~0; // All layers by default - adjust in inspector to exclude UI/Player

    [SerializeField]
    [Tooltip("How far to keep the UI from the wall.")]
    public float wallOffset = 0.1f;

    [Tooltip("Whether following is active.")]
    public bool followActive = true;

    [Tooltip("How fast the UI moves (Higher = Faster).")]
    public float positionLerpSpeed = 8.0f; // Increased for faster response to avoid wall clipping

    [Tooltip("How fast the UI rotates.")]
    public float rotationLerpSpeed = 8.0f;

    [Tooltip("Smooth time for summoning.")]
    public float smoothTime = 0.2f;

    private Vector3 m_TargetLastPos;
    private Vector3 velocity = Vector3.zero;
    private Camera m_Camera;

    // improved target position calculation
    Vector3 targetPosition
    {
        get
        {
            if (m_Target == null) return transform.position;

            Vector3 desiredPos = m_Target.TransformPoint(m_TargetOffset);

            // Check for walls between camera and desired position
            Vector3 dir = desiredPos - m_Target.position;
            float dist = dir.magnitude;

            if (Physics.Raycast(m_Target.position, dir.normalized, out RaycastHit hit, dist, collisionMask))
            {
                // Hit a wall! Place UI slightly in front of the hit point
                return hit.point - (dir.normalized * wallOffset);
            }

            return desiredPos;
        }
    }

    Quaternion targetRotation
    {
        get
        {
            if (m_Target == null) return transform.rotation;

            // If ZRot is true, we want the UI to stay upright (gravity aligned)
            // If ZRot is false, it locks exactly to the head (like a pilot HUD)
            if (m_ZRot)
            {
                Vector3 lookPos = transform.position - m_Target.position;
                // Zero out Y difference to keep it upright
                lookPos.y = 0;
                if (lookPos != Vector3.zero)
                    return Quaternion.LookRotation(lookPos);
            }

            return m_Target.rotation;
        }
    }

    void Awake()
    {
        // Auto-find the camera rig if target not set
        if (m_Target == null)
        {
            // Try to find OVR Camera Rig first
            var ovrRig = FindFirstObjectByType<OVRCameraRig>();
            if (ovrRig != null)
                m_Target = ovrRig.centerEyeAnchor;
            else
            {
                // Fallback to Main Camera
                if (Camera.main != null) m_Target = Camera.main.transform;
            }
        }

        m_Camera = Camera.main;
        if (m_Camera == null)
        {
            m_Camera = FindFirstObjectByType<Camera>();
        }
        
        // Ensure canvas is configured to render on top of walls (only if not manually configured)
        // You can disable this if it interferes with your manual positioning
        if (GetComponent<Canvas>() != null)
        {
            ConfigureCanvasRenderOrder();
        }
    }
    
    /// <summary>
    /// Configures the canvas to render on top of walls and scene geometry
    /// </summary>
    private void ConfigureCanvasRenderOrder()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) return;
        
        // Set high sorting order to render on top
        canvas.sortingOrder = 100;
        canvas.overrideSorting = true;
        
        // Ensure canvas is in World Space (required for proper depth sorting in VR)
        if (canvas.renderMode != RenderMode.WorldSpace)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }
        
        // Disable culling on all canvas renderers to ensure visibility
        CanvasRenderer[] renderers = GetComponentsInChildren<CanvasRenderer>(true);
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.cullTransparentMesh = false;
            }
        }
        
        // Set all UI elements to UI layer
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer != -1)
        {
            SetLayerRecursively(gameObject, uiLayer);
        }
    }
    
    /// <summary>
    /// Recursively sets layer for GameObject and all children
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    void Start()
    {
        if (m_Target != null)
        {
            m_TargetLastPos = targetPosition;
        }
    }

    void OnEnable()
    {
        // FIX: Removed 'hasSnappedOnce' so the menu always appears 
        // in front of you every time you enable it.
        if (m_SnapOnEnable && m_Target != null)
        {
            transform.position = targetPosition;
            // Align rotation immediately so it doesn't spin in
            Vector3 lookDirection = transform.position - m_Target.position;
            if (lookDirection != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookDirection);

            velocity = Vector3.zero;
        }
    }

    void Update()
    {
        if (!followActive || m_Target == null)
            return;

        // --- 1. Position Update ---
        // Only update position if FOV check is disabled OR if it is outside the FOV.
        bool shouldUpdatePosition = true;

        if (m_FOV && m_Camera != null)
        {
            Vector3 screenPoint = m_Camera.WorldToViewportPoint(transform.position);

            // Define "in view" slightly tighter than 0 to 1 to account for lag/smoothness
            bool inFov = screenPoint.z > 0f &&
                         screenPoint.x > 0.1f && screenPoint.x < 0.9f &&
                         screenPoint.y > 0.1f && screenPoint.y < 0.9f;

            // If it is in the FOV, we DO NOT update its position.
            if (inFov)
            {
                shouldUpdatePosition = false;
            }
        }

        if (shouldUpdatePosition)
        {
            // Smoothly follow the target's position
            // This runs only when the UI is outside the FOV (or m_FOV is false)
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpSpeed);
        }

        // --- 2. Rotation Update (Always run for smooth head tracking) ---
        if (m_Target != null)
        {
            Quaternion targetRot = targetRotation;

            // Use Slerp to smooth the rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationLerpSpeed);
        }

        m_TargetLastPos = targetPosition;
    }

    /// <summary>
    /// Manually summon the UI to the target position
    /// </summary>
    public void Summon()
    {
        if (!followActive)
            StartCoroutine(OneTimeSummonPosition());
        else
        {
            // If following is active, just snap immediately or let Update handle it
            transform.position = targetPosition;
        }
    }

    IEnumerator OneTimeSummonPosition()
    {
        while (m_Target != null && Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            yield return null;
        }
    }

    public void SetTarget(Transform target)
    {
        m_Target = target;
        if (m_Target != null)
        {
            m_TargetLastPos = targetPosition;
        }
    }
}
