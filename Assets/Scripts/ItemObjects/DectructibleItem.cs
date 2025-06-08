using UnityEngine;

public class DesctructibleItem : GrabbableItem, IDamageable
{
    [SerializeField] TargetType type;
    [SerializeField] int health;

    public TargetType ObjectType => type;

    public int CurrentHealth => health;

    public void TakeDamage(int amount, Vector3 pos, Quaternion? rot = null, bool onHitEffect = true)
    {
        health -= amount;

        if (health <= 0)
        {
            Destroy(gameObject);
        }     
    }
}