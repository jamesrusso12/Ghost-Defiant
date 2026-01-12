using UnityEngine;

/// <summary>
/// Attach this to a Directional Light to automatically parent it to the VR camera.
/// Useful for weapon lighting that should always follow the player's view.
/// </summary>
public class WeaponLightSetup : MonoBehaviour
{
    [Tooltip("If true, the light will be parented to the camera on Start")]
    public bool parentToCameraOnStart = true;
    
    [Tooltip("If true, reset local position/rotation after parenting")]
    public bool resetTransformAfterParent = true;

    void Start()
    {
        if (parentToCameraOnStart)
        {
            ParentToCamera();
        }
    }

    public void ParentToCamera()
    {
        Transform cameraTransform = GetCameraTransform();
        
        if (cameraTransform != null)
        {
            transform.SetParent(cameraTransform, worldPositionStays: false);
            
            if (resetTransformAfterParent)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            
            Debug.Log($"[WeaponLightSetup] Successfully parented {gameObject.name} to camera: {cameraTransform.name}");
        }
        else
        {
            Debug.LogWarning("[WeaponLightSetup] Could not find camera! Light will not be parented.");
        }
    }

    Transform GetCameraTransform()
    {
        // Try OVRCameraRig first (Meta XR SDK)
        OVRCameraRig ovrRig = FindFirstObjectByType<OVRCameraRig>();
        if (ovrRig != null && ovrRig.centerEyeAnchor != null)
        {
            return ovrRig.centerEyeAnchor;
        }
        
        // Fallback to Camera.main
        if (Camera.main != null)
        {
            return Camera.main.transform;
        }
        
        // Last resort: find any camera
        Camera cam = FindFirstObjectByType<Camera>();
        return cam != null ? cam.transform : null;
    }
}
