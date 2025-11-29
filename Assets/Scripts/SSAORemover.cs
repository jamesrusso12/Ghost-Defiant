using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SSAORemover : MonoBehaviour
{
    [Header("SSAO Removal Settings")]
    public bool autoRemoveOnStart = true;
    public bool enableDebugLogs = true;
    
    void Start()
    {
        if (autoRemoveOnStart)
        {
            RemoveSSAOFromRenderers();
        }
    }
    
    [ContextMenu("Remove SSAO from All Renderers")]
    public void RemoveSSAOFromRenderers()
    {
        if (enableDebugLogs)
            Debug.Log("[SSAORemover] Starting SSAO removal...");
            
        // Find all URP Renderer assets
        var rendererAssets = Resources.FindObjectsOfTypeAll<UniversalRendererData>();
        
        foreach (var renderer in rendererAssets)
        {
            if (renderer != null)
            {
                RemoveSSAOFromRenderer(renderer);
            }
        }
        
        if (enableDebugLogs)
            Debug.Log($"[SSAORemover] Processed {rendererAssets.Length} renderer assets");
    }
    
    private void RemoveSSAOFromRenderer(UniversalRendererData renderer)
    {
        if (renderer == null) return;
        
        // Get the renderer features
        var features = renderer.rendererFeatures;
        
        for (int i = features.Count - 1; i >= 0; i--)
        {
            var feature = features[i];
            if (feature != null)
            {
                // Check if it's SSAO
                if (feature.GetType().Name.Contains("SSAO") || 
                    feature.GetType().Name.Contains("ScreenSpaceAmbientOcclusion"))
                {
                    if (enableDebugLogs)
                        Debug.Log($"[SSAORemover] Removing SSAO feature from {renderer.name}");
                    
                    // Remove the feature
                    features.RemoveAt(i);
                    
                    // Mark asset as dirty
                    #if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(renderer);
                    #endif
                }
            }
        }
    }
    
    [ContextMenu("Check for SSAO References")]
    public void CheckForSSAOReferences()
    {
        if (enableDebugLogs)
            Debug.Log("[SSAORemover] Checking for SSAO references...");
            
        var rendererAssets = Resources.FindObjectsOfTypeAll<UniversalRendererData>();
        int ssaoCount = 0;
        
        foreach (var renderer in rendererAssets)
        {
            if (renderer != null)
            {
                var features = renderer.rendererFeatures;
                foreach (var feature in features)
                {
                    if (feature != null && 
                        (feature.GetType().Name.Contains("SSAO") || 
                         feature.GetType().Name.Contains("ScreenSpaceAmbientOcclusion")))
                    {
                        ssaoCount++;
                        if (enableDebugLogs)
                            Debug.LogWarning($"[SSAORemover] Found SSAO in {renderer.name}");
                    }
                }
            }
        }
        
        if (ssaoCount == 0)
        {
            Debug.Log("[SSAORemover] No SSAO references found - good!");
        }
        else
        {
            Debug.LogWarning($"[SSAORemover] Found {ssaoCount} SSAO references that need removal");
        }
    }
}
