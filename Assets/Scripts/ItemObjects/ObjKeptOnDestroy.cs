using UnityEngine;

namespace DestrObj
{
    public class ObjKeptOnDestroy : DestructibleObj
    {
        [SerializeField] private ParticleSystem onHitParticles;


        public override void TakeDamage(int damage, Vector3 hitPos, Quaternion? hitRot)
        {
            base.TakeDamage(damage, hitPos, hitRot);

            if (damage != 0)
            {
                onHitParticles.transform.position = hitPos;
                onHitParticles.Play();
            }
        }
    }
}
