using UnityEngine;
using PoolSpawner;
using Weapons;
using DestrObj;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        [Header("Crosshair Settings")]
        [SerializeField] private Transform crosshairTarget; 
        [SerializeField] private float defaultAimDistance = 10f;
        [SerializeField] private RectTransform crosshairUI;
        [SerializeField] private Camera fpsCamera;
        [SerializeField] private LayerMask targetLayerForCrosshair;

        [Header("Weapon Settings")]
        [SerializeField] private Transform gunBarrel, weaponsContainer;
        [SerializeField] private Weapon[] possibleWeapons;
        private Dictionary<int , WeaponData> weaponsCollection;
        private GameObject currentWeapon;

        private SpawnWithPool<Bullet> poolSpawner;
        private SpawnWithPool<PoolableOnHit> onHitEffectPoolSpawner;
        private PlayerGameInfo playerGameInfo;
        private DestructibleObj lastTargetObj;
        private Vector3? lastTargetHitPos;
        private Quaternion? lastTargetHitRot;

        private int currentWeaponIndex, weaponsLen;

        private int currentDamage, currentAmmo, currentMagazines, magazine;
        private Coroutine shootingRoutine, reloadRoutine;
        private ObjectType currentTargMat = ObjectType.None;
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

        public static Action<Vector3, int> OnHitEffect;
        public static Action<Vector3, int, int> OnRadiusHit;
        public static Action<Transform> OnWeaponSwitch;
        public static Action<int, int> OnAmmoChange;

        private void Awake()
        {
            OnHitEffect = OnHitWeaponAction;
            OnRadiusHit = OnRadiusHitAction;
        }

        private void Start()
        {
            poolSpawner = new();
            onHitEffectPoolSpawner = new();
            playerGameInfo = new();

            PrepareWeapons();
            OnDestObjTarget?.Invoke(CrosshairTarget.None);

            ray = fpsCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, crosshairUI.position));
            crosshairTarget.position = ray.GetPoint(defaultAimDistance);

        }

        private void Update()
        {
            if ((Time.frameCount & 1) == 0)
                GunBarrelInfo();
        }

        private void OnDestroy()
        {
         
            OnHitEffect = null;
            OnRadiusHit = null;
        }

        #region Input
        public void ShotLMouseBut(bool isPerformed) // Aim at the 3D crosshair position
        {
            if (!isPerformed)
            {
                if (shootingRoutine != null) StopCoroutine(shootingRoutine);
                shootingRoutine = null;
                return;
            }

            if (possibleWeapons[currentWeaponIndex].rifleType == RifleType.automatic)
            {
                if (shootingRoutine != null)
                {
                    StopCoroutine(shootingRoutine);
                    shootingRoutine = null;
                }
                shootingRoutine = StartCoroutine(DelayedShot(possibleWeapons[currentWeaponIndex].shotInterval));
            }
            else
                Shot();
        }

        public void SwitchWeaponMouseButScroll(bool isNext)
        {
            weaponsCollection[currentWeaponIndex].currentAmmo = currentAmmo;
            weaponsCollection[currentWeaponIndex].currentMagazines = currentMagazines;

            if (isNext)
                currentWeaponIndex++;
            else
                currentWeaponIndex--;

            if (currentWeaponIndex >= weaponsLen)
                currentWeaponIndex = 0;
            else if (currentWeaponIndex < 0)
                currentWeaponIndex = weaponsLen - 1;

            if (shootingRoutine != null)
            {
                StopCoroutine(shootingRoutine);
                shootingRoutine = null;
            }
            if (reloadRoutine != null) StopCoroutine(reloadRoutine);

            playerGameInfo.CurrentWeaponMatInfo = possibleWeapons[currentWeaponIndex].GetMaterialInfo();
            currentDamage = possibleWeapons[currentWeaponIndex].GetDamageInfo(currentTargMat);
            magazine = weaponsCollection[currentWeaponIndex].magazine;

            WeaponModelSwitch(currentWeaponIndex);

            if (currentTargMat == ObjectType.None)
            {
                CurrentTarget = CrosshairTarget.None;
                return;
            }

            CurrentTarget = currentDamage > 0 ? CrosshairTarget.Destroy : CrosshairTarget.CantDestroy;
        }
        #endregion

        private void PrepareWeapons()
        {
            weaponsLen = possibleWeapons.Length;
            weaponsCollection = new();

            for (int i = 0; i < weaponsLen; i++)
            {
                possibleWeapons[i].PrepareWeapon();

                GameObject weapon = Instantiate(possibleWeapons[i].weaponModel, weaponsContainer);
                weapon.transform.localPosition = possibleWeapons[i].weaponPos;
                GameObject gunBarrel = new("GunBarrel");
                gunBarrel.transform.parent = weapon.transform;
                gunBarrel.transform.localPosition = possibleWeapons[i].gunBarrelPos;
                weaponsCollection.Add(i, new(weapon, gunBarrel.transform, possibleWeapons[i].magazine, possibleWeapons[i].maxMagazines));
                weapon.SetActive(false);

                if (possibleWeapons[i].bulletTemplate != null)
                {
                    poolSpawner.AddPoolForGameObject(possibleWeapons[i].bulletTemplate, i);
                }

                if (possibleWeapons[i].weaponOnHit != null)
                    onHitEffectPoolSpawner.AddPoolForGameObject(possibleWeapons[i].weaponOnHit, i);
            }

            currentWeaponIndex = 0;
            playerGameInfo.CurrentWeaponMatInfo = possibleWeapons[currentWeaponIndex].GetMaterialInfo();
            magazine = weaponsCollection[currentWeaponIndex].magazine;

            WeaponModelSwitch(currentWeaponIndex);
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

                    crosshairTarget.position = hit.point + ray.direction.normalized * 0.25f;
                   
                    if (possibleWeapons[currentWeaponIndex].weaponType == ShotType.ray)
                    {
                        lastTargetHitPos = hit.point;
                        lastTargetHitRot = Quaternion.LookRotation(transform.position - hit.point);
                    }

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
            lastTargetHitPos = null;
            lastTargetHitRot = null;

            crosshairTarget.position = ray.GetPoint(defaultAimDistance);

            if (currentTargMat == ObjectType.None)
            {
                playerGameInfo.ObjMat = ObjectType.None;
                return;
            }

            currentTargMat = ObjectType.None;
            CurrentTarget = CrosshairTarget.None;
        }

        private IEnumerator DelayedBulletHit()
        {
            DestructibleObj target = lastTargetObj;
            Vector3 pos = lastTargetHitPos.Value;
            Quaternion rot = lastTargetHitRot.Value;
            yield return new WaitForSeconds((transform.position - pos).sqrMagnitude * possibleWeapons[currentWeaponIndex].onHitDelayMultiplayer);
            if (target != null)
                target.TakeDamage(currentDamage, pos, rot, true);
            yield return null;
        }

        private void OnHitWeaponAction(Vector3 pos, int weaponId)
        {
            onHitEffectPoolSpawner.GetSpawnObject(pos, weaponId);
        }

        private void OnRadiusHitAction(Vector3 position, int radius, int Id)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

            foreach (Collider nearbyObject in colliders)
            {
                if (nearbyObject.TryGetComponent<IDamageable>(out var damageable))
                    damageable.TakeDamage(possibleWeapons[Id].GetDamageInfo(damageable.ObjectType), transform.position, onHitEffect: false);
            }
        }

        private void WeaponModelSwitch(int currentWeaponIndex)
        {
            if (weaponsCollection.TryGetValue(currentWeaponIndex, out WeaponData data))
            {
                if(currentWeapon !=null) currentWeapon.SetActive(false);
                data.modelRef.SetActive(true);
                gunBarrel = data.barrel;          
                currentWeapon = data.modelRef;

                currentAmmo = data.currentAmmo;
                currentMagazines = data.currentMagazines;
            
                OnWeaponSwitch?.Invoke(data.modelRef.transform);

                OnAmmoChange?.Invoke(currentAmmo, currentMagazines * magazine);
            }
        }

        #region Shooting
        private IEnumerator DelayedShot(float interval)
        {
            while (true)
            {
                Shot();
                yield return new WaitForSeconds(interval);
            }
        }

        private void Shot()
        {
            if(reloadRoutine != null)
                return;

            if (currentAmmo <= 0)
            {
                if (shootingRoutine != null)
                {
                    StopCoroutine(shootingRoutine);
                    shootingRoutine = null;
                    return;
                }

                reloadRoutine = StartCoroutine(OnReload());
                return;
            }

            switch (possibleWeapons[currentWeaponIndex].weaponType)
            {
                default:
                case ShotType.obj:
                    Bullet bullet = poolSpawner.GetSpawnObject(gunBarrel, currentWeaponIndex);
                    bullet.damage = currentDamage;
                    bullet.SetDirection((crosshairTarget.position - gunBarrel.position).normalized);
                    break;
                case ShotType.ray:
                    if (lastTargetObj != null && lastTargetHitPos.HasValue)
                    {
                        if (possibleWeapons[currentWeaponIndex].onHitDelayMultiplayer > 0)
                            StartCoroutine(DelayedBulletHit());
                        else
                            lastTargetObj.TakeDamage(currentDamage, lastTargetHitPos.Value, lastTargetHitRot.Value, true);
                    }
                    break;
                case ShotType.grenade:
                    BulletGrenade grenate = poolSpawner.GetSpawnObject(gunBarrel, currentWeaponIndex) as BulletGrenade;
                    grenate.damage = currentDamage;
                    grenate.ThrowItem((crosshairTarget.position - gunBarrel.position).normalized);
                    break;
            }
            currentAmmo--;
            OnAmmoChange?.Invoke(currentAmmo, currentMagazines * magazine);
        }

        private IEnumerator OnReload()
        {
            if (currentMagazines > 0)
            {
                yield return new WaitForSeconds(possibleWeapons[currentWeaponIndex].reloadTime);
                currentMagazines--;
                currentAmmo = magazine;
                reloadRoutine = null;
                OnAmmoChange?.Invoke(currentAmmo, currentMagazines * magazine);
            }
        }
        #endregion
    }

    public class WeaponData
    {
        public GameObject modelRef;
        public Transform barrel;
        public int currentAmmo, currentMagazines;
        public int magazine, maxMagazines;

        public WeaponData(GameObject modelRef, Transform gunBarrel, int magazine, int maxMagazines)
        {
            this.modelRef = modelRef;
            this.barrel = gunBarrel;
            this.currentAmmo = magazine;
            this.currentMagazines = 1;
            this.magazine = magazine;
            this.maxMagazines = maxMagazines;
        }
    }
}

#region enums
public enum ObjectType { None, Iron, Wood, Conrete, Steel, EnergyField, Everything }

public enum ShotType { obj, ray, grenade }

public enum RifleType { single, automatic }

public enum CrosshairTarget { None, Destroy, CantDestroy }
#endregion

public interface IDamageable
{
    void TakeDamage(int amount, Vector3 pos, Quaternion? rot = null, bool onHitEffect = true);
    ObjectType ObjectType { get; }
}