using UnityEngine;

public class TriggerObjects : MonoBehaviour
{
    public virtual void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
