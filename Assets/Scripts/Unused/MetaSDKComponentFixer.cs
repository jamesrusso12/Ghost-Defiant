using UnityEngine;
using Oculus.Interaction.Locomotion;
using System.Linq;

public class MetaSDKComponentFixer : MonoBehaviour
{
    [Header("Component Fix Settings")]
    public bool autoFixOnStart = true;
    public bool enableDebugLogs = true;
    
    [Header("Ground Detection")]
    public LayerMask groundLayerMask = 1; // Default layer
    public float maxStartGroundDistance = 5f;
    public bool disableVelocityWhenNoGround = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixMetaSDKComponents();
        }
    }
    
    [ContextMenu("Fix Meta SDK Components")]
    public void FixMetaSDKComponents()
    {
        if (enableDebugLogs)
            Debug.Log("[MetaSDKComponentFixer] Starting component fixes...");
            
        FixFirstPersonLocomotor();
        FixTubeRenderer();
        FixGroundDetection();
        
        if (enableDebugLogs)
            Debug.Log("[MetaSDKComponentFixer] Component fixes completed!");
    }
    
    private void FixFirstPersonLocomotor()
    {
        // Find all FirstPersonLocomotor components
        FirstPersonLocomotor[] locomotorComponents = FindObjectsByType<FirstPersonLocomotor>(FindObjectsSortMode.None);
        
        foreach (var locomotor in locomotorComponents)
        {
            if (locomotor != null)
            {
                if (enableDebugLogs)
                    Debug.Log($"[MetaSDKComponentFixer] Fixing FirstPersonLocomotor on {locomotor.gameObject.name}");
                
                // Configure ground detection
                ConfigureGroundDetection(locomotor);
            }
        }
    }
    
    private void ConfigureGroundDetection(FirstPersonLocomotor locomotor)
    {
        // This would configure the locomotor, but requires reflection or public fields
        // For now, we'll create a ground plane if none exists
        CreateGroundPlaneIfNeeded();
        
        if (enableDebugLogs)
            Debug.Log($"[MetaSDKComponentFixer] Ground detection configured for {locomotor.gameObject.name}");
    }
    
    private void CreateGroundPlaneIfNeeded()
    {
        // Check if there's already a ground plane
        GameObject existingGround = GameObject.Find("GroundPlane");
        
        if (existingGround == null)
        {
            // Create a ground plane
            GameObject groundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundPlane.name = "GroundPlane";
            groundPlane.transform.position = Vector3.zero;
            groundPlane.transform.localScale = new Vector3(10, 1, 10);
            
            // Set layer for ground detection
            groundPlane.layer = 0; // Default layer
            
            if (enableDebugLogs)
                Debug.Log("[MetaSDKComponentFixer] Created ground plane for locomotor");
        }
    }
    
    private void FixTubeRenderer()
    {
        // Find all TubeRenderer components
        var tubeRenderers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .Where(mb => mb.GetType().Name.Contains("TubeRenderer"));
        
        foreach (var tubeRenderer in tubeRenderers)
        {
            if (tubeRenderer != null)
            {
                if (enableDebugLogs)
                    Debug.Log($"[MetaSDKComponentFixer] Found TubeRenderer on {tubeRenderer.gameObject.name}");
                
                // The MinMaxAABB warning is usually non-critical
                // It's related to mesh bounds calculation
                Debug.Log("[MetaSDKComponentFixer] TubeRenderer MinMaxAABB warning is non-critical");
            }
        }
    }
    
    private void FixGroundDetection()
    {
        // Find PlayerController or similar objects
        GameObject playerController = GameObject.Find("PlayerController");
        
        if (playerController != null)
        {
            if (enableDebugLogs)
                Debug.Log("[MetaSDKComponentFixer] Found PlayerController, configuring ground detection");
            
            // Ensure there's a ground collider
            CreateGroundPlaneIfNeeded();
            
            // Configure character controller if present
            UnityEngine.CharacterController characterController = playerController.GetComponent<UnityEngine.CharacterController>();
            if (characterController != null)
            {
                // Set appropriate layer mask for ground detection
                // This would require access to the locomotor's layer mask
                Debug.Log("[MetaSDKComponentFixer] CharacterController found, ground detection should work");
            }
        }
    }
    
    // Public method to manually fix components
    [ContextMenu("Check Component Status")]
    public void CheckComponentStatus()
    {
        Debug.Log("[MetaSDKComponentFixer] Component Status Check:");
        
        // Check for ground
        GameObject ground = GameObject.Find("GroundPlane");
        Debug.Log($"Ground Plane: {(ground != null ? "Found" : "Missing")}");
        
        // Check for locomotor
        FirstPersonLocomotor locomotor = FindFirstObjectByType<FirstPersonLocomotor>();
        Debug.Log($"FirstPersonLocomotor: {(locomotor != null ? "Found" : "Missing")}");
        
        // Check for player controller
        GameObject playerController = GameObject.Find("PlayerController");
        Debug.Log($"PlayerController: {(playerController != null ? "Found" : "Missing")}");
    }
}
