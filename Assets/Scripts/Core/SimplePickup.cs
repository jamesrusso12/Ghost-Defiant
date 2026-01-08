using UnityEngine;

/// <summary>
/// Simple dummy pickup that destroys itself and activates the real item in hand.
/// </summary>
public class SimplePickup : MonoBehaviour
{
    public enum ItemType { Gun, Flashlight }
    public ItemType itemType;

    private bool isPickedUp = false;

    // Detect collision with Hand
    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;

        // Check if it's a hand or controller
        // Meta SDK usually names colliders "HandCollider" or objects "Hand..."
        string name = other.gameObject.name.ToLower();
        string tag = other.gameObject.tag.ToLower();

        if (name.Contains("hand") || name.Contains("controller") || 
            tag.Contains("hand") || tag.Contains("player"))
        {
            Pickup();
        }
    }
    
    // Fallback: Use OnCollisionEnter as well just in case
    private void OnCollisionEnter(Collision collision)
    {
        if (isPickedUp) return;
        
        string name = collision.gameObject.name.ToLower();
        string tag = collision.gameObject.tag.ToLower();
        
        if (name.Contains("hand") || name.Contains("controller") || 
            tag.Contains("hand") || tag.Contains("player"))
        {
            Pickup();
        }
    }

    public void Pickup()
    {
        if (isPickedUp) return;
        isPickedUp = true;

        Debug.Log($"[SimplePickup] Picked up {itemType}!");

        // Notify Manager
        if (LoadoutManager.instance != null)
        {
            LoadoutManager.instance.OnItemPickedUp(itemType);
        }

        // Play sound effect? (Optional)

        // Destroy this dummy object
        Destroy(gameObject);
    }
}

