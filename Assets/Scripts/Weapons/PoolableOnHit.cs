using PoolSpawner;
using System;
using UnityEngine;

namespace Weapons
{
    public class PoolableOnHit : MonoBehaviour, IPoolable<PoolableOnHit>
    {
        private Action<PoolableOnHit, int> returnToPool;
        public int id;

        public void OnParticleSystemStopped()
        {
            ReturnToPool();
        }

        public void Initialize(Action<PoolableOnHit, int> returnAction, int id)
        {
            returnToPool = returnAction;
            this.id = id; 
        }

        public void ReturnToPool()
        {
            returnToPool?.Invoke(this, id);
        }
    }
}
