using UnityEngine;
using Player;
using Player.WeaponData;
using System;
using UI.Elements;

namespace GameInput
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerMouseLook mouseLook;
        [SerializeField] private PlayerWeapon weapon;
        [SerializeField] private PlayerWeaponAnimation weaponAnim;
        [SerializeField] private PlayerGrabController grabController;
        [SerializeField] private PlayerUICrosshair playerCrosshair;
        [SerializeField] private PlayerInteract playerInteract;

        private PlayerInputActions controls;
        private PlayerInputActions.PlayerActions playerActions;

        private Vector2 horizontalInput;
        private Vector2 mouseInput;

        public static Action onPlayerEscButton;
        private bool isGrab, isFocus;

        private void Awake()
        {
            controls = new PlayerInputActions();
            playerActions = controls.Player;

            playerActions.Movement.performed += ctx => horizontalInput = ctx.ReadValue<Vector2>();
            playerActions.Jump.performed += _ => movement.OnJumpPressed();

            playerActions.Sprint.performed += _ => {
                movement.OnSprintPressed(true);
                weaponAnim.OnWeaponSprint(true);
            };

            playerActions.Sprint.canceled += _ => {
                movement.OnSprintPressed(false);
                weaponAnim.OnWeaponSprint(false);
            };

            playerActions.MouseX.performed += ctx => mouseInput.x = ctx.ReadValue<float>();
            playerActions.MouseY.performed += ctx => mouseInput.y = ctx.ReadValue<float>();

            playerActions.Shot1.performed += _ => OnShotLMB(true);
            playerActions.Shot1.canceled += _ => OnShotLMB(false);

            playerActions.WeaponSwitchNext.performed += _ => OnWeaponSwitchMMB(true);
            playerActions.WeaponSwitchPrevious.performed += _ => OnWeaponSwitchMMB(false);

            playerActions.GrabFocus.performed += _ => OnFocusGrabRMB(true);
            playerActions.GrabFocus.canceled += _ => OnFocusGrabRMB(false);

            playerActions.Interact.performed += _ => playerInteract.Interact();

            playerActions.PauseMenu.performed += _ => EscapeButPerformed();

            PanelPauseUI.OnPlayerPauseMenuOff += EnableControlls;
        }

        private void OnEnable()
        {
            controls.Enable();
        }

        private void OnDisable()
        {
            controls.Disable();
        }

        private void Update()
        {
            movement.ReceiveInput(horizontalInput);
            mouseLook.ReceiveInput(mouseInput);

            playerCrosshair.UpdateCrosshair(mouseInput, horizontalInput);
            weaponAnim.WeaponUpdate(mouseInput, horizontalInput);
        }

        private void EscapeButPerformed()
        {
            onPlayerEscButton.Invoke();
            controls.Disable();
            mouseLook.OnPauseGame(true);
        }

        private void EnableControlls()
        {
            controls.Enable();
            mouseLook.OnPauseGame(false);
        }

        private void OnShotLMB(bool isPerformed)
        {
            if (isGrab)
            {
                grabController.OnMouseLMB(true);
                isGrab = false;
            }
            else
            {
                weapon.ShotLMouseBut(isPerformed);
            }
        }

        private void OnFocusGrabRMB(bool isPerformed)
        {
            if (grabController.IsAimedAt() && !isFocus)
            {
                isGrab = isPerformed;
                grabController.OnMouseRMB(isPerformed);
                movement.OnFocus(false);
                weaponAnim.OnFocus(false);
            }
            else
            {
                isFocus = isPerformed;
                movement.OnFocus(isPerformed);
                weaponAnim.OnFocus(isPerformed);
            }
        }

        private void OnWeaponSwitchMMB(bool isNext)
        {
            if(!isGrab && !isFocus)
                weapon.SwitchWeaponMouseButScroll(isNext);
        }
    }
}