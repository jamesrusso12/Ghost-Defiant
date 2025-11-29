using UnityEngine;

public class BillboardMenu : MonoBehaviour
{
    [Tooltip("Target camera, usually the main VR camera.")]
    public Transform targetCamera;

    [Tooltip("Distance in front of the camera.")]
    public float distance = 2f;

    [Tooltip("Optional offset from the default forward position.")]
    public Vector3 offset = Vector3.zero;

    [Tooltip("Should the menu smoothly follow the camera?")]
    public bool smoothFollow = true;

    [Tooltip("Speed of the smooth movement.")]
    public float smoothSpeed = 5f;

    void Start()
    {
        // If no target is assigned, default to the main camera.
        if (targetCamera == null && Camera.main != null)
        {
            targetCamera = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null)
            return;

        // Calculate the desired position in front of the camera.
        Vector3 desiredPosition = targetCamera.position + (targetCamera.forward * distance) + offset;

        // Smoothly interpolate to the desired position if needed.
        if (smoothFollow)
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        else
            transform.position = desiredPosition;

        // Billboard effect: rotate the menu so it always faces the camera.
        // Optionally, to keep the menu level horizontally, you can zero out the Y component:
        Vector3 lookDirection = targetCamera.position - transform.position;
        lookDirection.y = 0; // Uncomment this line if you want the menu to only rotate on the Y axis.

        // Rotate to face the camera.
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        if (smoothFollow)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
        else
            transform.rotation = targetRotation;
    }
}
