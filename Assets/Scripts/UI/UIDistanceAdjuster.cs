using UnityEngine;

/// <summary>
/// Helper script to adjust UI distance from camera at runtime.
/// Attach to any Canvas with GameUILazyFollow component.
/// Use RIGHT controller thumbstick UP/DOWN to adjust distance.
/// </summary>
[RequireComponent(typeof(GameUILazyFollow))]
public class UIDistanceAdjuster : MonoBehaviour
{
    [Header("Adjustment Settings")]
    [Tooltip("Enable runtime adjustment with controller")]
    public bool enableRuntimeAdjustment = true;
    
    [Tooltip("Minimum distance from camera (meters)")]
    public float minDistance = 0.2f;
    
    [Tooltip("Maximum distance from camera (meters)")]
    public float maxDistance = 1.0f;
    
    [Tooltip("Adjustment speed (meters per second)")]
    public float adjustmentSpeed = 0.5f;
    
    [Header("Debug")]
    [Tooltip("Show distance in console when adjusting")]
    public bool showDebugLog = true;
    
    private GameUILazyFollow lazyFollow;
    private float currentDistance;
    private float lastLogTime = 0f;
    
    void Start()
    {
        lazyFollow = GetComponent<GameUILazyFollow>();
        if (lazyFollow != null)
        {
            currentDistance = lazyFollow.m_TargetOffset.magnitude;
            Debug.Log($"[UIDistanceAdjuster] Initial distance: {currentDistance:F2}m. Use RIGHT thumbstick UP/DOWN to adjust.");
        }
        else
        {
            Debug.LogError("[UIDistanceAdjuster] GameUILazyFollow component not found!");
            enabled = false;
        }
    }
    
    void Update()
    {
        if (!enableRuntimeAdjustment || lazyFollow == null) return;
        
        // Get RIGHT controller thumbstick Y axis
        Vector2 thumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
        
        // Adjust distance based on thumbstick Y
        if (Mathf.Abs(thumbstick.y) > 0.1f)
        {
            currentDistance += thumbstick.y * adjustmentSpeed * Time.deltaTime;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            
            // Update the lazy follow offset
            Vector3 direction = lazyFollow.m_TargetOffset.normalized;
            if (direction == Vector3.zero)
                direction = Vector3.forward;
            
            lazyFollow.m_TargetOffset = direction * currentDistance;
            
            // Log periodically
            if (showDebugLog && Time.time - lastLogTime > 1f)
            {
                Debug.Log($"[UIDistanceAdjuster] UI distance: {currentDistance:F2}m");
                lastLogTime = Time.time;
            }
        }
        
        // Reset to default with RIGHT thumbstick press
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch))
        {
            currentDistance = 0.35f; // Default distance
            Vector3 direction = lazyFollow.m_TargetOffset.normalized;
            if (direction == Vector3.zero)
                direction = Vector3.forward;
            
            lazyFollow.m_TargetOffset = direction * currentDistance;
            Debug.Log($"[UIDistanceAdjuster] Reset distance to {currentDistance:F2}m");
        }
    }
    
    /// <summary>
    /// Set the UI distance programmatically
    /// </summary>
    public void SetDistance(float distance)
    {
        currentDistance = Mathf.Clamp(distance, minDistance, maxDistance);
        
        if (lazyFollow != null)
        {
            Vector3 direction = lazyFollow.m_TargetOffset.normalized;
            if (direction == Vector3.zero)
                direction = Vector3.forward;
            
            lazyFollow.m_TargetOffset = direction * currentDistance;
        }
    }
    
    /// <summary>
    /// Get the current UI distance
    /// </summary>
    public float GetDistance()
    {
        return currentDistance;
    }
}
