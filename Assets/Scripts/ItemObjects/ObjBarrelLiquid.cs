using UnityEngine;

namespace DestrObj
{
    public class ObjBarrelLiquid : DestructibleObj
    {
        [SerializeField] private ParticleSystem onHitParticles;

        private Vector3 onHitDirection;
        private Quaternion hitRotation;

        public override void TakeDamage(int damage, Vector3 hitPos, Quaternion? hitRot, bool onHitEffect = true)
        {
            base.TakeDamage(damage, hitPos, hitRot, onHitEffect);

            if (currentHealth <= 0)
            {
                Destroy(gameObject);
                return;
            }

            if (onHitEffect && damage != 0)
            {
                onHitDirection = hitPos - transform.position;

                if (!hitRot.HasValue)
                {
                    hitRotation = Quaternion.Euler(
                        0f,
                        (Mathf.Atan2(onHitDirection.x, onHitDirection.z) * Mathf.Rad2Deg),
                        0f);
                }
                ParticleSystem liquidParticles = Instantiate(onHitParticles, hitPos, hitRot ?? hitRotation);
                liquidParticles.transform.parent = transform;

                liquidParticles.Play();
            }  
        }
    }
}
