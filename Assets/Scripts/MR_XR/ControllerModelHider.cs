using UnityEngine;

/// <summary>
/// Hides Meta Quest controller models during gameplay.
/// Shows them when menu is open for UI interaction.
/// </summary>
public class ControllerModelHider : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Hide controllers when game starts")]
    public bool hideOnStart = true;
    
    [Tooltip("Delay before hiding (seconds)")]
    public float hideDelay = 0.5f;
    
    [Header("Debug")]
    [Tooltip("Enable console logging")]
    public bool enableDebug = false;
    
    private bool controllersCurrentlyHidden = false;
    
    void Start()
    {
        if (hideOnStart)
        {
            Invoke(nameof(HideControllers), hideDelay);
        }
    }
    
    /// <summary>
    /// Hide controller models (for gameplay)
    /// </summary>
    public void HideControllers()
    {
        if (controllersCurrentlyHidden) return;
        
        SetControllerVisibility(false);
        controllersCurrentlyHidden = true;
        
        if (enableDebug) Debug.Log("[ControllerModelHider] Controllers HIDDEN");
    }
    
    /// <summary>
    /// Show controller models (for menu interaction)
    /// </summary>
    public void ShowControllers()
    {
        if (!controllersCurrentlyHidden) return;
        
        SetControllerVisibility(true);
        controllersCurrentlyHidden = false;
        
        if (enableDebug) Debug.Log("[ControllerModelHider] Controllers SHOWN");
    }
    
    private void SetControllerVisibility(bool visible)
    {
        // Find controller anchors
        GameObject leftAnchor = GameObject.Find("LeftControllerAnchor");
        GameObject rightAnchor = GameObject.Find("RightControllerAnchor");
        
        if (leftAnchor != null)
        {
            SetRenderersInHierarchy(leftAnchor, visible);
        }
        
        if (rightAnchor != null)
        {
            SetRenderersInHierarchy(rightAnchor, visible);
        }
    }
    
    private void SetRenderersInHierarchy(GameObject parent, bool visible)
    {
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>(true);
        
        foreach (Renderer renderer in renderers)
        {
            // Skip if this looks like a weapon (gun, flashlight)
            if (IsWeaponRenderer(renderer.gameObject))
            {
                if (enableDebug) Debug.Log($"[ControllerModelHider] Skipping weapon: {renderer.gameObject.name}");
                continue;
            }
            
            renderer.enabled = visible;
            
            if (enableDebug) Debug.Log($"[ControllerModelHider] Set {renderer.gameObject.name} to {(visible ? "visible" : "hidden")}");
        }
    }
    
    private bool IsWeaponRenderer(GameObject obj)
    {
        // Check if this object or its parents contain weapon keywords
        string name = obj.name.ToLower();
        
        if (name.Contains("gun") || 
            name.Contains("weapon") || 
            name.Contains("flashlight") ||
            name.Contains("revolver") ||
            name.Contains("pistol"))
        {
            return true;
        }
        
        // Check parent hierarchy
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            string parentName = parent.name.ToLower();
            if (parentName.Contains("gun") || 
                parentName.Contains("weapon") || 
                parentName.Contains("flashlight") ||
                parentName.Contains("revolver") ||
                parentName.Contains("pistol"))
            {
                return true;
            }
            parent = parent.parent;
        }
        
        return false;
    }
}

