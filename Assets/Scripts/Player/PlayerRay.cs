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
        [SerializeField] PlayerGrabController grabbing;

        private Ray ray;
        private RaycastHit hit;
        private const int rayDistance = 25;

        private InteractableObj lastTargetObj;

        private CrosshairTarget currentTarget = CrosshairTarget.None;
        public CrosshairTarget CurrentTarget
        {
            get { return currentTarget; }
            set
            {
                if (currentTarget != value)
                {
                    currentTarget = value;
                    OnObjTargetInfo?.Invoke(currentTarget);
                }
            }
        }

        public delegate void OnObjTarget(CrosshairTarget target);
        public static event OnObjTarget OnObjTargetInfo;

        private void Start()
        {
            OnObjTargetInfo?.Invoke(CrosshairTarget.None);
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
                    switch (obj)
                    {
                        case ICanBeGrabbed grabbable:
                            if (obj == lastTargetObj)
                                return;
                            lastTargetObj = obj;
                            break;

                        case IDamageable damageable:
                            weapon.GunBarrelInfo(damageable, hit, ray.direction);
                            break;


                        default:
                            weapon.GunBarrelInfo(null);
                            break;
                    }
                }
                else
                {
                    weapon.GunBarrelInfo(null);
                }
                crosshairTarget.position = ray.GetPoint(defaultAimDistance);
            }
        }
    }
}

public enum CrosshairRayTarget { None, CanDestroy, Interactable, Grabbable }