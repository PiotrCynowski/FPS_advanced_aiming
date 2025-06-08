using UnityEngine;
using Player.WeaponData;
using System;

namespace Player
{
    public class PlayerRay : MonoBehaviour
    {
        [SerializeField] private Transform crosshairTarget;

        [SerializeField] private RectTransform crosshairUI;
        [SerializeField] private Camera fpsCamera;
        [SerializeField] private LayerMask targetLayerForCrosshair;
        [SerializeField] private LayerMask weaponLayerForCrosshair;
        [SerializeField] private float defaultAimDistance = 10f;

        [SerializeField] PlayerWeapon weapon;
        [SerializeField] PlayerGrabController grabController;
        [SerializeField] PlayerInteract interact;

        private bool isMatchedRay;

        private Ray ray;
        private RaycastHit hit, hitWeapon;
        private const int rayDistance = 2, weaponRayDistance = 25;

        #region properties
        private bool isTD; //Target Damageable
        public bool IsTD
        {
            get
            {
                return isTD;
            }
            set
            {
                if (isTD != value)
                {
                    isTD = value;
                }
            }
        }

        private bool isTG; //Target Grabbable
        public bool IsTG
        {
            get
            {
                return isTG;
            }
            set
            {
                if (isTG != value)
                {
                    isTG = value;
                }
            }
        }

        public static event Action<bool> OnInteractableSwitch;
        private bool isTI; //Target Interactable
        public bool IsTI
        {
            get
            {
                return isTI;
            }
            set
            {
                if (isTI != value)
                {
                    isTI = value;
                    OnInteractableSwitch?.Invoke(isTI);
                }
            }
        }
        #endregion

        private void Start()
        {
            ray = fpsCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, crosshairUI.position));
            crosshairTarget.position = ray.GetPoint(defaultAimDistance);
        }

        private void Update()
        {
            if ((Time.frameCount & 1) == 0)
                RayInfo();
        }

        private void RayInfo()
        {
            ray = fpsCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, crosshairUI.position));

#if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
#endif

            if (Physics.Raycast(ray, out hitWeapon, weaponRayDistance, weaponLayerForCrosshair, QueryTriggerInteraction.Ignore))
            {
                weapon.SetWeaponHit(hitWeapon.point);
                if (hitWeapon.transform.TryGetComponent<IDamageable>(out var obj))
                {
                    IsTD = true;
                    crosshairTarget.position = hitWeapon.point + ray.direction.normalized * 0.25f;
                    weapon.GunBarrelInfo(obj, hitWeapon.point);
                    isMatchedRay = true;
                    return;
                }
            }

            if (Physics.Raycast(ray, out hit, rayDistance, targetLayerForCrosshair, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.TryGetComponent<InteractableObj>(out var obj))
                {
                    isMatchedRay = false;

                    if (obj is ICanBeGrabbed grabbable)
                    {
                        IsTG = true;
                        grabController.RaycastInfo(grabbable);
                        isMatchedRay = true;
                    }

                    if (obj is ICanBeInteracted interactable)
                    {
                        IsTI = true;
                        interact.RaycastInfo(interactable);
                        isMatchedRay = true;
                    }

                    if (!isMatchedRay)
                        ResetRay(false);

                    return;
                }  
            }

            ResetRay(true);
        }

        private void ResetRay(bool isEmptySpace)
        {
            crosshairTarget.position = ray.GetPoint(defaultAimDistance);

            if(isEmptySpace)
                weapon.ClearWeaponTarget();

            if (isTD)
            {
                weapon.GunBarrelInfo(null);
                IsTD = false;
            }

            if (IsTG)
            {
                grabController.RaycastInfo(null);
                IsTG = false;
            }

            if (IsTI)
            {        
                interact.RaycastInfo(null);
                IsTI = false;
            }    
        }
    }
}