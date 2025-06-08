using UnityEngine;
using Player.WeaponData;

namespace Player
{
    public class PlayerRay : MonoBehaviour
    {
        [SerializeField] private Transform crosshairTarget;

        [SerializeField] private RectTransform crosshairUI;
        [SerializeField] private Camera fpsCamera;
        [SerializeField] private LayerMask targetLayerForCrosshair;
        [SerializeField] private float defaultAimDistance = 10f;

        [SerializeField] PlayerWeapon weapon;
        [SerializeField] PlayerGrabController grabController;

        private bool isMatchedRay;

        private Ray ray;
        private RaycastHit hit;
        private const int rayDistance = 25;

        private InteractableObj lastTargetObj;

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

            if (Physics.Raycast(ray, out hit, rayDistance, targetLayerForCrosshair, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.TryGetComponent<InteractableObj>(out var obj))
                {
                    isMatchedRay = false;

                    if (obj is ICanBeGrabbed grabbable)
                    {
                        if (obj != lastTargetObj)
                        {
                            IsTG = true;
                            lastTargetObj = obj;
                            grabController.RaycastInfo(grabbable);
                        }
                        isMatchedRay = true;
                    }

                    if (obj is IDamageable damageable)
                    {
                        IsTD = true;
                        crosshairTarget.position = hit.point + ray.direction.normalized * 0.25f;
                        weapon.GunBarrelInfo(damageable, hit.point);
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
            crosshairTarget.position = ray.GetPoint(defaultAimDistance);

            if (isTD)
            {
                Debug.Log("Reset");
                weapon.GunBarrelInfo(null);
                IsTD = false;
            }
           
        }
    }
}