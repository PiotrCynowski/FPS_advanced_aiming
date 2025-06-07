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

        private CrosshairRayTarget currentTarget = CrosshairRayTarget.None;
        public CrosshairRayTarget CurrentTarget
        {
            get { return currentTarget; }
            set
            {
                if (currentTarget != value)
                {
                    currentTarget = value;
                    OnRayTargetInfo?.Invoke(currentTarget);
                }
            }
        }

        public delegate void OnRayTarget(CrosshairRayTarget target);
        public static event OnRayTarget OnRayTargetInfo;

        private void Start()
        {
            OnRayTargetInfo?.Invoke(CrosshairRayTarget.None);
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
                            CurrentTarget = CrosshairRayTarget.Grabbable;
                            lastTargetObj = obj;
                            break;
                        case IDamageable damageable:
                            CurrentTarget = CrosshairRayTarget.Damageable;
                            crosshairTarget.position = hit.point + ray.direction.normalized * 0.25f;
                            weapon.GunBarrelInfo(damageable, hit.point);
                            break;

                        default:
                            ResetRay();
                            break;
                    }
                    return;
                }  
            }

            ResetRay();
        }

        private void ResetRay()
        {
            CurrentTarget = CrosshairRayTarget.None;
            weapon.GunBarrelInfo(null);
            crosshairTarget.position = ray.GetPoint(defaultAimDistance);
        }
    }
}

public enum CrosshairRayTarget { None, Damageable, Interactable, Grabbable }