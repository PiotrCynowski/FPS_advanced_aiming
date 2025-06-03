using System;
using UnityEngine;
using PoolSpawner;
using System.Collections;

namespace Weapons
{
    public abstract class Bullet : MonoBehaviour, IPoolable<Bullet>
    {      
        public float speed;
        [SerializeField] private int lifeTime;
        [SerializeField] private LayerMask targetLayer;

        private Coroutine returnCoroutine;

        private Action<Bullet, int> returnToPool;
        private int id;
        [HideInInspector] public int damage;
        private Vector3 direction;

        protected void OnTriggerEnter(Collider other)
        {
            if (targetLayer == (targetLayer | (1 << other.gameObject.layer)))
            {
                OnHitTarget(other.gameObject);
            }
        }

        private void Update()
        {
            UpdateBullet();
        }

        public abstract void UpdateBullet();

        public void SetDirection(Vector3 dir)
        {
            direction = dir.normalized;
            transform.forward = direction;
        }

        protected void OnHitTarget(GameObject target)
        {
            if (target.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage, transform.position);
            }
            ReturnToPool();
        }

        #region pool
        public void Initialize(Action<Bullet, int> returnAction, int _id)
        {
            this.returnToPool = returnAction;
            id = _id;
        }

        public void ReturnToPool()
        {
            returnToPool?.Invoke(this, id);
        }

        private IEnumerator ReturnToPoolAfterDelay()
        {
            yield return new WaitForSeconds(lifeTime);
            ReturnToPool();
        }
        #endregion


        #region enable/disable
        private void OnEnable()
        {
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
            }
            returnCoroutine = StartCoroutine(ReturnToPoolAfterDelay());
        }

        private void OnDisable()
        {
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
            }
        }
        #endregion
    }
}