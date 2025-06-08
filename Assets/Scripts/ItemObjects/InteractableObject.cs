using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : InteractableObj, ICanBeInteracted, ICanBeDoorAnimated
{
    [SerializeField] UnityEvent onInteraction;

    public bool IsDoorOpen()
    {
        throw new System.NotImplementedException();
    }

    public void OnInteracted()
    {
        onInteraction?.Invoke();
    }

    public void SwitchAnimOpenCloseDoor()
    {
        throw new System.NotImplementedException();
    }
}