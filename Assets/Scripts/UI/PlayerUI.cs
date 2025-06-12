using UnityEngine;
using UnityEngine.UI;
using Player.WeaponData;
using Player;

namespace UI.Elements 
{
    public class PlayerUI : MonoBehaviour
    {
        [Header("Weapon")]
        [SerializeField] private Text playerWeaponStateText;
        [SerializeField] private Text playerTargetStateText;
        [SerializeField] private GameObject playerTargetPanel;
        [Header("Crosshair")]
        [SerializeField] private Image crosshair;
        [Header("Stamina")]
        [SerializeField] private GameObject staminaContainer;
        [SerializeField] private Image stamina;
        [Header("Ammo")]
        [SerializeField] private Text ammoInfo;
        [Header("Info")]
        [SerializeField] private Text infoText;
        private readonly string interactOn = "[E] to interact";

        private void Start()
        {
            staminaContainer.SetActive(false);
            Subscribe();
        }

        private void OnDestroy()
        {
            UnSubscribe();
        }

        private void Subscribe()
        {
            PlayerWeaponInfo.OnPlayerWeaponChanged += UpdateWeaponState;
            PlayerWeaponInfo.OnPlayerDestrObjChanged += UpdateDestrObjState;
            PlayerWeapon.Instance.OnWeaponObjTarget += CrosshairTargetInformation;
            PlayerWeapon.Instance.OnAmmoChange += OnAmmoChange;
            PlayerRay.OnInteractableSwitch += OnInteractInfo;
            PlayerMovement.OnStaminaChanged += OnStaminaChanged;
        }

        private void UnSubscribe()
        {
            PlayerWeaponInfo.OnPlayerWeaponChanged -= UpdateWeaponState;
            PlayerWeaponInfo.OnPlayerDestrObjChanged -= UpdateDestrObjState;
            PlayerWeapon.Instance.OnWeaponObjTarget -= CrosshairTargetInformation;
            PlayerWeapon.Instance.OnAmmoChange -= OnAmmoChange;
            PlayerRay.OnInteractableSwitch -= OnInteractInfo;
            PlayerMovement.OnStaminaChanged -= OnStaminaChanged;
        }

        #region update text
        private void UpdateWeaponState(TargetType[] canDestroyInfo)
        {
            playerWeaponStateText.text = "Weapon can destroy: " + string.Join(", ", canDestroyInfo);
        }

        private void UpdateDestrObjState(TargetType objMat, int objHP)
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

        private void OnAmmoChange(int currentAmmo, int maxAmmo)
        {
            ammoInfo.text = currentAmmo.ToString() + " / " + maxAmmo.ToString();
        }

        private void OnInteractInfo(bool isOn, string isCan)
        {
            Debug.Log("OnInteractInfo:" + isCan);
            infoText.text = string.IsNullOrEmpty(isCan) ? interactOn : isCan;
            infoText.gameObject.SetActive(isOn);
        }
    }  
}