using UnityEngine;

namespace Weapons
{
    public class BulletDefault : Bullet
    {
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] Rigidbody rb;
        private Vector3 direction;
     
        protected override void OnDisable()
        {
            base.OnDisable();
            rb.velocity = Vector3.zero;
        }

        protected void FixedUpdate()
        {
            rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * transform.forward);
        }

        protected virtual void OnCollisionEnter(UnityEngine.Collision collision)
        {
            if (targetLayer == (targetLayer | (1 << collision.gameObject.layer)))
            {
                rb.velocity = Vector3.zero;
                OnHitTarget(collision.gameObject);
            }
            OnWeaponHitEffect?.Invoke(transform.position, id);
        }

        public override void SetDirection(Vector3 dir, Vector3 dest, float speed)
        {
            direction = dir.normalized;
            transform.forward = direction;
        }
    }
}