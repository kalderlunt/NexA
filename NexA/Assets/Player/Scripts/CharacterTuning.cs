using UnityEngine;

namespace Player
{
    [System.Serializable]
    public class CharacterTuning
    {
        [Header("Move")]
        public float maxSpeed = 6.5f;
        public float acceleration = 40f;
        public float deceleration = 55f;
        public float rotationSharpness = 18f;

        [Header("Dash")]
        public float dashSpeed = 14f;
        public float dashDuration = 0.12f;
        public float dashCooldown = 0.5f;

        [Header("Aim")]
        public float minMoveToFaceMoveDir = 0.2f;
    }
}