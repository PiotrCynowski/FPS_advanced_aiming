using System.Collections;
using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    [SerializeField] float openAngle = 90f;
    [SerializeField] float animationTime = 1f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine currentAnimation;
    private bool isOpen = false;
    private bool isAnimating = false;

    private void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    public void OpenDoor()
    {
        StartDoorAnimation(openRotation);
        isOpen = true;
    }

    public void CloseDoor()
    {
        StartDoorAnimation(closedRotation);
        isOpen = false;
    }

    private void StartDoorAnimation(Quaternion target)
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(RotateDoor(target));
    }

    private IEnumerator RotateDoor(Quaternion targetRotation)
    {
        Quaternion startRotation = transform.rotation;
        float duration = animationTime * Quaternion.Angle(startRotation, targetRotation) / openAngle;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        currentAnimation = null;
    }
}

public interface ICanBeDoorAnimated
{
    void OpenDoor();
    void CloseDoor();
    DoorBehaviour DoorAnim { get; }
}
