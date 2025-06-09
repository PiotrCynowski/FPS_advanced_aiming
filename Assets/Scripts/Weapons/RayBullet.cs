using UnityEngine;

namespace Weapons
{
    public class RayBullet : Bullet
    {
        private Vector3 startPos, direction, dest;
        private float elapsedTime;
        private bool isMoving = false;

        protected override void OnEnable()
        {
            base.OnEnable();

            isMoving = true;
            startPos = transform.position;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            elapsedTime = 0;
        }

        protected void Update()
        {
            if (!isMoving) return;
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / speed);
            transform.position = Vector3.Lerp(startPos, dest, t);
            if (t >= 1f)
            {
                isMoving = false;
                ReturnToPool();
            }
        }

        public override void SetDirection(Vector3 dir, Vector3 dest, float speed)
        {
            direction = dir.normalized;
            transform.forward = direction;
            this.dest = dest;
            this.speed = speed;
        }
    }
}