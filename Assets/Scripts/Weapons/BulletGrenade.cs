using System;
using UnityEngine;
using Player.WeaponData;

namespace Weapons
{
    public class BulletGrenade : Bullet
    {
        [SerializeField] private float itemThrowForce;
        [SerializeField] private int radius;
        [SerializeField] Rigidbody rb;
        [SerializeField] private LayerMask targetLayer;
        public Action<Vector3, int, int> OnGrenadeRayDamage;

        protected override void Start()
        {
            base.Start();
            OnGrenadeRayDamage += PlayerWeapon.OnRadiusHit;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnGrenadeRayDamage -= PlayerWeapon.OnRadiusHit;
        }

        protected override void OnDisable()
        {
            Vector3 pos = transform.position;
            OnWeaponHitEffect?.Invoke(pos, id);
            OnGrenadeRayDamage?.Invoke(pos, radius, id);
            rb.velocity = Vector3.zero;
            base.OnDisable();
        }

        protected virtual void OnCollisionEnter(UnityEngine.Collision collision)
        {
            if (targetLayer == (targetLayer | (1 << collision.gameObject.layer)))
            {
                rb.velocity = Vector3.zero;
                OnHitTarget(collision.gameObject);
            }
        }

        public void ThrowItem(Vector3 direction)
        {
            rb.AddForce(direction * itemThrowForce, ForceMode.Impulse);
        }

        public override void SetDirection(Vector3 dir, Vector3 dest, float speed)
        {

        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}