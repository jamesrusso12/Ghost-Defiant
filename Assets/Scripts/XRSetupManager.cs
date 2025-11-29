using UnityEngine;
using UnityEngine.EventSystems;

public class XRSetupManager : MonoBehaviour
{
    void Start()
    {
        ConfigureInputSystem();
    }

    void ConfigureInputSystem()
    {
        // Find StandaloneInputModule in the scene
        OVRInputModule inputModule = FindAnyObjectByType<OVRInputModule>();

        if (inputModule != null)
        {
            Debug.Log("OVRInputModule found");

            // Find the RightControllerAnchor
            GameObject rightControllerAnchor = GameObject.Find("RightControllerAnchor");

            if (rightControllerAnchor != null)
            {
                Debug.Log("RightControllerAnchor found: " + rightControllerAnchor.name);
            }
            else
            {
                Debug.LogError("Could not find RightControllerAnchor in the scene.");
            }
        }
        else
        {
            Debug.LogError("Could not find StandaloneInputModule in the scene.");
        }
    }
}
