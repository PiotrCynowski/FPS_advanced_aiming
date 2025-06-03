using UnityEngine;

namespace Weapons
{
    public class BulletDefault : Bullet
    {
        public void Update()
        {
            transform.Translate(speed * Time.deltaTime * Vector3.forward);
        }
    }
}