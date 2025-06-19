using Player.WeaponData;
using UnityEngine;
using UnityEngine.UI;

public class WeaponsMenuUI : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float stepSize = 0.1f; 

    private void Start()
    {
        PlayerWeapon.Instance.OnWeaponUISwitch += OnScroll;
    }

    private void OnDestroy()
    {
        PlayerWeapon.Instance.OnWeaponUISwitch -= OnScroll;
    }

    public void OnScroll(bool isScrollDown)
    {
        float direction = isScrollDown ? 1f : -1f;

        if (scrollRect.vertical)
        {
            float newPos = scrollRect.verticalNormalizedPosition + direction * stepSize;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(newPos);
        }
        else if (scrollRect.horizontal)
        {
            float newPos = scrollRect.horizontalNormalizedPosition + direction * stepSize;
            scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(newPos);
        }
    }
}
