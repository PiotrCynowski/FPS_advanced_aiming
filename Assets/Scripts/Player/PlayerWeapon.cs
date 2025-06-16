using UnityEngine;
using PoolSpawner;
using Weapons;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Player.WeaponData
{
    public class PlayerWeapon : MonoBehaviour
    {
        [Header("Crosshair Settings")]
        [SerializeField] private Transform crosshairTarget;
        [SerializeField] private RectTransform crosshairUI;

        [Header("Weapon Settings")]
        [SerializeField] private Transform weaponsContainer;
        [SerializeField] private List<Weapon> possibleWeapons;
        private Dictionary<int, WeaponData> weaponsCollection;
        private GameObject currentWeapon;
        private Transform gunBarrel;

        private SpawnWithPool<Bullet> poolSpawner;
        private SpawnWithPool<PoolableOnHit> onHitEffectPoolSpawner;
        private PlayerWeaponInfo weaponInfo;
        private IDamageable lastTargetObj;
        private Vector3? lastTargetHitPos;
        private Quaternion? lastTargetHitRot;

        private ParticleSystem currentMuzzle;
        public int currentWeaponIndex, weaponsLen;
        public bool isSwitching, isAnim, isReadyToSwitch;

        private float defaultRayDistance;
        private int currentDamage, currentAmmo, currentMagazines, magazine;
        private Coroutine shootingRoutine, reloadRoutine;
        private TargetType currentTargMat = TargetType.None;
        private CrosshairTarget currentTarget = CrosshairTarget.None;

        public CrosshairTarget CurrentTarget
        {
            get { return currentTarget; }
            set
            {
                if (currentTarget != value)
                {
                    currentTarget = value;
                    OnWeaponObjTarget?.Invoke(currentTarget);
                }
            }
        }

        public delegate void OnObjTarget(CrosshairTarget target);
        public event OnObjTarget OnWeaponObjTarget;

        public Action<Transform, Action, Action> OnWeaponSwitch;
        public Action<int, int> OnAmmoChange;

        public static PlayerWeapon Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            poolSpawner = new();
            onHitEffectPoolSpawner = new();
            weaponInfo = new();

            PrepareWeapons();
        }

        public void GunBarrelInfo(Vector3? point = null, Vector3? direction = null, IDamageable target = null)
        {
            lastTargetHitPos = point.HasValue ? point.Value : null;
            lastTargetHitRot = direction.HasValue ? Quaternion.LookRotation(direction.Value) : null;
        
            if (target != null)
            {
                weaponInfo.ObjHP = target.CurrentHealth;

                if (currentTargMat == target.ObjectType && lastTargetObj == target)
                    return;

                lastTargetObj = target;
                currentTargMat = target.ObjectType;

                currentDamage = possibleWeapons[currentWeaponIndex].GetDamageInfo(currentTargMat);
                CurrentTarget = currentDamage > 0 ? CrosshairTarget.Destroy : CrosshairTarget.CantDestroy;

                weaponInfo.ObjMat = currentTargMat;
                return;
            }

            lastTargetObj = null;  

            if (currentTargMat == TargetType.None)
            {
                weaponInfo.ObjMat = TargetType.None;
                return;
            }

            currentTargMat = TargetType.None;
            CurrentTarget = CrosshairTarget.None;
        }

        #region Input
        public void ShotLMouseBut(bool isPerformed) // Aim at the 3D crosshair position
        {
            if(!isReadyToSwitch) return;

            if (!isPerformed)
            {
                if (shootingRoutine != null)
                {
                    StopCoroutine(shootingRoutine);
                    shootingRoutine = null;
                }
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
            if (!isReadyToSwitch && isSwitching && isAnim)
                return;

            isReadyToSwitch = false;
            isAnim = true;
            isSwitching = true;

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

            weaponInfo.CurrentWeaponMatInfo = possibleWeapons[currentWeaponIndex].GetMaterialInfo();
            currentDamage = possibleWeapons[currentWeaponIndex].GetDamageInfo(currentTargMat);
            magazine = weaponsCollection[currentWeaponIndex].magazine;

            StartCoroutine(WeaponModelSwitch(currentWeaponIndex));

            if (currentTargMat == TargetType.None)
            {
                CurrentTarget = CrosshairTarget.None;
                return;
            }

            CurrentTarget = currentDamage > 0 ? CrosshairTarget.Destroy : CrosshairTarget.CantDestroy;
        }
        #endregion

        private void PrepareWeapons()
        {
            weaponsLen = possibleWeapons.Count;
            weaponsCollection = new();

            for (int i = 0; i < possibleWeapons.Count; i++)
            {
                PrepareWeapon(i, possibleWeapons[i]);
            }

            currentWeaponIndex = 0;
            weaponInfo.CurrentWeaponMatInfo = possibleWeapons[currentWeaponIndex].GetMaterialInfo();
            magazine = weaponsCollection[currentWeaponIndex].magazine;

            StartCoroutine(WeaponModelSwitch(currentWeaponIndex));
        }

        private void PrepareWeapon(int index, Weapon weapon)
        {
            weapon.PrepareWeapon(index);

            GameObject weaponGO = Instantiate(weapon.weaponModel, weaponsContainer);
            weaponGO.transform.localPosition = weapon.weaponPos;
            GameObject gunBarrel = new("GunBarrel");
            gunBarrel.transform.parent = weaponGO.transform;
            gunBarrel.transform.localPosition = weapon.gunBarrelPos;

            ParticleSystem muzzle = null;
            if (weapon.muzzle != null)
                muzzle = Instantiate(weapon.muzzle, gunBarrel.transform);

            weaponsCollection.Add(index, new(weapon.objName, weaponGO, gunBarrel.transform, weapon.magazine, weapon.maxMagazines, muzzle));
            weaponGO.SetActive(false);

            if (weapon.bulletTemplate != null)
            {
                poolSpawner.AddPoolForGameObject(weapon.bulletTemplate, index);
            }

            if (weapon.weaponOnHit != null)
                onHitEffectPoolSpawner.AddPoolForGameObject(weapon.weaponOnHit, index);
        }

        private void MarkAnimInMiddle() => isAnim = false;
        private void MarkReadyToSwitch() => isSwitching = false;

        private IEnumerator WeaponModelSwitch(int currentWeaponIndex)
        {
            if (weaponsCollection.TryGetValue(currentWeaponIndex, out WeaponData data))
            {
                gunBarrel = data.barrel;

                currentAmmo = data.currentAmmo;
                currentMagazines = data.currentMagazines;

                currentMuzzle = data.muzzle;

                OnAmmoChange?.Invoke(currentAmmo, currentMagazines * magazine);

                OnWeaponSwitch?.Invoke(data.modelRef.transform, MarkAnimInMiddle, MarkReadyToSwitch);

                yield return new WaitUntil(() => !isAnim);
   
                if (currentWeapon != null) currentWeapon.SetActive(false);
                data.modelRef.SetActive(true);
                currentWeapon = data.modelRef;

                yield return null;

                isReadyToSwitch = true;
            }
        }

        #region Callbacks
        public void OnHitWeaponAction(Vector3? pos, int weaponId)
        {
            if (pos.HasValue && onHitEffectPoolSpawner.IsContainID(weaponId))
                    onHitEffectPoolSpawner.GetSpawnObject(pos.Value, weaponId);
        }

        public void OnRadiusHitAction(Vector3 position, int radius, int Id)
        {
            Collider[] colliders = Physics.OverlapSphere(position, radius);

            foreach (Collider nearbyObject in colliders)
            {
                if (nearbyObject.TryGetComponent<IDamageable>(out var damageable))
                    damageable.TakeDamage(possibleWeapons[Id].GetDamageInfo(damageable.ObjectType), position, onHitEffect: false);
            }
        }

        public void OnAddNewWeapon(Weapon weapon, bool isAmmo)
        {
            int index = weapon.GetIndex();

            if (!weaponsCollection.ContainsKey(index))
            {
                int newIndex = weaponsCollection.Keys.Count;
                weapon.PrepareWeapon(newIndex);
                PrepareWeapon(newIndex, weapon);
                StartCoroutine(WeaponModelSwitch(newIndex));
                weaponsLen = newIndex + 1;
                currentWeaponIndex = newIndex;
                possibleWeapons.Add(weapon);
            }
            else
                OnAddAmunition(index);
        }

        public void OnAddAmunition(int index)
        {
            Debug.Log($"OnAddAmunition index: {index}");

            if (currentWeaponIndex == index)
            {
                magazine = possibleWeapons[index].maxMagazines;
                OnAmmoChange?.Invoke(currentAmmo, currentMagazines * magazine);
            }

            weaponsCollection[index].magazine = possibleWeapons[index].maxMagazines;
        }
        #endregion

        #region Shooting
        public void PrepareDefaultRayDistanceShot(Vector3 pos)
        {
            defaultRayDistance = (transform.position - pos).sqrMagnitude;
        }

        private void Shot()
        {
            if (reloadRoutine != null)
                return;

            if (currentAmmo <= 0)
            {
                if (shootingRoutine != null)
                {
                    StopCoroutine(shootingRoutine);
                    shootingRoutine = null;
                    return;
                }

                if (currentMagazines > 0)
                    reloadRoutine = StartCoroutine(OnReload());
                return;
            }

            if (currentMuzzle != null)
                currentMuzzle.Play();

            switch (possibleWeapons[currentWeaponIndex].weaponType)
            {
                default:
                case ShotType.obj:
                    Bullet bullet = poolSpawner.GetSpawnObject(gunBarrel, currentWeaponIndex);
                    bullet.damage = currentDamage;
                    bullet.SetDirection((crosshairTarget.position - gunBarrel.position).normalized, Vector3.zero, 0);
                    break;
                case ShotType.distanceRay:
                    StartCoroutine(DelayedBulletHit());
                    break;
                case ShotType.bulletRay:
                    StartCoroutine(DelayedBulletRay());
                    break;
                case ShotType.ray:
                    lastTargetObj?.TakeDamage(currentDamage, lastTargetHitPos.Value, lastTargetHitRot, true);
                    if (possibleWeapons[currentWeaponIndex].weaponOnHit != null)
                        OnHitWeaponAction(lastTargetHitPos.Value, currentWeaponIndex);
                    break;
                case ShotType.grenade:
                    BulletGrenade grenade = poolSpawner.GetSpawnObject(gunBarrel, currentWeaponIndex) as BulletGrenade;
                    grenade.damage = currentDamage;
                    grenade.ThrowItem((crosshairTarget.position - gunBarrel.position).normalized);
                    break;
            }
            currentAmmo--;
            OnAmmoChange?.Invoke(currentAmmo, currentMagazines * magazine);
        }

        private IEnumerator DelayedShot(float interval)
        {
            while (true)
            {
                Shot();
                yield return new WaitForSeconds(interval);
            }
        }

        private IEnumerator DelayedBulletHit()
        {
            if (!lastTargetHitPos.HasValue || !lastTargetHitRot.HasValue)
                yield break;

            IDamageable target = lastTargetObj;
            Vector3 pos = lastTargetHitPos.Value;
            Quaternion rot = lastTargetHitRot.Value;
            yield return new WaitForSeconds((transform.position - pos).sqrMagnitude * possibleWeapons[currentWeaponIndex].onHitDelayMultiplayer);
            target?.TakeDamage(currentDamage, pos, rot, true);
            OnHitWeaponAction(pos, currentWeaponIndex);
            yield return null;
        }

        private IEnumerator DelayedBulletRay()
        {
            if (lastTargetHitRot.HasValue)
            {
                IDamageable target = lastTargetObj;
                Vector3 pos = lastTargetHitPos.Value;
                Quaternion rot = lastTargetHitRot.Value * Quaternion.Euler(0,180,0);

                float time = (transform.position - pos).sqrMagnitude * possibleWeapons[currentWeaponIndex].onHitDelayMultiplayer;

                Bullet bullet = poolSpawner.GetSpawnObject(gunBarrel, currentWeaponIndex);
                bullet.SetDirection((crosshairTarget.position - gunBarrel.position).normalized, pos, time);

                yield return new WaitForSeconds(time);

                target?.TakeDamage(currentDamage, pos, rot, true);

                if (possibleWeapons[currentWeaponIndex].weaponOnHit !=null)
                    OnHitWeaponAction(pos, currentWeaponIndex);
            }
            else
            {
                Bullet bullet = poolSpawner.GetSpawnObject(gunBarrel, currentWeaponIndex);
                bullet.SetDirection((crosshairTarget.position - gunBarrel.position).normalized, crosshairTarget.position, defaultRayDistance * possibleWeapons[currentWeaponIndex].onHitDelayMultiplayer);
            }
        }

        private IEnumerator OnReload()
        {
            yield return new WaitForSeconds(possibleWeapons[currentWeaponIndex].reloadTime);
            currentMagazines--;
            currentAmmo = magazine;
            reloadRoutine = null;
            OnAmmoChange?.Invoke(currentAmmo, currentMagazines * magazine);
        }
        #endregion
    }

    public class WeaponData
    {
        public string name;
        public GameObject modelRef;
        public Transform barrel;
        public ParticleSystem muzzle;
        public int currentAmmo, currentMagazines;
        public int magazine, maxMagazines;

        public WeaponData(string name, GameObject modelRef, Transform gunBarrel, int magazine, int maxMagazines, ParticleSystem muzzle = null)
        {
            this.name = name;
            this.modelRef = modelRef;
            this.barrel = gunBarrel;  
            this.currentAmmo = magazine;
            this.currentMagazines = 1;
            this.magazine = magazine;
            this.maxMagazines = maxMagazines;
            this.muzzle = muzzle;
        }
    }
}

#region enums
public enum TargetType { None, Iron, Wood, Conrete, Steel, EnergyField, Everything }
public enum ShotType { obj, ray, grenade, distanceRay, bulletRay }
public enum RifleType { single, automatic }
public enum CrosshairTarget { None, Destroy, CantDestroy }
#endregion

public interface IDamageable
{
    void TakeDamage(int amount, Vector3 pos, Quaternion? rot = null, bool onHitEffect = true);
    TargetType ObjectType { get; }
    int CurrentHealth { get; }
}