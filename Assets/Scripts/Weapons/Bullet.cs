using System;
using UnityEngine;
using PoolSpawner;
using System.Collections;
using Player.WeaponData;

namespace Weapons
{
    public abstract class Bullet : MonoBehaviour, IPoolable<Bullet>
    {
        public float speed;
        protected int id;

        [HideInInspector] public int damage;
        [SerializeField] private int lifeTime;

        private Coroutine returnCoroutine;
        private bool isReturned;

        public Action<Vector3?, int> OnWeaponHitEffect;
        private Action<Bullet, int> returnToPool;

        protected virtual void Start()
        {
            OnWeaponHitEffect += PlayerWeapon.OnHitEffect;
        }

        protected virtual void OnEnable()
        {
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
            }
            isReturned = false;
            returnCoroutine = StartCoroutine(ReturnToPoolAfterDelay());
        }

        protected virtual void OnDisable()
        {
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
            }
        }

        protected virtual void OnDestroy()
        {
            OnWeaponHitEffect -= PlayerWeapon.OnHitEffect;
        }

        protected void OnHitTarget(GameObject target)
        {
            if (target.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage, transform.position);
            }
            ReturnToPool();
        }

        public abstract void SetDirection(Vector3 dir, Vector3 dest, float speed);

        #region pool
        public void Initialize(Action<Bullet, int> returnAction, int _id)
        {
            this.returnToPool = returnAction;
            id = _id;
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
        #endregion
    }
}