using UnityEngine;
using UnityEngine.Assertions;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private RigidbodyCharacterMotor motor;

        private Vector3 cachedAim;
        private float lastDashTime;

        private void Awake()
        {
            Assert.IsNotNull(input);
            Assert.IsNotNull(motor);
            
            cachedAim = transform.forward;
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            Vector2 move2D = input.ReadMove();
            Vector3 moveWorld = new Vector3(move2D.x, 0f, move2D.y);

            Vector3 aimDir = input.ReadAimWorld(transform.position);
            if (aimDir.sqrMagnitude > 0.001f)
            {
                cachedAim = aimDir;
            }
            else if (moveWorld.sqrMagnitude > motor.Tuning.minMoveToFaceMoveDir)
            {
                cachedAim = moveWorld.normalized;
            }

            if (input.ConsumeDash() && motor.CanDash(lastDashTime))
            {
                lastDashTime = Time.time;
                Vector3 dashDir = moveWorld.sqrMagnitude > 0.001f ? moveWorld.normalized : transform.forward;
                motor.StartDash(dashDir);
            }

            motor.TickMove(moveWorld, dt);
            motor.TickRotate(cachedAim, dt);
        }
    }
}