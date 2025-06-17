using System;
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

        private Vector3 moveDirection, velocity, currentVelocity;
        private CharacterController charCtrl;
        private bool isCanJump = true;

        private bool isFocusing = false;

        public static Action<bool> onJump;

        private bool isGrounded = true;
        public bool IsGrounded
        {
            get => isGrounded;
            set
            {
                if (isGrounded != value)
                {
                    previousSprintValue = SprintValue;
                    onJump?.Invoke(value);
                    isGrounded = value;
                }
            }
        }

        private float sprintValue = 1, previousSprintValue = 1;
        public float SprintValue
         {
            get 
            {
                return isGrounded ? sprintValue : previousSprintValue;
            }
            set
            {
                if (sprintValue != value)
                {
                    sprintValue = value;
                }
            }
        }

        #region Notify
        public static event Action<float> OnStaminaChanged;
        private float currentStamina;
        public float CurrentStamina
        {
            get => currentStamina;
            set
            {
                if (currentStamina != value)
                {
                    currentStamina = value;
                    OnStaminaChanged?.Invoke(currentStamina/maxStamina);
                }
            }
        }
        #endregion

        private void Start()
        {
            charCtrl = GetComponent<CharacterController>();
            currentStamina = maxStamina;
        }

        private void Update()
        {
            IsGrounded = IsGroundedCheck();

            HandleMovement();
            HandleGravity();
            HandleStamina();

            charCtrl.Move(moveSpeed * SprintValue * Time.deltaTime * velocity);   
        }

        #region Movement
        private void HandleMovement()
        {
            Vector3 targetVelocity = moveSpeed * SprintValue * moveDirection;
            float currentAcceleration = isGrounded ?
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

        public void OnFocus(bool isFocus)
        {
            isFocusing = isFocus;
        }

        #region Jump
        public void OnJumpPressed()
        {
            if (isGrounded && isCanJump)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                StartCoroutine(JumpCooldown());
            }  
        }

        private void HandleGravity()
        {
            if (isGrounded && velocity.y < 0)
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
        public void OnSprintPressed(bool isActive)
        {
            if (isActive && CurrentStamina > 10) 
            {
                SprintValue = sprinteMultiplier;
            }
            else
            {
                SprintValue = 1;
                if (isActive) StartCoroutine(SprintCooldown());
            }
        }

        private void HandleStamina()
        {
            if (SprintValue > 1 && moveDirection.magnitude > 0.1f)
            {
                CurrentStamina -= staminaDepletionRate * Time.deltaTime;
                if (CurrentStamina <= 0)
                {
                    OnSprintPressed(false);
                    CurrentStamina = 0;
                }
            }
            else if (CurrentStamina < maxStamina)
            {
                CurrentStamina += staminaRecoveryRate * Time.deltaTime;
            }
        }

        private IEnumerator SprintCooldown()
        {
            yield return new WaitForSeconds(sprintCooldown);
            OnSprintPressed(false);
        }
        #endregion

        private bool IsGroundedCheck()
        {
            return Physics.CheckSphere(groundCheckPoint.position, checkRadius, groundLayer);
        }
    }
}