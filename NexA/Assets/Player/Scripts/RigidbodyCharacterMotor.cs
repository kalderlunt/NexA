using UnityEngine;
using UnityEngine.Assertions;

namespace Player
{
    
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyCharacterMotor : MonoBehaviour
    {
        [SerializeField] private CharacterTuning tuning;
        public CharacterTuning Tuning => tuning;
        
        private Rigidbody rb;
        private Vector3 planarVelocity;

        private bool isDashing;
        private float dashEndTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            Assert.IsNotNull(rb);
            Assert.IsNotNull(tuning);

            rb.useGravity = true;
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        public void TickMove(Vector3 moveDir, float dt)
        {
            moveDir.y = 0f;
            if (moveDir.sqrMagnitude > 1f)
                moveDir.Normalize();

            if (isDashing && Time.time >= dashEndTime)
                isDashing = false;

            Vector3 targetVel;
            float rate;

            if (isDashing)
            {
                targetVel = planarVelocity;
                rate = 9999f;
            }
            else
            {
                targetVel = moveDir * tuning.maxSpeed;
                rate = moveDir.sqrMagnitude > 0.001f ? tuning.acceleration : tuning.deceleration;
            }

            planarVelocity = Vector3.MoveTowards(planarVelocity, targetVel, rate * dt);

            Vector3 velocity = new Vector3(planarVelocity.x, rb.linearVelocity.y, planarVelocity.z);
            rb.linearVelocity = velocity;
        }

        public void TickRotate(Vector3 aimDir, float dt)
        {
            aimDir.y = 0f;
            if (aimDir.sqrMagnitude < 0.001f)
                return;

            Quaternion target = Quaternion.LookRotation(aimDir);
            float t = 1f - Mathf.Exp(-tuning.rotationSharpness * dt);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, t));
        }

        public bool CanDash(float lastDashTime)
        {
            return !isDashing && Time.time - lastDashTime >= tuning.dashCooldown;
        }

        public void StartDash(Vector3 dir)
        {
            if (dir.sqrMagnitude < 0.001f)
                dir = transform.forward;

            isDashing = true;
            dashEndTime = Time.time + tuning.dashDuration;
            planarVelocity = dir.normalized * tuning.dashSpeed;
        }
    }
}