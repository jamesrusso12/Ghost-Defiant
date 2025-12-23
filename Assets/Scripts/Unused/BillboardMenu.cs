using UnityEngine;
using System.Collections;

/// <summary>
/// Billboard script that makes UI always face the camera.
/// Optimized for Meta XR SDK - works with OVRCameraRig.
/// </summary>
public class BillboardMenu : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Target camera, usually the main VR camera. Leave empty for auto-detect.")]
    public Transform targetCamera;

    [Header("Positioning")]
    [Tooltip("Distance in front of the camera.")]
    public float distance = 2.5f;

    [Tooltip("Optional offset from the default forward position (X=left/right, Y=up/down, Z=forward/back).")]
    public Vector3 offset = new Vector3(0f, -0.3f, 0f);

    [Header("Movement")]
    [Tooltip("Should the menu smoothly follow the camera?")]
    public bool smoothFollow = true;

    [Tooltip("Speed of the smooth movement (higher = faster).")]
    public float smoothSpeed = 5f;

    [Header("Rotation")]
    [Tooltip("Keep menu upright (only rotate on Y-axis). Recommended for VR.")]
    public bool keepUpright = true;

    [Header("Initialization")]
    [Tooltip("Delay before starting billboard behavior (gives VR time to initialize).")]
    public float startDelay = 0.5f;

    private bool isInitialized = false;

    void Start()
    {
        StartCoroutine(InitializeCamera());
    }

    private IEnumerator InitializeCamera()
    {
        // Wait for start delay (VR initialization)
        yield return new WaitForSeconds(startDelay);

        // If no target is assigned, auto-detect camera
        if (targetCamera == null)
        {
            // Try to find OVRCameraRig first (Meta XR SDK)
            OVRCameraRig ovrRig = FindFirstObjectByType<OVRCameraRig>();
            if (ovrRig != null && ovrRig.centerEyeAnchor != null)
            {
                targetCamera = ovrRig.centerEyeAnchor;
                Debug.Log("[BillboardMenu] Found OVRCameraRig - using center eye anchor.");
            }
            // Fallback to main camera
            else if (Camera.main != null)
            {
                targetCamera = Camera.main.transform;
                Debug.Log("[BillboardMenu] Using Camera.main as fallback.");
            }
        }

        // Wait until we have a valid camera
        while (targetCamera == null)
        {
            Debug.LogWarning("[BillboardMenu] Waiting for camera...");
            yield return new WaitForSeconds(0.1f);
            
            // Retry detection
            if (Camera.main != null)
            {
                targetCamera = Camera.main.transform;
            }
        }

        // Position menu initially in front of camera
        PositionMenuImmediately();
        
        isInitialized = true;
        Debug.Log("[BillboardMenu] Initialized successfully.");
    }

    private void PositionMenuImmediately()
    {
        if (targetCamera == null) return;

        // Calculate position
        Vector3 desiredPosition = targetCamera.position + (targetCamera.forward * distance) + offset;
        transform.position = desiredPosition;

        // Calculate rotation (direction FROM camera TO menu, not TO camera)
        Vector3 lookDirection = transform.position - targetCamera.position;
        if (keepUpright)
        {
            lookDirection.y = 0; // Keep upright
        }

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    void LateUpdate()
    {
        if (!isInitialized || targetCamera == null)
            return;

        // Calculate the desired position in front of the camera
        Vector3 desiredPosition = targetCamera.position + (targetCamera.forward * distance) + offset;

        // Smoothly interpolate to the desired position if needed
        if (smoothFollow)
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        else
            transform.position = desiredPosition;

        // Billboard effect: rotate the menu so it always faces the camera
        // Direction FROM menu TO camera would make UI backwards
        // So we use direction FROM camera TO menu (inverted)
        Vector3 lookDirection = transform.position - targetCamera.position;
        
        // Keep menu upright by zeroing out the Y component
        if (keepUpright)
        {
            lookDirection.y = 0;
        }

        // Only rotate if we have a valid direction
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            
            if (smoothFollow)
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
            else
                transform.rotation = targetRotation;
        }
    }

    /// <summary>
    /// Call this to reset the menu position (useful if camera changes)
    /// </summary>
    public void ResetPosition()
    {
        if (targetCamera != null)
        {
            PositionMenuImmediately();
        }
    }

    private void OnValidate()
    {
        // Ensure reasonable values in inspector
        distance = Mathf.Max(0.5f, distance);
        smoothSpeed = Mathf.Max(0.1f, smoothSpeed);
        startDelay = Mathf.Max(0f, startDelay);
    }
}
