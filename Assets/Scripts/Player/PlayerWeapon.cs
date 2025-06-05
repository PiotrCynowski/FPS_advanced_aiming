using UnityEngine;
using PoolSpawner;
using Weapons;
using DestrObj;
using System.Collections;
using System;

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
        [SerializeField] private Transform gunBarrel;
        [SerializeField] private Weapon[] possibleWeapons;

        private SpawnWithPool<Bullet> poolSpawner;
        private SpawnWithPool<PoolableOnHit> onHitEffectPoolSpawner;
        private PlayerGameInfo playerGameInfo;
        private DestructibleObj lastTargetObj;
        private Vector3? lastTargetHitPos;
        private Quaternion? lastTargetHitRot;

        private int currentWeaponIndex, weaponsLen;

        private int currentDamage;
        private Coroutine shootingRoutine;
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
            if (!isPerformed && (shootingRoutine != null))
            {
                StopCoroutine(shootingRoutine);
                return;
            }

            if (possibleWeapons[currentWeaponIndex].rifleType == RifleType.automatic)
            {
                if(shootingRoutine != null) StopCoroutine(shootingRoutine);
                shootingRoutine = StartCoroutine(DelayedShot(possibleWeapons[currentWeaponIndex].shotInterval));
            }
            else
                Shot();
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

            for (int i = 0; i < weaponsLen; i++)
            {
                possibleWeapons[i].PrepareWeapon();

                if (possibleWeapons[i].bulletTemplate != null)
                {
                    poolSpawner.AddPoolForGameObject(possibleWeapons[i].bulletTemplate, i);
                }

                if (possibleWeapons[i].weaponOnHit != null)
                    onHitEffectPoolSpawner.AddPoolForGameObject(possibleWeapons[i].weaponOnHit, i);
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

        private void OnHitWeaponAction(Vector3 pos, int weaponId)
        {
            onHitEffectPoolSpawner.GetSpawnObject(pos, weaponId);
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
                Shot();
                yield return new WaitForSeconds(interval);
            }
        }

        private void Shot()
        {
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
        }
        #endregion
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