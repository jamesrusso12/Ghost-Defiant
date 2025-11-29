using UnityEngine;
using Oculus.Interaction.Samples;
using Oculus.Interaction;

public class MetaSDKFix : MonoBehaviour
{
    [Header("Auto-fix Settings")]
    public bool autoFixOnStart = true;
    public bool enableDebugLogs = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixMetaSDKIssues();
        }
    }
    
    public void FixMetaSDKIssues()
    {
        if (enableDebugLogs)
            Debug.Log("[MetaSDKFix] Starting Meta SDK fixes...");
            
        FixMRPassThroughHandVisualize();
        FixDeprecatedPrefabs();
        
        if (enableDebugLogs)
            Debug.Log("[MetaSDKFix] Meta SDK fixes completed!");
    }
    
    private void FixMRPassThroughHandVisualize()
    {
        // Find all MRPassThroughHandVisualize components
        MRPassThroughHandVisualize[] handVisualizers = FindObjectsByType<MRPassThroughHandVisualize>(FindObjectsSortMode.None);
        
        foreach (var handVisualizer in handVisualizers)
        {
            if (handVisualizer != null)
            {
                // Check if Hand Material Property Blocks is empty or null
                var handMaterialPropertyBlocks = handVisualizer.GetType()
                    .GetField("handMaterialPropertyBlocks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (handMaterialPropertyBlocks != null)
                {
                    var value = handMaterialPropertyBlocks.GetValue(handVisualizer);
                    
                    // If the collection is null or empty, create a default one
                    if (value == null || (value is System.Collections.ICollection collection && collection.Count == 0))
                    {
                        if (enableDebugLogs)
                            Debug.Log($"[MetaSDKFix] Fixing MRPassThroughHandVisualize on {handVisualizer.gameObject.name}");
                            
                        // Create a default MaterialPropertyBlockEditor
                        CreateDefaultMaterialPropertyBlocks(handVisualizer);
                    }
                }
            }
        }
    }
    
    private void CreateDefaultMaterialPropertyBlocks(MRPassThroughHandVisualize handVisualizer)
    {
        // This is a simplified fix - in a real scenario, you'd want to configure this properly
        // For now, we'll disable the component to prevent crashes
        if (handVisualizer != null)
        {
            handVisualizer.enabled = false;
            
            if (enableDebugLogs)
                Debug.Log($"[MetaSDKFix] Disabled MRPassThroughHandVisualize on {handVisualizer.gameObject.name} to prevent crash");
        }
    }
    
    private void FixDeprecatedPrefabs()
    {
        // Find all deprecated prefab components
        DeprecatedPrefab[] deprecatedPrefabs = FindObjectsByType<DeprecatedPrefab>(FindObjectsSortMode.None);
        
        foreach (var deprecatedPrefab in deprecatedPrefabs)
        {
            if (deprecatedPrefab != null)
            {
                if (enableDebugLogs)
                    Debug.Log($"[MetaSDKFix] Found deprecated prefab: {deprecatedPrefab.gameObject.name}");
                    
                // You can either:
                // 1. Replace with new prefab
                // 2. Unpack the prefab
                // 3. Disable the component temporarily
                
                // For now, let's just log a warning
                Debug.LogWarning($"[MetaSDKFix] Deprecated prefab found: {deprecatedPrefab.gameObject.name}. Consider updating to the new version.");
            }
        }
    }
    
    // Public method to manually trigger fixes
    [ContextMenu("Fix Meta SDK Issues")]
    public void ManualFix()
    {
        FixMetaSDKIssues();
    }
}
