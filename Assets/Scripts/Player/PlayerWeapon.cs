using UnityEngine;
using PoolSpawner;
using Weapons;
using DestrObj;

namespace Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        [Header("3D Crosshair Settings")]
        [SerializeField] private Transform crosshairTarget; 
        [SerializeField] private float defaultAimDistance = 10f;
        [SerializeField] private RectTransform crosshairUI;
        [SerializeField] private Camera fpsCamera;
        [SerializeField] private LayerMask targetLayerForCrosshair;

        [Header("Weapon Settings")]
        [SerializeField] private Transform weaponGO;
        [SerializeField] private Transform gunBarrel;
        [SerializeField] private Weapon[] possibleWeapons;

        [Header("Sway Settings")]
        [SerializeField] private float moveSwayAmount = 0.03f;
        [SerializeField] private float mouseSwayAmount = 0.02f;
        [SerializeField] private float swaySmooth = 6f;

        private Vector3 initialLocalPos;
        private Vector3 targetOffset;

        private SpawnWithPool<Bullet> poolSpawner;
        private PlayerGameInfo playerGameInfo;
        private DestructibleObj lastTargetObj;

        private int currentWeaponIndex;
        private int weaponsLen;

        private int currentDamage;
        private ObjectMaterials currentTargMat = ObjectMaterials.None;
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

        private Ray ray;
        private RaycastHit hit;    
        private const int rayDistance = 25;

        public delegate void OnObjTarget(CrosshairTarget target);
        public static event OnObjTarget OnDestObjTarget;

        private void Start()
        {
            poolSpawner = new();
            playerGameInfo = new();

            PrepareWeapons();
            OnDestObjTarget?.Invoke(CrosshairTarget.None);

            initialLocalPos = weaponGO.localPosition;

            ray = fpsCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, crosshairUI.position));
            crosshairTarget.position = ray.GetPoint(defaultAimDistance);
        }

        private void Update()
        {
            if (Time.frameCount % 2 == 0) 
            {
                GunBarrelInfo();
            }

            weaponGO.localPosition = Vector3.Lerp(weaponGO.localPosition, initialLocalPos + targetOffset, Time.deltaTime * swaySmooth);
        }

        public void WeaponUpdate(Vector2 mouseInput, Vector2 movementInput)
        {
            Vector3 moveOffset = new Vector3(movementInput.x, 0, 0) * moveSwayAmount;
            Vector3 mouseOffset = new Vector3(-mouseInput.x, -mouseInput.y, 0f) * mouseSwayAmount;

            targetOffset = moveOffset + mouseOffset;
        }

        private void PrepareWeapons()
        {
            weaponsLen = possibleWeapons.Length;

            for (int i = 0; i < weaponsLen; i++)
            {
                poolSpawner.AddPoolForGameObject(possibleWeapons[i].bulletTemplate.gameObject, i);
            }

            currentWeaponIndex = 0;
            playerGameInfo.CurrentWeaponMatInfo = possibleWeapons[currentWeaponIndex].GetMaterialInfo();          
        }

        private void GunBarrelInfo()
        {
            ray = fpsCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, crosshairUI.position));

#if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
#endif

            if (Physics.Raycast(ray, out hit, rayDistance, targetLayerForCrosshair, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.TryGetComponent<DestructibleObj>(out var obj))
                {
                    playerGameInfo.ObjHP = obj.currentHealth;

                    if (currentTargMat == obj.thisObjMaterial && lastTargetObj == obj)
                    {
                        return;
                    }
                    lastTargetObj = obj;
                    currentTargMat = obj.thisObjMaterial;

                    currentDamage = possibleWeapons[currentWeaponIndex].GetDamageInfo(currentTargMat);
                    CurrentTarget = currentDamage > 0 ? CrosshairTarget.Destroy : CrosshairTarget.CantDestroy;

                    crosshairTarget.position = hit.point;

                    playerGameInfo.ObjMat = currentTargMat;
                  
                    return;
                }
            }

            lastTargetObj = null;          

            if (currentTargMat == ObjectMaterials.None)
            {
                playerGameInfo.ObjMat = ObjectMaterials.None;
                return;
            }

            crosshairTarget.position = ray.GetPoint(defaultAimDistance);

            currentTargMat = ObjectMaterials.None;
            CurrentTarget = CrosshairTarget.None;
        }

        #region Input
        public void ShotLMouseBut() // Aim at the 3D crosshair position
        {
            Bullet bullet = poolSpawner.GetSpawnObject(gunBarrel, currentWeaponIndex);
            bullet.damage = currentDamage;        
            bullet.SetDirection((crosshairTarget.position - gunBarrel.position).normalized);
        }

        public void SwitchWeaponRMouseBut()
        {
            currentWeaponIndex++;

            if (currentWeaponIndex >= weaponsLen)
            {
                currentWeaponIndex = 0;
            }

            playerGameInfo.CurrentWeaponMatInfo = possibleWeapons[currentWeaponIndex].GetMaterialInfo();
            currentDamage = possibleWeapons[currentWeaponIndex].GetDamageInfo(currentTargMat);

            if(currentTargMat == ObjectMaterials.None)
            {
                CurrentTarget = CrosshairTarget.None;
                return;
            }

            CurrentTarget = currentDamage > 0 ? CrosshairTarget.Destroy : CrosshairTarget.CantDestroy;
        }
        #endregion
    }
}

#region enums
public enum ObjectMaterials { None, Iron, Wood, Conrete, Steel, EnergyField }

public enum CrosshairTarget { None, Destroy, CantDestroy }
#endregion

public interface IDamageable
{
    void TakeDamage(int amount, Vector3 pos);
}