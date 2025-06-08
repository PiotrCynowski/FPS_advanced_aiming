using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : InteractableObj, ICanBeInteracted
{
    [SerializeField] UnityEvent onInteraction;

    public void OnInteracted()
    {
        onInteraction?.Invoke();
    }
}