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

        private bool isJumping;


        private void Start()
        {
            charCtrl = GetComponent<CharacterController>();
        }

        private void Update()
        {
            move.x = moveDirection.x;
            move.z = moveDirection.z;

            applyGravity();

            if (isJumping)
            {
                move.y = jump;
                isJumping = false;
            }

            charCtrl.Move(move * Time.deltaTime * moveSpeed);   
        }


        public void ReceiveInput(Vector2 _horizontalInput)
        {
            moveDirection = transform.right * _horizontalInput.x + transform.forward * _horizontalInput.y;
        }

        public void OnJumpPressed()
        {
            isJumping = true;
        }


        private void applyGravity()
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