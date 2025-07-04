using UnityEngine;
using Player.WeaponData;
using System.Collections;

namespace Player
{
    public class PlayerWeaponAnimation : MonoBehaviour
    {
        [SerializeField] private Transform crosshairTarget;
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private float rotationSpeed = 5f;
        private Transform weaponGO;

        [Header("Sway Settings")]
        [SerializeField] private float moveSwayAmount = 0.03f;
        [SerializeField] private float mouseSwayAmount = 0.02f;
        [SerializeField] private float swaySmooth = 6f;

        [SerializeField] private float jumpSwayAmount = 0.03f;
        [SerializeField] private float landBounceAmount = 0.05f;
        [SerializeField] private float impulseDecayTime = 0.2f;
        private Vector3 swayImpulse = Vector3.zero;
        private Vector3 swayImpulseVelocity = Vector3.zero;

        [Header("Bobbing Settings")]
        [SerializeField] private float bobFrequency = 8f;
        [SerializeField] private float bobSprintFrequency = 15f;
        [SerializeField] private float bobAmplitude = 0.015f;
        [SerializeField] private float bobSpeedThreshold = 0.1f;
        [SerializeField] private float bobSwayLerp = 8f;
        private bool isGround = true, isSprint = false;

        private float bobTimer = 0f;
        private Vector3 bobOffset = Vector3.zero;

        [Header("Focus")]
        [SerializeField] private Vector3 focusOffset = new Vector3(0f, -0.02f, 0.05f);
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float focusFOV = 40f;
        [SerializeField] private float fovTransitionSpeed = 5f;
        [SerializeField] Camera playerCamera;
        private float currentFOV;
        private Vector3 currentFocusOffset = Vector3.zero;
        private bool isFocusing = false;  
    
        [Header("Weapon Switch Animation")]
        [SerializeField] private float switchAnimDistance = 0.3f;
        [SerializeField] private float switchAnimSpeed = 6f;

        private Coroutine switchCoroutine;

        private Vector3 initialLocalPos, targetOffset, downPos;

        private void Awake()
        {
            downPos = Vector3.zero - new Vector3(0f, switchAnimDistance, 0f);
        }

        private void Start()
        {
            PlayerWeapon.Instance.OnWeaponSwitch += OnWeaponSwitchModel;
            PlayerMovement.onJump += OnJumpOrLandAction;

            currentFOV = playerCamera.fieldOfView;
        }

        private void Update()
        {
            if(weaponGO == null) return;

            Vector3 desiredPos = initialLocalPos + targetOffset + currentFocusOffset;
            weaponGO.localPosition = Vector3.Lerp(weaponGO.localPosition, desiredPos, Time.deltaTime * swaySmooth);

            Quaternion targetRotation = Quaternion.LookRotation(crosshairTarget.position - weaponGO.position);

            weaponGO.rotation = Quaternion.Slerp(
                weaponGO.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );

            currentFOV = Mathf.Lerp(currentFOV, isFocusing ? focusFOV : normalFOV, Time.deltaTime * fovTransitionSpeed);
            playerCamera.fieldOfView = currentFOV;
        }

        private void OnDestroy()
        {
            PlayerWeapon.Instance.OnWeaponSwitch -= OnWeaponSwitchModel;
            PlayerMovement.onJump -= OnJumpOrLandAction;
        }

        public void OnWeaponSprint(bool isPlayerSprint)
        {
            isSprint = isPlayerSprint;
        }

        public void OnFocus(bool isFocus)
        {
            isFocusing = isFocus;
            currentFocusOffset = isFocusing ? focusOffset : Vector3.zero;
        }

        public void WeaponUpdate(Vector2 mouseInput, Vector2 movementInput)
        {
            //SWAY
            Vector3 moveOffset = new Vector3(movementInput.x, 0, 0) * moveSwayAmount;
            Vector3 mouseOffset = new Vector3(-mouseInput.x, -mouseInput.y, 0f) * mouseSwayAmount;

            swayImpulse = Vector3.SmoothDamp(swayImpulse, Vector3.zero, ref swayImpulseVelocity, impulseDecayTime);

            //BOBBING
            float playerSpeed = new Vector2(movementInput.x, movementInput.y).magnitude;
            if (playerSpeed > bobSpeedThreshold && isGround)
            {
                bobTimer += Time.deltaTime * (!isSprint ? bobFrequency : bobSprintFrequency);
                float bobY = Mathf.Sin(bobTimer) * bobAmplitude;
                bobOffset = new Vector3(0f, bobY, 0f);
            }
            else
            {
                bobTimer = 0f;
                bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, Time.deltaTime * bobSwayLerp);
            }

            targetOffset = moveOffset + mouseOffset + swayImpulse + bobOffset;
        }

        private void OnJumpOrLandAction(bool isJump)
        {
            isGround = isJump;
            swayImpulse = new Vector3(0f, isJump ? -jumpSwayAmount : landBounceAmount, 0f);
        }

        private void OnWeaponSwitchModel(Transform weaponRef, System.Action onMiddleAnim, System.Action onComplete)
        {
            initialLocalPos = weaponRef.localPosition;
            weaponGO = weaponRef;

            if (switchCoroutine != null)
                StopCoroutine(switchCoroutine);
            switchCoroutine = StartCoroutine(AnimateWeaponSwitch(onMiddleAnim, onComplete));
        }

        private IEnumerator AnimateWeaponSwitch(System.Action onMiddleAnim, System.Action onComplete)
        {
            float timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime * switchAnimSpeed;
                weaponHolder.localPosition = Vector3.Lerp(Vector3.zero, downPos, timer);
                yield return null;
            }

            onMiddleAnim?.Invoke();
            timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime * switchAnimSpeed;
                weaponHolder.localPosition = Vector3.Lerp(downPos, Vector3.zero, timer);
                yield return null;
            }

            onComplete?.Invoke();
        }
    }
}