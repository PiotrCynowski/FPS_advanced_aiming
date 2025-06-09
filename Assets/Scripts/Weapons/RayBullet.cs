using PoolSpawner;
using System;
using System.Collections;
using UnityEngine;

namespace Weapons
{
    public class RayBullet : MonoBehaviour, IPoolable<RayBullet>
    {
        private Action<RayBullet, int> returnToPool;
        protected int id;
        private bool isReturned;
        private Vector3 startPos, direction, dest;
        private float lifeTime;
        private Coroutine returnCoroutine;
        private float elapsedTime;
        private bool isMoving = false;

        protected void OnEnable()
        {
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
            }
            isReturned = false;
            isMoving = true;
            startPos = transform.position;
            returnCoroutine = StartCoroutine(ReturnToPoolAfterDelay());
        }

        protected virtual void OnDisable()
        {
            elapsedTime = 0;
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
            }
        }

        void Update()
        {
            if (!isMoving) return;
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / lifeTime); 
            transform.position = Vector3.Lerp(startPos, dest, t);
            if (t >= 1f)
                isMoving = false; 
        }

        public void Initialize(Action<RayBullet, int> returnAction, int id)
        {
            this.returnToPool = returnAction; 
            this.id = id;
        }

        public void SetDirection(Vector3 dir, Vector3 dest, float lifeTime)
        {
            direction = dir.normalized;
            transform.forward = direction;
            this.dest = dest;
            this.lifeTime = lifeTime;
        }

        public void ReturnToPool()
        {
            isReturned = true;
            returnToPool?.Invoke(this, id);
        }

        private IEnumerator ReturnToPoolAfterDelay()
        {
            yield return new WaitForSeconds(lifeTime);
            if (!isReturned) ReturnToPool();
        }
    }
}