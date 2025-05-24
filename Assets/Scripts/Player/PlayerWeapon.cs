using UnityEngine;
using PoolSpawner;
using Weapons;
using DestrObj;
using System;

namespace Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private Transform gunBarrel;
        [SerializeField] private Weapon[] possibleWeapons;

        [Header("Crosshair Settings")]
        [SerializeField] private RectTransform crosshairUI;
        [SerializeField] private Camera fpsCamera;
        [SerializeField] private LayerMask targetLayerForCrosshair;

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
        }

        private void Update()
        {
            if (Time.frameCount % 2 == 0) 
            {
                GunBarrelInfo();
            }
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
            Vector2 crosshairScreenPos = crosshairUI.position;
            Ray uiRay = fpsCamera.ScreenPointToRay(crosshairScreenPos);

            ray = new Ray(gunBarrel.position, uiRay.direction);

#if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
#endif

            if (Physics.Raycast(ray, out hit, rayDistance, targetLayerForCrosshair))
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

            currentTargMat = ObjectMaterials.None;
            CurrentTarget = CrosshairTarget.None;
        }

        #region Input
        public void ShotLMouseBut()
        {
            if (poolSpawner == null || gunBarrel == null) return;

            //if (muzzleFlash != null)
            //{
            //    muzzleFlash.Play();
            //}

            Ray uiRay = fpsCamera.ScreenPointToRay(crosshairUI.position);

            Bullet bullet = poolSpawner.GetSpawnObject(gunBarrel, currentWeaponIndex);
            bullet.damage = currentDamage;

            bullet.transform.position = gunBarrel.position + uiRay.direction;

            Debug.DrawLine(gunBarrel.position, bullet.transform.position, Color.green, 1f);
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
