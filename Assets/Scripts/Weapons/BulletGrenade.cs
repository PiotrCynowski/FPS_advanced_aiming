using System;
using UnityEngine;
using Player.WeaponData;

namespace Weapons
{
    public class BulletGrenade : Bullet
    {
        [SerializeField] private float itemThrowForce;      
        [SerializeField] private int radius;
        public Action<Vector3, int, int> OnGrenadeRayDamage;

        protected override void Start()
        {
            base.Start();
            OnGrenadeRayDamage += PlayerWeapon.OnRadiusHit; 
        }
       
        protected override void OnDisable()
        {
           Vector3 pos = transform.position;
           OnWeaponHitEffect?.Invoke(pos, id);
           OnGrenadeRayDamage?.Invoke(pos, radius, id);

           base.OnDisable();
        }

        private void OnDestroy()
        {
            OnGrenadeRayDamage -= PlayerWeapon.OnRadiusHit;
        }

        public void ThrowItem(Vector3 direction)
        {
            rb.AddForce(direction * itemThrowForce, ForceMode.Impulse);
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