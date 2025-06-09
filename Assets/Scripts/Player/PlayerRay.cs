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

        #region Target Bool  
        private bool isTG; //Target Grabbable

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
            weapon.PrepareDefaultRayDistanceShot(crosshairTarget.position);
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
                if (hitWeapon.transform.TryGetComponent<IDamageable>(out var obj))
                {
                    crosshairTarget.position = hitWeapon.point + ray.direction.normalized * 0.25f;
                    weapon.GunBarrelInfo(hitWeapon.point, ray.direction, obj);
                    isMatchedRay = true;
                    return;
                }
                else
                {
                    weapon.GunBarrelInfo(hitWeapon.point, ray.direction);
                }
            }
            else
            {
                crosshairTarget.position = ray.GetPoint(defaultAimDistance);
                weapon.GunBarrelInfo(crosshairTarget.position); //look at air
            }

            if (Physics.Raycast(ray, out hit, rayDistance, targetLayerForCrosshair, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.TryGetComponent<InteractableObj>(out var obj))
                {
                    isMatchedRay = false;

                    if (obj is ICanBeGrabbed grabbable)
                    {
                        isTG = true;
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
                        ResetRay();

                    return;
                }  
            }

            ResetRay();
        }

        private void ResetRay()
        {
            if (isTG)
            {
                grabController.RaycastInfo(null);
                isTG = false;
            }

            if (IsTI)
            {        
                interact.RaycastInfo(null);
                IsTI = false;
            }    
        }
    }
}