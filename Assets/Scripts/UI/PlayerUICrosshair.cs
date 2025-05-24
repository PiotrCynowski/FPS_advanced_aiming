using UnityEngine;

public class PlayerUICrosshair : MonoBehaviour
{
    [SerializeField] private RectTransform crosshair;
    private Vector2 velocity;
    [SerializeField] private float smoothTime = 0.1f;

    [SerializeField] private float moveSwayAmount = 10f;
    [SerializeField] private float mouseSwayAmount = 5f;
    [SerializeField] private float returnSpeed = 5f;

    private Vector2 originalPosition;

    void Start()
    {
        originalPosition = crosshair.anchoredPosition;
    }

    public void UpdateCrosshair(Vector2 mouseInput, Vector2 keyboardInput)
    {
        float horizontalMove = keyboardInput.x;
        float verticalMove = 0;

        Vector2 moveOffset = new Vector2(horizontalMove, verticalMove).normalized * moveSwayAmount;

        float mouseX = mouseInput.x;
        float mouseY = mouseInput.y;

        Vector2 mouseOffset = new Vector2(mouseX, mouseY) * mouseSwayAmount;
        Vector2 totalOffset = moveOffset + mouseOffset;
        Vector2 targetPosition = originalPosition + totalOffset;

        crosshair.anchoredPosition = Vector2.SmoothDamp(crosshair.anchoredPosition, targetPosition, ref velocity, smoothTime);
    }
}