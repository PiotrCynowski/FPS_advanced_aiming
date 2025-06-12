using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : InteractableObj, ICanBeInteracted
{
    [SerializeField] UnityEvent onInteraction;

    public bool IsConditionMet()
    {
        return true;
    }

    public void OnInteracted()
    {
        onInteraction?.Invoke();
    }
}