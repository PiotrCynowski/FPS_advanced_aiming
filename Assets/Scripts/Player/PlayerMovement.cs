using System.Collections;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed;
        [SerializeField] private float jumpCooldown = 0.35f;
        [SerializeField] private float jumpHeight = 5f;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float groundCheckDistance = 2f;

        private Vector3 moveDirection;
        private Vector3 velocity;
        private CharacterController charCtrl;
      
        private bool isCanJump = true;

        private void Start()
        {
            charCtrl = GetComponent<CharacterController>();
        }

        private void Update()
        {
            ApplyGravity();

            velocity.x = moveDirection.x;
            velocity.z = moveDirection.z;

            charCtrl.Move(moveSpeed * Time.deltaTime * velocity);   
        }

        public void ReceiveInput(Vector2 _horizontalInput)
        {
            moveDirection = transform.right * _horizontalInput.x + transform.forward * _horizontalInput.y;
        }

        public void OnJumpPressed()
        {
            if (IsGrounded() && isCanJump)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                StartCoroutine(JumpCooldown());
            }
        }

        private void ApplyGravity()
        {
            if (IsGrounded() && velocity.y < 0)
            {
                velocity.y = -2f; 
            }

            velocity.y += gravity * Time.deltaTime;
        }

        private bool IsGrounded()
        {
            return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
        }

        private IEnumerator JumpCooldown()
        {
            isCanJump = false;
            yield return new WaitForSeconds(jumpCooldown);
            isCanJump = true;
        }
    }
}