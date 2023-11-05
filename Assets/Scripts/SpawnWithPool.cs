using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace PoolSpawner
{
    public class SpawnWithPool<T> where T : Component
    {
        private readonly bool collectionChecks = true;
        private readonly int maxPoolSize = 20;

        private Transform elementsContainer;
        private Transform startPosition;

        private readonly Dictionary<int, ObjectPool<T>> poolObjList = new();


        public void AddPoolForGameObject(GameObject toSpawn, int id)
        {
            if (elementsContainer == null)
            {
                elementsContainer = new GameObject().GetComponent<Transform>();
                elementsContainer.name = "PoolElementsContainer";
            }

            ObjectPool<T> pool = new(() =>
            {
                var obj = GameObject.Instantiate(toSpawn, Vector3.zero, Quaternion.identity, elementsContainer);

                if (obj.GetComponent<IPoolable<T>>() != null)
                {
                    obj.GetComponent<IPoolable<T>>().Initialize(ThisObjReleased, id);
                    return obj.GetComponent<T>();
                }

                return null;
            },
            OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize);

            poolObjList.Add(id, pool);
        }

        public void Spawn(Transform _pos, int _id)
        {
            startPosition = _pos;
            poolObjList[_id].Get();
        }

        public T GetSpawnObject(Transform pos, int id)
        {
            startPosition = pos;
            return poolObjList[id].Get();
        }


        private void ThisObjReleased(T obj, int id)
        {
            poolObjList[id].Release(obj);
        }


        #region poolOperations
        private void OnReturnedToPool(T system)
        {         
            system.gameObject.SetActive(false);
        }

        private void OnTakeFromPool(T system)
        {
            system.transform.SetPositionAndRotation(startPosition.position, startPosition.rotation);
            system.gameObject.SetActive(true);
        }

        // If the pool capacity is reached then any items returned will be destroyed.
        private void OnDestroyPoolObject(T system)
        {
            GameObject.Destroy(system.gameObject);
        }
        #endregion  
    }


    public interface IPoolable<T> where T : Component
    {
        void Initialize(System.Action<T, int> returnAction, int id);
        void ReturnToPool();
    }
}