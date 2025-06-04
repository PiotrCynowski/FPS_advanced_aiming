using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    public class BulletGrenade : Bullet
    {
        [SerializeField] private float itemThrowForce;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float radius;

        private Dictionary<ObjectType, int> grenadeDamage = new Dictionary<ObjectType, int>();

        public void ThrowItem(Vector3 direction)
        {
            rb.AddForce(direction * itemThrowForce, ForceMode.Impulse);
        }

        protected override void OnDisable()
        {
           Vector3 pos = transform.position;
           OnWeaponHitEffect?.Invoke(pos, id);

           rb.velocity = Vector3.zero;
           OnGrenadeRayDamage();
           base.OnDisable();
        }

        private void OnGrenadeRayDamage()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

            foreach (Collider nearbyObject in colliders)
            {
                if (nearbyObject.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(GetDamageInfo(damageable.ObjectType), transform.position);
                }
            }
        }

        public int GetDamageInfo(ObjectType objMat)
        {
            if (grenadeDamage.TryGetValue(objMat, out int value))
                return value;

            if (grenadeDamage.TryGetValue(ObjectType.Everything, out int all))
                return all;

            return 0;
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