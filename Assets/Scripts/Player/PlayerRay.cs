using DestrObj;
using UnityEngine;

namespace Player
{
    public class PlayerRay : MonoBehaviour
    {
        [SerializeField] private RectTransform crosshairUI;
        [SerializeField] private Camera fpsCamera;
        [SerializeField] private LayerMask targetLayerForCrosshair;

        [SerializeField] PlayerWeapon weapon;

        private Ray ray;
        private RaycastHit hit;
        private const int rayDistance = 25;

        private CrosshairTarget currentTarget = CrosshairTarget.None;
        public CrosshairTarget CurrentTarget
        {
            get { return currentTarget; }
            set
            {
                if (currentTarget != value)
                {
                    currentTarget = value;
                    OnDestObjTarget?.Invoke(currentTarget);
                }
            }
        }


        public delegate void OnObjTarget(CrosshairTarget target);
        public static event OnObjTarget OnDestObjTarget;


        private void Start()
        {
            ray = fpsCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, crosshairUI.position));

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
                if (hit.transform.TryGetComponent<DestructibleObj>(out var obj))
                {
                  //  weapon.GunBarrelInfo(obj);
                }
                else
                {
                   // weapon.GunBarrelInfo(null);
                }
            }
        }
    }
}