using UnityEngine;
using Oculus.Interaction.Samples;
using System.Collections.Generic;

public class MetaSDKMaterialFixer : MonoBehaviour
{
    [Header("Fix Settings")]
    public bool autoFixOnStart = true;
    public bool enableDebugLogs = true;
    
    [Header("Default Materials")]
    public Material defaultHandMaterial;
    public Material defaultControllerMaterial;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixMissingMaterials();
        }
    }
    
    [ContextMenu("Fix Missing Materials")]
    public void FixMissingMaterials()
    {
        if (enableDebugLogs)
            Debug.Log("[MetaSDKMaterialFixer] Starting material fix...");
            
        FixMRPassThroughHandVisualizeMaterials();
        FixControllerMaterials();
        FixOtherMetaComponents();
        
        if (enableDebugLogs)
            Debug.Log("[MetaSDKMaterialFixer] Material fix completed!");
    }
    
    private void FixMRPassThroughHandVisualizeMaterials()
    {
        MRPassThroughHandVisualize[] handVisualizers = FindObjectsByType<MRPassThroughHandVisualize>(FindObjectsSortMode.None);
        
        foreach (var handVisualizer in handVisualizers)
        {
            if (handVisualizer != null)
            {
                if (enableDebugLogs)
                    Debug.Log($"[MetaSDKMaterialFixer] Fixing MRPassThroughHandVisualize on {handVisualizer.gameObject.name}");
                
                // Try to fix the material property blocks
                FixHandMaterialPropertyBlocks(handVisualizer);
            }
        }
    }
    
    private void FixHandMaterialPropertyBlocks(MRPassThroughHandVisualize handVisualizer)
    {
        // This is a complex fix that requires reflection to access private fields
        // For now, we'll disable the component to prevent crashes
        if (handVisualizer != null)
        {
            handVisualizer.enabled = false;
            
            if (enableDebugLogs)
                Debug.Log($"[MetaSDKMaterialFixer] Disabled MRPassThroughHandVisualize on {handVisualizer.gameObject.name} - requires manual material configuration");
        }
    }
    
    private void FixControllerMaterials()
    {
        // Find controller visual components
        var controllerVisuals = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        
        foreach (var controller in controllerVisuals)
        {
            if (controller.name.Contains("OVRControllerVisual") || controller.name.Contains("Controller"))
            {
                // Check for missing materials on renderers
                Renderer[] renderers = controller.GetComponentsInChildren<Renderer>();
                
                foreach (var renderer in renderers)
                {
                    if (renderer.material == null || renderer.material.name.Contains("Default"))
                    {
                        if (defaultControllerMaterial != null)
                        {
                            renderer.material = defaultControllerMaterial;
                            
                            if (enableDebugLogs)
                                Debug.Log($"[MetaSDKMaterialFixer] Applied default controller material to {renderer.gameObject.name}");
                        }
                    }
                }
            }
        }
    }
    
    private void FixOtherMetaComponents()
    {
        // Find other Meta SDK components that might need materials
        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (var obj in allObjects)
        {
            // Check for Meta SDK components
            if (obj.name.Contains("Meta") || obj.name.Contains("OVR") || obj.name.Contains("Hand"))
            {
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                
                foreach (var renderer in renderers)
                {
                    if (renderer.material == null || renderer.material.name.Contains("Default"))
                    {
                        // Try to find a suitable material
                        Material suitableMaterial = FindSuitableMaterial(obj.name);
                        
                        if (suitableMaterial != null)
                        {
                            renderer.material = suitableMaterial;
                            
                            if (enableDebugLogs)
                                Debug.Log($"[MetaSDKMaterialFixer] Applied material to {renderer.gameObject.name}");
                        }
                    }
                }
            }
        }
    }
    
    private Material FindSuitableMaterial(string objectName)
    {
        // Try to find appropriate materials based on object name
        if (objectName.Contains("Hand"))
        {
            return defaultHandMaterial;
        }
        else if (objectName.Contains("Controller"))
        {
            return defaultControllerMaterial;
        }
        
        // Try to find materials in the project
        Material[] allMaterials = Resources.FindObjectsOfTypeAll<Material>();
        
        foreach (var material in allMaterials)
        {
            if (material.name.Contains("Hand") && objectName.Contains("Hand"))
            {
                return material;
            }
            else if (material.name.Contains("Controller") && objectName.Contains("Controller"))
            {
                return material;
            }
        }
        
        return null;
    }
    
    // Public method to manually assign materials
    [ContextMenu("Find All Missing Materials")]
    public void FindAllMissingMaterials()
    {
        var allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int missingCount = 0;
        
        foreach (var renderer in allRenderers)
        {
            if (renderer.material == null || renderer.material.name.Contains("Default"))
            {
                missingCount++;
                Debug.LogWarning($"[MetaSDKMaterialFixer] Missing material on: {renderer.gameObject.name}");
            }
        }
        
        Debug.Log($"[MetaSDKMaterialFixer] Found {missingCount} objects with missing materials");
    }
}
