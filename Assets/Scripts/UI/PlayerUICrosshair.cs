using UnityEngine;
using Player;

public class PlayerUICrosshair : MonoBehaviour
{
    [SerializeField] private RectTransform crosshair;
    private Vector2 velocity;
    [SerializeField] private float smoothTime = 0.1f;

    [SerializeField] private float moveSwayAmount = 10f;
    [SerializeField] private float mouseSwayAmount = 5f;

    [SerializeField] private float jumpSwayAmount = 0.03f;
    [SerializeField] private float landBounceAmount = 0.05f;
    [SerializeField] private float impulseDecayTime = 0.2f;
    private Vector2 crosshairImpulse = Vector2.zero;
    private Vector2 crosshairImpulseVelocity = Vector2.zero;

    private Vector2 originalPosition;

    void Start()
    {
        originalPosition = crosshair.anchoredPosition;
        PlayerMovement.onJump += OnJumpOrLandCrosshair;
    }

    private void OnDestroy()
    {
        PlayerMovement.onJump -= OnJumpOrLandCrosshair;
    }

    public void UpdateCrosshair(Vector2 mouseInput, Vector2 keyboardInput)
    {
        Vector2 moveOffset = new Vector2(keyboardInput.x, 0f).normalized * moveSwayAmount;

        Vector2 mouseOffset = new Vector2(-mouseInput.x, mouseInput.y) * mouseSwayAmount;

        crosshairImpulse = Vector2.SmoothDamp(crosshairImpulse, Vector2.zero, ref crosshairImpulseVelocity, impulseDecayTime);

        Vector2 totalOffset = moveOffset + mouseOffset + crosshairImpulse;
        Vector2 targetPosition = originalPosition + totalOffset;

        crosshair.anchoredPosition = Vector2.SmoothDamp(crosshair.anchoredPosition, targetPosition, ref velocity, smoothTime);
    }

    public void OnJumpOrLandCrosshair(bool isJump)
    {
        crosshairImpulse = new Vector2(0f, isJump ? - jumpSwayAmount * 100f : landBounceAmount * 100f);
    }
}