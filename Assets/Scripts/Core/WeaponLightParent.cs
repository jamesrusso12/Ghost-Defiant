using UnityEngine;

/// <summary>
/// Simple script to parent a weapon light to the VR camera.
/// Attach this to your Directional Light GameObject.
/// </summary>
public class WeaponLightParent : MonoBehaviour
{
    void Start()
    {
        // Try to find OVRCameraRig first (Meta XR SDK / Quest)
        OVRCameraRig ovrRig = FindFirstObjectByType<OVRCameraRig>();
        Transform cameraTransform = null;
        
        if (ovrRig != null && ovrRig.centerEyeAnchor != null)
        {
            // Use the VR camera from OVRCameraRig
            cameraTransform = ovrRig.centerEyeAnchor;
        }
        else if (Camera.main != null)
        {
            // Fallback to Camera.main if OVRCameraRig not found
            cameraTransform = Camera.main.transform;
        }
        
        if (cameraTransform != null)
        {
            // Parent this object (the light) to the camera
            transform.SetParent(cameraTransform);
            
            // Reset position to be exactly at the camera
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("[WeaponLightParent] Could not find camera! Light will not be parented.");
        }
    }
}
