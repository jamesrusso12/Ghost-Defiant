using System.Collections;
using UnityEngine;

namespace Meta.XR.MRUtilityKit
{
    public class StartMenuPanelPlacement : MonoBehaviour
    {
        [Header("Panel Placement Settings")]
        [Tooltip("Reference to the user's center eye anchor.")]
        [SerializeField] private Transform centerEyeAnchor;
        [Tooltip("The panel that holds the Start Menu UI (make sure this has a World Space Canvas as a child).")]
        [SerializeField] private Transform panel;
        [Tooltip("Distance from the user where the panel will appear.")]
        [SerializeField] private float distanceFromUser = 2f;

        private OVRSpatialAnchor spatialAnchor;

        private IEnumerator Start()
        {
            // Wait until the headset is tracking.
            while (!OVRPlugin.userPresent || !OVRManager.isHmdPresent)
            {
                yield return null;
            }
            yield return null;

            // Calculate the panel position in front of the user.
            Vector3 panelPosition = centerEyeAnchor.position + centerEyeAnchor.forward * distanceFromUser;
            panel.position = panelPosition;

            // Rotate the panel to face the user.
            Vector3 lookDirection = Vector3.ProjectOnPlane(centerEyeAnchor.position - panelPosition, Vector3.up).normalized;
            panel.rotation = Quaternion.LookRotation(lookDirection);

            // Create a spatial anchor to lock the panel's position.
            spatialAnchor = new GameObject("OVRSpatialAnchor").AddComponent<OVRSpatialAnchor>();
            spatialAnchor.transform.SetPositionAndRotation(panel.position, panel.rotation);
            panel.SetParent(spatialAnchor.transform);
        }
    }
}
