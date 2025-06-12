using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class InteractableDoor : InteractableObj, ICanBeInteracted, ICanBeDoorAnimated
{
    [SerializeField] UnityEvent<bool> onInteraction;
    [SerializeField] bool isLocked;
    bool isOn = false;
    Coroutine interactRoutine;

    public void OnInteracted()
    {
        if (isLocked)
            return;

        if (interactRoutine != null) StopCoroutine(interactRoutine);
        isOn = !isOn;
        interactRoutine = StartCoroutine(InteractionRoutine(isOn));
    }

    public string IsConditionMet()
    {
        return isLocked ? "locked" : string.Empty;
    }

    IEnumerator InteractionRoutine(bool isOn)
    {
        onInteraction?.Invoke(isOn);

        if (isOn)
            OpenDoor();
        else
            CloseDoor();

        yield return null;
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