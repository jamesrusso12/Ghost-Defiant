using System.Collections;
using UnityEngine;
using UnityEngine.XR;

namespace Unity.VRTemplate
{
    /// <summary>
    /// Makes the object this is attached to follow a target with a smooth delay.
    /// </summary>
    public class LazyFollow : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The object being followed.")]
        Transform m_Target;

        [SerializeField]
        [Tooltip("Whether to only follow when out of view.")]
        bool m_FOV = false;

        [SerializeField]
        [Tooltip("Whether rotation is locked to z-axis.")]
        bool m_ZRot = true;

        [SerializeField]
        [Tooltip("Offset from the target position.")]
        Vector3 m_TargetOffset = Vector3.forward;

        [SerializeField]
        [Tooltip("Snap to target on enable.")]
        bool m_SnapOnEnable = true;

        public bool followActive = true;

        public float smoothTime = 0.3f;

        private Vector3 m_TargetLastPos;
        private Vector3 velocity = Vector3.zero;
        private Camera m_Camera;

        Vector3 targetPosition => m_Target.position + m_Target.TransformVector(m_TargetOffset);
        Quaternion targetRotation
        {
            get
            {
                if (!m_ZRot)
                {
                    Vector3 euler = m_Target.eulerAngles;
                    euler.z = 0f;
                    return Quaternion.Euler(euler);
                }
                return m_Target.rotation;
            }
        }

        void Awake()
        {
            var ovrRig = FindFirstObjectByType<OVRCameraRig>();
            if (ovrRig != null)
                m_Target = ovrRig.centerEyeAnchor;

            m_Camera = Camera.main;
        }

        void Start()
        {
            m_TargetLastPos = targetPosition;
        }

        void OnEnable()
        {
            if (m_SnapOnEnable && m_Target != null)
            {
                transform.position = targetPosition;
                velocity = Vector3.zero;
            }
        }

        void Update()
        {
            if (!followActive || m_Target == null)
                return;

            if (m_FOV && m_Camera != null)
            {
                Vector3 screenPoint = m_Camera.WorldToViewportPoint(transform.position);
                bool inFov = screenPoint.z > 0f &&
                             screenPoint.x > 0f && screenPoint.x < 1f &&
                             screenPoint.y > 0f && screenPoint.y < 1f;

                if (inFov)
                    return;
            }

            // Smoothly follow the target's position
            float positionLerpSpeed = 2.0f; // Lower is slower follow
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpSpeed);

            // Billboard effect: rotate to always face the main camera
            if (m_Camera != null)
            {
                Vector3 lookDir = transform.position - m_Camera.transform.position;
                Quaternion targetLookRotation = Quaternion.LookRotation(lookDir.normalized);
                float rotationLerpSpeed = 5.0f;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetLookRotation, Time.deltaTime * rotationLerpSpeed);
            }

            m_TargetLastPos = targetPosition;
        }


        public void Summon()
        {
            if (!followActive)
                StartCoroutine(OneTimeSummonPosition());
        }

        IEnumerator OneTimeSummonPosition()
        {
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
                yield return null;
            }
        }
    }
}
