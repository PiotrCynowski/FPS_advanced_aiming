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
        [SerializeField] private Weapon[] possibleWeapons;
        private Dictionary<int, WeaponData> weaponsCollection;
        private GameObject currentWeapon;
        private Transform gunBarrel;

        private SpawnWithPool<Bullet> poolSpawner;
        private SpawnWithPool<PoolableOnHit> onHitEffectPoolSpawner;
        private PlayerWeaponInfo weaponInfo;
        private IDamageable lastTargetObj;
        private Vector3? lastTargetHitPos;
        private Quaternion? lastTargetHitRot;

        private int currentWeaponIndex, weaponsLen;

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
        public static event OnObjTarget OnWeaponObjTarget;

        public static Action<Vector3?, int> OnHitEffect;
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
            weaponInfo = new();

            PrepareWeapons();
        }

        private void OnDestroy()
        {

            OnHitEffect = null;
            OnRadiusHit = null;
        }

        public void GunBarrelInfo(IDamageable target, Vector3? point = null)
        {
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

        public void ClearWeaponTarget()
        {
            lastTargetHitPos = null;
            lastTargetHitRot = null;
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
            {
                UpdateRayTransforms();
                Shot();
            }
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

            weaponInfo.CurrentWeaponMatInfo = possibleWeapons[currentWeaponIndex].GetMaterialInfo();
            currentDamage = possibleWeapons[currentWeaponIndex].GetDamageInfo(currentTargMat);
            magazine = weaponsCollection[currentWeaponIndex].magazine;

            WeaponModelSwitch(currentWeaponIndex);

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
            weaponInfo.CurrentWeaponMatInfo = possibleWeapons[currentWeaponIndex].GetMaterialInfo();
            magazine = weaponsCollection[currentWeaponIndex].magazine;

            WeaponModelSwitch(currentWeaponIndex);
        }

        private void WeaponModelSwitch(int currentWeaponIndex)
        {
            if (weaponsCollection.TryGetValue(currentWeaponIndex, out WeaponData data))
            {
                if (currentWeapon != null) currentWeapon.SetActive(false);
                data.modelRef.SetActive(true);
                gunBarrel = data.barrel;
                currentWeapon = data.modelRef;

                currentAmmo = data.currentAmmo;
                currentMagazines = data.currentMagazines;

                OnWeaponSwitch?.Invoke(data.modelRef.transform);

                OnAmmoChange?.Invoke(currentAmmo, currentMagazines * magazine);
            }
        }

        private IEnumerator DelayedBulletHit()
        {
            IDamageable target = lastTargetObj;
            Vector3 pos = lastTargetHitPos.Value;
            Quaternion rot = lastTargetHitRot.Value;
            yield return new WaitForSeconds((transform.position - pos).sqrMagnitude * possibleWeapons[currentWeaponIndex].onHitDelayMultiplayer);
            if (target != null)
            {
                target.TakeDamage(currentDamage, pos, rot, true);
            }

            if(lastTargetHitPos.HasValue)
                OnHitWeaponAction(lastTargetHitPos.Value, currentWeaponIndex);
            yield return null;
        }

        private void OnHitWeaponAction(Vector3? pos, int weaponId)
        {
            if (possibleWeapons[currentWeaponIndex].weaponOnHit != null && pos.HasValue)
                onHitEffectPoolSpawner.GetSpawnObject(pos.Value, weaponId);
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

        #region Shooting
        private IEnumerator DelayedShot(float interval)
        {
            while (true)
            {
                UpdateRayTransforms();
                Shot();
                yield return new WaitForSeconds(interval);
            }
        }

        private void UpdateRayTransforms()
        {
            if (possibleWeapons[currentWeaponIndex].weaponType == ShotType.ray)
            {
                GameObject hit = new("hit");

                Vector3 hitPoint = PlayerRay.hitPoint;
                hit.transform.position = hitPoint;
                lastTargetHitPos = hitPoint;
                lastTargetHitRot = Quaternion.LookRotation(transform.position - hitPoint);
            }
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
                case ShotType.rayBullet:

                    break;
                case ShotType.ray:
                    if (lastTargetObj != null)
                    {
                        if (possibleWeapons[currentWeaponIndex].onHitDelayMultiplayer > 0)
                            StartCoroutine(DelayedBulletHit());
                        else
                        {
                            lastTargetObj.TakeDamage(currentDamage, lastTargetHitPos.Value, lastTargetHitRot, true);
                            OnHitWeaponAction(lastTargetHitPos.Value, currentWeaponIndex);
                        }
                        return;
                    }
                    if (possibleWeapons[currentWeaponIndex].onHitDelayMultiplayer > 0)
                        StartCoroutine(DelayedBulletHit());
                    else
                        OnHitWeaponAction(lastTargetHitPos.Value, currentWeaponIndex);
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
public enum TargetType { None, Iron, Wood, Conrete, Steel, EnergyField, Everything }

public enum ShotType { obj, ray, grenade, rayBullet }

public enum RifleType { single, automatic }

public enum CrosshairTarget { None, Destroy, CantDestroy }
#endregion

public interface IDamageable
{
    void TakeDamage(int amount, Vector3 pos, Quaternion? rot = null, bool onHitEffect = true);
    TargetType ObjectType { get; }
    int CurrentHealth { get; }
}