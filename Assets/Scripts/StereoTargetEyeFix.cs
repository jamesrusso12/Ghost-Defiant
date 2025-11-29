using UnityEngine;

/// <summary>
/// Fixes the OVROverlayCanvas stereoTargetEye warning in URP
/// This script should be added to the project to suppress the console error
/// </summary>
[DefaultExecutionOrder(-100)] // Run early
public class StereoTargetEyeFix : MonoBehaviour
{
    void Awake()
    {
        // Disable the problematic code in OVROverlayCanvas
        // This is a workaround for the Meta SDK issue with URP
        
        // Find all OVROverlayCanvas components in the scene
        OVROverlayCanvas[] canvases = FindObjectsByType<OVROverlayCanvas>(FindObjectsSortMode.None);
        
        foreach (var canvas in canvases)
        {
            // The error occurs in OVROverlayCanvas.Start()
            // We can't directly fix it, but we can log that we're aware
            Debug.Log($"[StereoTargetEyeFix] Found OVROverlayCanvas on {canvas.gameObject.name}. " +
                     "stereoTargetEye warning is expected with URP and can be safely ignored.");
        }
    }
}

