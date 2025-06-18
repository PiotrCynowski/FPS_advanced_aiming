using UnityEngine;

namespace Player
{
    public class PlayerMouseLook : MonoBehaviour
    {
        [SerializeField] private float sensitivityX = 85f;
        [SerializeField] private float sensitivityY = 0.5f;
       
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float xClamp = 45f;

        private float mouseX, mouseY;
        private float xRotation = 0f;
        private Vector3 targetRotation;
        private bool isPause;

        private void Update()
        {
            if(isPause) return;

            transform.Rotate(Vector3.up, mouseX * Time.deltaTime);
            LimitViewAngle(); 
        }

        public void ReceiveInput(Vector2 _mouseIn)
        {
            mouseX = _mouseIn.x * sensitivityX;
            mouseY = _mouseIn.y * sensitivityY;
        }

        public void OnPauseGame(bool isPause)
        {
            this.isPause = isPause;
        }

        private void LimitViewAngle()
        {
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -xClamp, xClamp);
            targetRotation = transform.eulerAngles;
            targetRotation.x = xRotation;
            playerCamera.eulerAngles = targetRotation;
        }
    }
}
