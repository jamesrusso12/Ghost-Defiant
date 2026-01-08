using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Helper component to detect when an item has been picked up by the player.
/// SAFE VERSION: Uses Events or Parent Checking only. No Reflection.
/// </summary>
public class LoadoutItem : MonoBehaviour
{
    public bool IsPickedUp { get; private set; } = false;
    
    [Header("Manual Pickup Trigger")]
    [Tooltip("Link your ISDK Grab Event here in the Inspector")]
    public UnityEvent OnPickedUp;

    private OVRGrabbable ovrGrabbable;
    private Rigidbody rb;
    private bool initialized = false;
    private Transform originalParent;

    void Start()
    {
        // 1. Setup OVRGrabbable (Legacy) - This is safe
        ovrGrabbable = GetComponent<OVRGrabbable>();
        
        rb = GetComponent<Rigidbody>();
        
        // Store original parent to detect hierarchy changes
        originalParent = transform.parent;

        initialized = true;
    }

    // Public method to be called from Inspector Events (ISDK Interactor Events)
    public void ConfirmPickup()
    {
        if (IsPickedUp) return;
        TriggerPickup("Manual Event Call");
    }

    void Update()
    {
        if (IsPickedUp || !initialized) return;

        // 1. Check OVRGrabbable (Legacy)
        if (ovrGrabbable != null && ovrGrabbable.isGrabbed)
        {
            TriggerPickup("OVRGrabbable");
            return;
        }

        // 2. Check for Parent Change (Universal)
        // This is the safest "Auto-detect" method that doesn't touch SDK code
        if (transform.parent != originalParent)
        {
            if (IsValidHandParent(transform.parent))
            {
                TriggerPickup("Parent Change");
                return;
            }
        }

        // 3. Fallback: Kinematic + Elevation
        if (rb != null && rb.isKinematic && transform.position.y > 0.5f)
        {
             if (IsValidHandParent(transform.parent))
             {
                 TriggerPickup("Kinematic + Elevation");
             }
        }
    }

    bool IsValidHandParent(Transform parent)
    {
        if (parent == null) return false;
        string name = parent.name.ToLower();
        return name.Contains("hand") || 
               name.Contains("controller") || 
               name.Contains("interactor") || 
               name.Contains("grab") ||
               name.Contains("socket");
    }

    void TriggerPickup(string detectionMethod)
    {
        IsPickedUp = true;
        Debug.Log($"[LoadoutItem] {name} picked up! Detected via: {detectionMethod}");
        OnPickedUp?.Invoke();
    }
}

