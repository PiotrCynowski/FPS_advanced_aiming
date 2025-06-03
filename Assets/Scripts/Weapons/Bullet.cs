using System;
using UnityEngine;
using PoolSpawner;
using System.Collections;

namespace Weapons
{
    public abstract class Bullet : MonoBehaviour, IPoolable<Bullet>
    {      
        public float speed;
        public int id;
        [HideInInspector] public int damage;
        [SerializeField] private int lifeTime;
        [SerializeField] private LayerMask targetLayer;
        private Vector3 direction;
        private Coroutine returnCoroutine;

        public Action<Transform, int> OnWeaponHitEffect;
        private Action<Bullet, int> returnToPool;
    
        private void OnEnable()
        {
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
            }
            returnCoroutine = StartCoroutine(ReturnToPoolAfterDelay());
        }

        protected virtual void OnDisable()
        {
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
            }
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (targetLayer == (targetLayer | (1 << other.gameObject.layer)))
            {
                OnHitTarget(other.gameObject);
            }
        }

        public void SetDirection(Vector3 dir)
        {
            direction = dir.normalized;
            transform.forward = direction;
        }

        public void SetAction(Action<Transform, int> OnWeaponHitEffect)
        {
            this.OnWeaponHitEffect = OnWeaponHitEffect;
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
    }
}