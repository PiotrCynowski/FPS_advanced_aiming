using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    [SerializeField] float openAngle = 90f;
    [SerializeField] float animationTime = 1f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpen = false;
    private bool isAnimating = false;

    private void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    public void OpenDoor()
    {
        if (!isOpen && !isAnimating)
            StartCoroutine(RotateDoor(openRotation));
    }

    public void CloseDoor()
    {
        if (isOpen && !isAnimating)
            StartCoroutine(RotateDoor(closedRotation));
    }

    private System.Collections.IEnumerator RotateDoor(Quaternion targetRotation)
    {
        isAnimating = true;

        Quaternion startRotation = transform.rotation;
        float elapsed = 0f;

        while (elapsed < animationTime)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / animationTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        isOpen = (targetRotation == openRotation);
        isAnimating = false;
    }
}

public interface ICanBeDoorAnimated
{
    void OpenDoor();
    void CloseDoor();
    DoorBehaviour DoorAnim { get; }
}
