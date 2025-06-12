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

        [Header("Weapon Switch Animation")]
        [SerializeField] private float switchAnimDistance = 0.3f;
        [SerializeField] private float switchAnimSpeed = 6f;

        private Coroutine switchCoroutine;

        private Vector3 initialLocalPos, targetOffset;

        public void Start()
        {
            PlayerWeapon.Instance.OnWeaponSwitch += OnWeaponSwitchModel;
            PlayerMovement.onJump += OnJumpOrLandAction;
        }

        public void Update()
        {
            if(weaponGO == null) return;

            weaponGO.localPosition = Vector3.Lerp(weaponGO.localPosition, initialLocalPos + targetOffset, Time.deltaTime * swaySmooth);

            Quaternion targetRotation = Quaternion.LookRotation(crosshairTarget.position - weaponGO.position);

            weaponGO.rotation = Quaternion.Slerp(
                weaponGO.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
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

        private void OnWeaponSwitchModel(Transform weaponRef, System.Action onComplete, bool switchAnim = false)
        {
            initialLocalPos = weaponRef.localPosition;
            weaponGO = weaponRef;

            if (!switchAnim)
                return;

            if (switchCoroutine != null)
                StopCoroutine(switchCoroutine);
            switchCoroutine = StartCoroutine(AnimateWeaponSwitch(weaponRef, onComplete));
        }

        private IEnumerator AnimateWeaponSwitch(Transform weaponRef, System.Action onComplete)
        {
            Vector3 downPos = initialLocalPos - new Vector3(0f, switchAnimDistance, 0f);
            float timer = 0f;

            while (timer < 1f)
            {
                timer += Time.deltaTime * switchAnimSpeed;
                weaponHolder.localPosition = Vector3.Lerp(initialLocalPos, downPos, timer);
                yield return null;
            }

            timer = 0f;

            while (timer < 1f)
            {
                timer += Time.deltaTime * switchAnimSpeed;
                weaponHolder.localPosition = Vector3.Lerp(downPos, initialLocalPos, timer);
                yield return null;
            }

            onComplete?.Invoke();
        }
    }
}