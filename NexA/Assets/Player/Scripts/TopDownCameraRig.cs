using Unity.Cinemachine;
using UnityEngine.Assertions;
using UnityEngine;

namespace Player
{
    public class TopDownCameraRig: MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera vcam;
        [SerializeField] private CinemachineImpulseSource impulse;

        [Header("Look-ahead")]
        [SerializeField] private float lookAheadDistance = 2.0f;
        [SerializeField] private float lookAheadSharpness = 10f;

        private CinemachineFramingTransposer framing;
        private Vector3 currentOffset;

        private void Awake()
        {
            Assert.IsNotNull(vcam);
            framing = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
            Assert.IsNotNull(framing);
        }

        public void TickLookAhead(Vector3 aimDir, float dt)
        {
            aimDir.y = 0f;
            Vector3 target = aimDir.sqrMagnitude > 0.0001f ? aimDir.normalized * lookAheadDistance : Vector3.zero;

            float t = 1f - Mathf.Exp(-lookAheadSharpness * dt);
            currentOffset = Vector3.Lerp(currentOffset, target, t);

            framing.m_TrackedObjectOffset = currentOffset;
        }

        public void Impulse(float strength = 1f)
        {
            if (impulse != null)
                impulse.GenerateImpulse(strength);
        } 
    }
}