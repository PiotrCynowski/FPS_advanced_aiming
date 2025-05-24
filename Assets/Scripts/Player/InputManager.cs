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
        [SerializeField] private PlayerGrabController grabController;

        private PlayerInputActions controls;
        private PlayerInputActions.PlayerActions playerActions;

        private Vector2 horizontalInput;
        private Vector2 mouseInput;

        public static Action onPlayerEscButton;
        private bool isRMB;

        private void Awake()
        {
            controls = new PlayerInputActions();
            playerActions = controls.Player;

            playerActions.Movement.performed += ctx => horizontalInput = ctx.ReadValue<Vector2>();
            playerActions.Jump.performed += _ => movement.OnJumpPressed();
            playerActions.Sprint.performed += _ => movement.OnSprintPressed(true);
            playerActions.Sprint.canceled += _ => movement.OnSprintPressed(false);

            playerActions.MouseX.performed += ctx => mouseInput.x = ctx.ReadValue<float>();
            playerActions.MouseY.performed += ctx => mouseInput.y = ctx.ReadValue<float>();

            playerActions.Shot1.performed += _ => OnShot1LMB();
            playerActions.WeaponSwitch.performed += _ => weapon.SwitchWeaponRMouseBut();

            playerActions.Grab.performed += ctx => { grabController.OnMouseRMB(true); isRMB = true; };
            playerActions.Grab.canceled += ctx => { grabController.OnMouseRMB(false); isRMB = false; };
            
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
        }

        private void EscapeButPerformed()
        {
            onPlayerEscButton.Invoke();
            controls.Disable();
        }

        private void EnableControlls()
        {
            controls.Enable();
        }

        private void OnShot1LMB() 
            {
            if (isRMB) 
                {
                grabController.OnMouseLMB(true);
            }
            else 
            {
                weapon.ShotLMouseBut();
            }
        }
    }
}
