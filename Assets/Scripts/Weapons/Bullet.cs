using System;
using UnityEngine;
using PoolSpawner;
using System.Collections;

namespace Weapons
{
    public class Bullet : MonoBehaviour, IPoolable<Bullet>
    {      
        [SerializeField] private float speed;
        [SerializeField] private int lifeTime;
        [SerializeField] private LayerMask targetLayer;

        private Coroutine returnCoroutine;

        private Action<Bullet, int> returnToPool;
        private int id;
        [HideInInspector] public int damage;


        protected void OnTriggerEnter(Collider other)
        {
            if (targetLayer == (targetLayer | (1 << other.gameObject.layer)))
            {
                OnHitTarget(other.gameObject);
            }
        }

        private void Update()
        {
            transform.Translate(speed * Time.deltaTime * Vector3.forward);
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