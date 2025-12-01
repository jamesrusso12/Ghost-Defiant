using System.Collections;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Makes the GameUI Canvas follow the camera with smooth movement.
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
    [Tooltip("Offset from the target position (in front of camera).")]
    public Vector3 m_TargetOffset = Vector3.forward * 2f;

    [SerializeField]
    [Tooltip("Snap to target on enable.")]
    public bool m_SnapOnEnable = true;

    [Tooltip("Whether following is active.")]
    public bool followActive = true;

    [Tooltip("How fast the UI moves (Higher = Faster).")]
    public float positionLerpSpeed = 5.0f; // Increased default for responsiveness

    [Tooltip("How fast the UI rotates.")]
    public float rotationLerpSpeed = 5.0f;

    [Tooltip("Smooth time for summoning.")]
    public float smoothTime = 0.3f;

    private Vector3 m_TargetLastPos;
    private Vector3 velocity = Vector3.zero;
    private Camera m_Camera;

    // improved target position calculation
    Vector3 targetPosition => m_Target != null ? m_Target.TransformPoint(m_TargetOffset) : transform.position;

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