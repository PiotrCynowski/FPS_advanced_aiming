using UnityEngine;

namespace Weapons
{
    public class BulletGrenade : Bullet
    {
        [SerializeField] private float itemThrowForce;
        [SerializeField] private Rigidbody rb;

        public void ThrowItem(Vector3 direction)
        {
            rb.AddForce(direction * itemThrowForce, ForceMode.Impulse);
        }

        protected override void OnDisable()
        {
            Vector3 pos = transform.position;

           OnWeaponHitEffect?.Invoke(pos, id);
           rb.velocity = Vector3.zero;
           base.OnDisable();
        }
    }
}