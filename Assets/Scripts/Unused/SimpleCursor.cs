using UnityEngine;

public class SimpleCursor : MonoBehaviour
{
    public Transform rayTransform;
    public float defaultDistance = 2.0f;
    public bool alwaysVisible = false;

    void Update()
    {
        if (rayTransform != null)
        {
            transform.position = rayTransform.position + rayTransform.forward * defaultDistance;
            transform.rotation = rayTransform.rotation;
        }

        if (!alwaysVisible && !OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            GetComponent<Renderer>().enabled = false;
        }
        else
        {
            GetComponent<Renderer>().enabled = true;
        }
    }
}