using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : InteractableObj, ICanBeInteracted
{
    [SerializeField] UnityEvent onInteraction;

    public string IsConditionMet()
    {
        return string.Empty;
    }

    public void OnInteracted()
    {
        onInteraction?.Invoke();
    }
}