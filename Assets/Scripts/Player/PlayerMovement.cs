using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed;

        private Vector3 moveDirection;
        private Vector3 move;
        private CharacterController charCtrl;
      
        private const float gravity = -25;
        private const float jump = 5;

        private bool isJumping, isJumpTimer;
        private float jumpTimer;

        private void Start()
        {
            charCtrl = GetComponent<CharacterController>();
        }

        private void Update()
        {
            move.x = moveDirection.x;
            move.z = moveDirection.z;

            ApplyGravity();

            if (isJumping)
            {
                move.y = jump;
                isJumpTimer = true;
                isJumping = false;
            }

            if (isJumpTimer)
            {
                jumpTimer += Time.deltaTime;
                if (jumpTimer > 0.35f)
                {
                    jumpTimer = 0;
                    isJumpTimer = false;
                }
            }

            charCtrl.Move(moveSpeed * Time.deltaTime * move);   
        }

        public void ReceiveInput(Vector2 _horizontalInput)
        {
            moveDirection = transform.right * _horizontalInput.x + transform.forward * _horizontalInput.y;
        }

        public void OnJumpPressed()
        {
            if(!isJumpTimer)
                isJumping = true;
        }

        private void ApplyGravity()
        {
            if (charCtrl.isGrounded)
            {
                move.y = 0;
                return;
            }

            move.y += gravity * Time.deltaTime;
        }
    }
}