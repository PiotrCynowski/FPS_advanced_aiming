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

        #region enable/disable
        private void OnEnable()
        {
            PlayerGameInfo.OnPlayerWeaponChanged += UpdateWeaponState;
            PlayerGameInfo.OnPlayerDestrObjChanged += UpdateDestrObjState;
            PlayerWeapon.OnDestObjTarget += CrosshairTargetInformation;
        }

        private void OnDisable()
        {
            PlayerGameInfo.OnPlayerWeaponChanged -= UpdateWeaponState;
            PlayerGameInfo.OnPlayerDestrObjChanged -= UpdateDestrObjState;
            PlayerWeapon.OnDestObjTarget -= CrosshairTargetInformation;
        }
        #endregion

        #region update text
        private void UpdateWeaponState(ObjectMaterials[] canDestroyInfo)
        {
            playerWeaponStateText.text = "Weapon can destroy: " + string.Join(", ", canDestroyInfo);
        }

        private void UpdateDestrObjState(ObjectMaterials objMat, int objHP)
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
    }  
}