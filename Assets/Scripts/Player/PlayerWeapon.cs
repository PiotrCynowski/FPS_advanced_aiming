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
        [SerializeField] private Transform weaponGO;
        [SerializeField] private Transform gunBarrel;
        [SerializeField] private Weapon[] possibleWeapons;
        [SerializeField] private float rotationSpeed = 5f;

        [Header("Sway Settings")]
        [SerializeField] private float moveSwayAmount = 0.03f;
        [SerializeField] private float mouseSwayAmount = 0.02f;
        [SerializeField] private float swaySmooth = 6f;

        [SerializeField] private float jumpSwayAmount = 0.03f;
        [SerializeField] private float landBounceAmount = 0.05f;
        [SerializeField] private float impulseDecayTime = 0.2f;
        private Vector3 swayImpulse = Vector3.zero;
        private Vector3 swayImpulseVelocity = Vector3.zero;

        [Header("Bobbing Settings")]
        [SerializeField] private float bobFrequency = 8f;
        [SerializeField] private float bobSprintFrequency = 15f;
        [SerializeField] private float bobAmplitude = 0.015f;
        [SerializeField] private float bobSpeedThreshold = 0.1f;
        [SerializeField] private float bobSwayLerp = 8f;
        private bool isGround = true, isSprint = false;

        private float bobTimer = 0f;
        private Vector3 bobOffset = Vector3.zero;

        private Vector3 initialLocalPos, targetOffset;

        private SpawnWithPool<Bullet> poolSpawner;
        private SpawnWithPool<ParticleSystem> onHitEffectPoolSpawner;
        private PlayerGameInfo playerGameInfo;
        private DestructibleObj lastTargetObj;
        private Vector3? lastTargetHitPos;
        private Quaternion? lastTargetHitRot;

        private int currentWeaponIndex, weaponsLen;

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

        public static Action<Vector3, int> OnHitEffect;

        private void Awake()
        {
            OnHitEffect = OnHitWeaponAction;
        }

        private void Start()
        {
            poolSpawner = new();
            onHitEffectPoolSpawner = new();
            playerGameInfo = new();

            PrepareWeapons();
            OnDestObjTarget?.Invoke(CrosshairTarget.None);

            initialLocalPos = weaponGO.localPosition;

            ray = fpsCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, crosshairUI.position));
            crosshairTarget.position = ray.GetPoint(defaultAimDistance);

            PlayerMovement.onJump += OnJumpOrLandAction;
        }

        private void Update()
        {
            if ((Time.frameCount & 1) == 0)
            {
                GunBarrelInfo();
            }

            weaponGO.localPosition = Vector3.Lerp(weaponGO.localPosition, initialLocalPos + targetOffset, Time.deltaTime * swaySmooth);

            Quaternion targetRotation = Quaternion.LookRotation(crosshairTarget.position - weaponGO.position);

            weaponGO.rotation = Quaternion.Slerp(
                weaponGO.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }

        private void OnDestroy()
        {
            PlayerMovement.onJump -= OnJumpOrLandAction;
            OnHitEffect = null;
        }

        public void WeaponUpdate(Vector2 mouseInput, Vector2 movementInput)
        {
            //SWAY
            Vector3 moveOffset = new Vector3(movementInput.x, 0, 0) * moveSwayAmount;
            Vector3 mouseOffset = new Vector3(-mouseInput.x, -mouseInput.y, 0f) * mouseSwayAmount;

            swayImpulse = Vector3.SmoothDamp(swayImpulse, Vector3.zero, ref swayImpulseVelocity, impulseDecayTime);

            //BOBBING
            float playerSpeed = new Vector2(movementInput.x, movementInput.y).magnitude;
            if (playerSpeed > bobSpeedThreshold && isGround)
            {
                bobTimer += Time.deltaTime * (!isSprint ? bobFrequency : bobSprintFrequency);
                float bobY = Mathf.Sin(bobTimer) * bobAmplitude;
                bobOffset = new Vector3(0f, bobY, 0f);
            }
            else
            {
                bobTimer = 0f;
                bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, Time.deltaTime * bobSwayLerp);
            }

            targetOffset = moveOffset + mouseOffset + swayImpulse + bobOffset;
        }

        public void OnWeaponSprint(bool isPlayerSprint)
        {
            isSprint = isPlayerSprint;
        }

        #region Input
        public void ShotLMouseBut() // Aim at the 3D crosshair position
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
                            lastTargetObj.TakeDamage(currentDamage, lastTargetHitPos.Value, lastTargetHitRot.Value);
                    }
                    break;
                case ShotType.grenade:
                    BulletGrenade grenate = poolSpawner.GetSpawnObject(gunBarrel, currentWeaponIndex) as BulletGrenade;
                    grenate.damage = currentDamage;
                    grenate.ThrowItem((crosshairTarget.position - gunBarrel.position).normalized);
                    break;
            }
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

            if (currentTargMat == ObjectMaterials.None)
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

            if (currentTargMat == ObjectMaterials.None)
            {
                playerGameInfo.ObjMat = ObjectMaterials.None;
                return;
            }

            currentTargMat = ObjectMaterials.None;
            CurrentTarget = CrosshairTarget.None;
        }

        private void OnJumpOrLandAction(bool isJump)
        {
            isGround = isJump;
            swayImpulse = new Vector3(0f, isJump ? -jumpSwayAmount : landBounceAmount, 0f);
        }

        public void OnHitWeaponAction(Vector3 pos, int weaponId)
        {
            Debug.Log("OnHitWeaponAction" + pos + " " + weaponId);
            onHitEffectPoolSpawner.GetSpawnObject(pos, weaponId).Play();
        }

        private IEnumerator DelayedBulletHit()
        {
            DestructibleObj target = lastTargetObj;
            Vector3 pos = lastTargetHitPos.Value;
            Quaternion rot = lastTargetHitRot.Value;
            yield return new WaitForSeconds((transform.position - pos).sqrMagnitude * possibleWeapons[currentWeaponIndex].onHitDelayMultiplayer);
            if (target != null)
                target.TakeDamage(currentDamage, pos, rot);

            //if (possibleWeapons[currentWeaponIndex].weaponOnHit != null)
            //{
            // Instantiate(possibleWeapons[currentWeaponIndex].weaponOnHit); //TODO
            //}

            yield return null;
        }
    }
}

#region enums
public enum ObjectMaterials { None, Iron, Wood, Conrete, Steel, EnergyField }

public enum ShotType { obj, ray, grenade }

public enum CrosshairTarget { None, Destroy, CantDestroy }
#endregion

public interface IDamageable
{
    void TakeDamage(int amount, Vector3 pos, Quaternion? rot = null);
}