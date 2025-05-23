using System.Collections;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 15f;
        [SerializeField] private float airControl = 0.5f;
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float checkRadius;

        [Header("Sprint Settings")]
        [SerializeField] private float sprinteMultiplier = 2f;
        [SerializeField] private float sprintCooldown = 2f;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDepletionRate = 20f;
        [SerializeField] private float staminaRecoveryRate = 15f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpCooldown = 0.35f;
        [SerializeField] private float jumpHeight = 5f;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float groundCheckDistance = 2f;

        private Vector3 moveDirection;
        private Vector3 velocity;
        private Vector3 currentVelocity;
        private float currentStamina;
        private float sprintValue = 1;
        private CharacterController charCtrl;   
        private bool isCanJump = true;

        private void Start()
        {
            charCtrl = GetComponent<CharacterController>();
        }

        private void Update()
        {
            HandleMovement();
            HandleGravity();
            HandleStamina();

            charCtrl.Move(moveSpeed * Time.deltaTime * velocity * sprintValue);   
        }

        #region Movement
        private void HandleMovement()
        {
            Vector3 targetVelocity = moveSpeed * sprintValue * moveDirection;
            float currentAcceleration = IsGrounded() ?
                (moveDirection.magnitude > 0.1f ? acceleration : deceleration) :
                (moveDirection.magnitude > 0.1f ? acceleration * airControl : deceleration * airControl);

            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, currentAcceleration * Time.deltaTime);

            velocity.x = currentVelocity.x;
            velocity.z = currentVelocity.z;
        }

        public void ReceiveInput(Vector2 _horizontalInput)
        {
            Vector3 input = new Vector3(_horizontalInput.x, 0, _horizontalInput.y);
            if (input.magnitude > 1f) input.Normalize();
            moveDirection = transform.right * _horizontalInput.x + transform.forward * _horizontalInput.y;
        }
        #endregion

        #region Jump
        public void OnJumpPressed()
        {
            if (IsGrounded() && isCanJump)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                StartCoroutine(JumpCooldown());
            }  
        }

        private void HandleGravity()
        {
            if (IsGrounded() && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
        }

        private IEnumerator JumpCooldown()
        {
            isCanJump = false;
            yield return new WaitForSeconds(jumpCooldown);
            isCanJump = true;
        }
        #endregion

        #region Sprint
        public float GetCurrentStaminaNormalized() => currentStamina / maxStamina;

        public void OnSprintPressed(bool isActive)
        {
            if (isActive && currentStamina > 10) 
            {
                sprintValue = sprinteMultiplier;
            }
            else
            {
                sprintValue = 1;
                if (isActive) StartCoroutine(SprintCooldown());
            }
        }

        private void HandleStamina()
        {
            if (sprintValue > 1 && moveDirection.magnitude > 0.1f)
            {
                currentStamina -= staminaDepletionRate * Time.deltaTime;
                if (currentStamina <= 0)
                {
                    OnSprintPressed(false);
                    currentStamina = 0;
                }
            }
            else if (currentStamina < maxStamina)
            {
                currentStamina += staminaRecoveryRate * Time.deltaTime;
            }
        }

        private IEnumerator SprintCooldown()
        {
            yield return new WaitForSeconds(sprintCooldown);
            OnSprintPressed(false);
        }
        #endregion

        private bool IsGrounded()
        {
            return Physics.CheckSphere(groundCheckPoint.position, checkRadius, groundLayer);
        }
    }
}