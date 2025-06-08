using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : InteractableObj, ICanBeInteracted, ICanBeDoorAnimated
{
    [SerializeField] UnityEvent<bool> onInteraction;
    bool isOn = false;

    public void OnInteracted()
    {
        onInteraction?.Invoke(isOn);

        isOn = !isOn;

        if (isOn)
            OpenDoor();
        else 
            CloseDoor();
    }

    public void OpenDoor()
    {
        DoorAnim.OpenDoor();
    }

    public void CloseDoor()
    {
        DoorAnim?.CloseDoor();
    }

    public DoorBehaviour DoorAnim => GetComponent<DoorBehaviour>();
}