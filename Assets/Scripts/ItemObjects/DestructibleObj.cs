using UnityEngine;
using UnityEngine.Events;

namespace DestrObj
{
    public abstract class DestructibleObj : MonoBehaviour, IDamageable
    {
        public int currentHealth;
        public ObjectType thisObjMaterial;

        [SerializeField] UnityEvent onObjectDestroyed;


        public virtual void TakeDamage(int damage, Vector3 hitPos, Quaternion? hitRot)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                onObjectDestroyed.Invoke();
            }
        }

        public void DetachThisElement(Transform element)
        {
            element.parent = null;
        }
    }
}