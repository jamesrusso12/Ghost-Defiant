using UnityEngine;
using Oculus.Interaction.Samples;

public class EmergencyMetaSDKFix : MonoBehaviour
{
    void Awake()
    {
        // This runs before Start() to prevent crashes
        FixMRPassThroughHandVisualize();
    }
    
    private void FixMRPassThroughHandVisualize()
    {
        // Find all MRPassThroughHandVisualize components and disable them
        MRPassThroughHandVisualize[] handVisualizers = FindObjectsByType<MRPassThroughHandVisualize>(FindObjectsSortMode.None);
        
        foreach (var handVisualizer in handVisualizers)
        {
            if (handVisualizer != null)
            {
                handVisualizer.enabled = false;
                Debug.Log($"[EmergencyMetaSDKFix] Disabled MRPassThroughHandVisualize on {handVisualizer.gameObject.name} to prevent crash");
            }
        }
        
        Debug.Log($"[EmergencyMetaSDKFix] Fixed {handVisualizers.Length} MRPassThroughHandVisualize components");
    }
}
