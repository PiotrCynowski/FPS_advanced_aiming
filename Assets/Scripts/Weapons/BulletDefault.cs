using UnityEngine;

namespace Weapons
{
    public class BulletDefault : Bullet
    {
        public override void UpdateBullet()
        {
            transform.Translate(speed * Time.deltaTime * Vector3.forward);
        }
    }
}