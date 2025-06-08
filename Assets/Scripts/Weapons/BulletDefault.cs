using UnityEngine;

namespace Weapons
{
    public class BulletDefault : Bullet
    {
        public void FixedUpdate()
        {
            rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * transform.forward);
        }

        protected override void OnCollisionEnter(UnityEngine.Collision collision)
        {
            base.OnCollisionEnter(collision);
            OnWeaponHitEffect?.Invoke(transform.position, id);
        }
    }
}