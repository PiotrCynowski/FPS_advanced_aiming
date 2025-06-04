using UnityEngine;
using UnityEngine.UI;
using Player;

namespace UI.Elements 
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private Text playerWeaponStateText;
        [SerializeField] private Text playerTargetStateText;
        [SerializeField] private GameObject playerTargetPanel;

        [SerializeField] private Image crosshair;

        [SerializeField] private GameObject staminaContainer;
        [SerializeField] private Image stamina;

        private void OnEnable()
        {
            PlayerGameInfo.OnPlayerWeaponChanged += UpdateWeaponState;
            PlayerGameInfo.OnPlayerDestrObjChanged += UpdateDestrObjState;
            PlayerWeapon.OnDestObjTarget += CrosshairTargetInformation;
            PlayerMovement.OnStaminaChanged += OnStaminaChanged;
        }

        private void OnDisable()
        {
            PlayerGameInfo.OnPlayerWeaponChanged -= UpdateWeaponState;
            PlayerGameInfo.OnPlayerDestrObjChanged -= UpdateDestrObjState;
            PlayerWeapon.OnDestObjTarget -= CrosshairTargetInformation;
            PlayerMovement.OnStaminaChanged -= OnStaminaChanged;
        }

        private void Start()
        {
            staminaContainer.SetActive(false);
        }

        #region update text
        private void UpdateWeaponState(ObjectType[] canDestroyInfo)
        {
            playerWeaponStateText.text = "Weapon can destroy: " + string.Join(", ", canDestroyInfo);
        }

        private void UpdateDestrObjState(ObjectType objMat, int objHP)
        {
            playerTargetStateText.text = objMat + " ,HP:" + objHP;
        }
        #endregion 

        private void CrosshairTargetInformation(CrosshairTarget currentTarget)
        {
            switch (currentTarget)
            {
                case CrosshairTarget.Destroy:
                    crosshair.color = Color.green;
                    break;
                case CrosshairTarget.CantDestroy:
                    crosshair.color = Color.red;
                    break;
                case CrosshairTarget.None:
                    crosshair.color = Color.white;
                    playerTargetPanel.SetActive(false);
                    return;
                default:
                    break;
            }

            playerTargetPanel.SetActive(true);
        }

        private void OnStaminaChanged(float Stamina)
        {
            if (!staminaContainer.activeSelf)
            {
                staminaContainer.SetActive(true);
            }

            stamina.fillAmount = Stamina;

            if (stamina.fillAmount >= 1)
            {
                staminaContainer.SetActive(false);
            }
        }
    }  
}