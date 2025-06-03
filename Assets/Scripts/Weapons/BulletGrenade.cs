using UnityEngine;

namespace Weapons
{
    public class BulletGrenade : Bullet
    {
        [SerializeField] private float itemThrowForce;

        private Rigidbody rb;
        private Collider coll;

        public override void UpdateBullet()
        {
           
        }

        public void ThrowItem(Vector3 direction, float force)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }
}