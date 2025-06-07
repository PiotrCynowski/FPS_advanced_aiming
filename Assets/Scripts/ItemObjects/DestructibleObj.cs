using UnityEngine;
using UnityEngine.Events;

namespace DestrObj
{
    public abstract class DestructibleObj : InteractableObj, IDamageable
    {
        public int currentHealth;
        public TargetType thisObjMaterial;

        [SerializeField] UnityEvent onObjectDestroyed;

        public TargetType ObjectType => thisObjMaterial;

        public int CurrentHealth => currentHealth;

        public virtual void TakeDamage(int damage, Vector3 hitPos, Quaternion? hitRot, bool onHitEffect)
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