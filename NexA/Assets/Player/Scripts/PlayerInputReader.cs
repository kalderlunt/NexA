using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInputReader : MonoBehaviour
    { 
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask groundMask = -1; // -1 = tous les layers par défaut

        private InputAction move;
        private InputAction look;
        private InputAction dash;

        private bool dashPressed;
        private Plane groundPlane;
        
        private void Awake()
        {
            Assert.IsNotNull(playerInput);

            if (mainCamera == null)
                mainCamera = Camera.main;

            move = playerInput.actions["Move"];
            look = playerInput.actions["Look"];
            dash = playerInput.actions["Dash"];

            Assert.IsNotNull(move);
            Assert.IsNotNull(look);
            Assert.IsNotNull(dash);

            dash.performed += _ => dashPressed = true;
        }

        public Vector2 ReadMove()
        {
            Vector2 v = move.ReadValue<Vector2>();
            return v.sqrMagnitude > 1f ? v.normalized : v;
        }

        public Vector3 ReadAimWorld(Vector3 playerPos)
        {
            // Souris → raycast ou plan virtuel
            if (Mouse.current != null)
            {
                Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                Vector3 hitPoint;
                
                groundPlane = new Plane(Vector3.up, playerPos);
                
                if (groundPlane.Raycast(ray, out float enter))
                {
                    hitPoint = ray.GetPoint(enter);
                }
                else
                {
                    // Fallback : projection du rayon dans la direction forward
                    hitPoint = playerPos + ray.direction * 10f;
                }
                
                Vector3 dir = hitPoint - playerPos;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                    return dir.normalized;
            }

            // Gamepad → stick droit
            Vector2 stick = look.ReadValue<Vector2>();
            if (stick.sqrMagnitude > 0.15f)
                return new Vector3(stick.x, 0f, stick.y).normalized;

            return Vector3.zero;
        }

        public bool ConsumeDash()
        {
            bool v = dashPressed;
            dashPressed = false;
            return v;
        }
    }
}