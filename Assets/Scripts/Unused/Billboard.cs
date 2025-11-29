using UnityEngine;

namespace XRMultiplayer
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private float positionLerpSpeed = 5f;
        [SerializeField] private float rotationLerpSpeed = 5f;
        [SerializeField] private Vector3 positionOffset = new Vector3(0f, 0f, 1f);
        [SerializeField] private bool keepUpright = true;

        private Camera m_Camera;
        private bool m_CameraReady = false;

        private void Start()
        {
            StartCoroutine(WaitForCamera());
        }

        private System.Collections.IEnumerator WaitForCamera()
        {
            while (Camera.main == null || Camera.main.transform.forward == Vector3.zero)
                yield return null;

            m_Camera = Camera.main;
            m_CameraReady = true;
        }

        private void LateUpdate()
        {
            if (!m_CameraReady || m_Camera == null)
                return;

            // Target position in front of the camera
            Vector3 targetPos =
                m_Camera.transform.position +
                m_Camera.transform.forward * positionOffset.z +
                m_Camera.transform.right * positionOffset.x +
                m_Camera.transform.up * positionOffset.y;

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * positionLerpSpeed);

            // Look at the camera with optional upright lock
            Quaternion targetRot = Quaternion.LookRotation(transform.position - m_Camera.transform.position);

            if (keepUpright)
            {
                Vector3 euler = targetRot.eulerAngles;
                targetRot = Quaternion.Euler(0, euler.y, 0);
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationLerpSpeed);
        }
    }
}
