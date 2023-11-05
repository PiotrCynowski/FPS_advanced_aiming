using UnityEngine;
using Player;
using System;
using UI.Elements;

namespace GameInput
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerMouseLook mouseLook;
        [SerializeField] private PlayerWeapon weapon;

        private PlayerInputActions controls;
        private PlayerInputActions.PlayerActions playerActions;

        private Vector2 horizontalInput;
        private Vector2 mouseInput;

        public static Action onPlayerEscButton;


        private void Awake()
        {
            controls = new PlayerInputActions();
            playerActions = controls.Player;

            playerActions.Movement.performed += ctx => horizontalInput = ctx.ReadValue<Vector2>();
            playerActions.Jump.performed += _ => movement.OnJumpPressed();

            playerActions.MouseX.performed += ctx => mouseInput.x = ctx.ReadValue<float>();
            playerActions.MouseY.performed += ctx => mouseInput.y = ctx.ReadValue<float>();

            playerActions.Shot1.performed += _ => weapon.ShotLMouseBut();
            playerActions.WeaponSwitch.performed += _ => weapon.SwitchWeaponRMouseBut();

            playerActions.PauseMenu.performed += _ => EscapeButPerformed();

            PanelPauseUI.OnPlayerPauseMenuOff += EnableControlls;
        }

        private void Update()
        {
            movement.ReceiveInput(horizontalInput);
            mouseLook.ReceiveInput(mouseInput);
        }


        #region enable/disable
        private void OnEnable()
        {
            controls.Enable();
        }

        private void OnDisable()
        {
            controls.Disable();
        }
        #endregion


        private void EscapeButPerformed()
        {
            onPlayerEscButton.Invoke();
            controls.Disable();
        }

        private void EnableControlls()
        {
            controls.Enable();
        }
    }
}
