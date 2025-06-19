using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponUIElement : MonoBehaviour
{
    [SerializeField] Text weaponName;

    public void Init(string name)
    {
        weaponName.text = name;
    }

    public RectTransform GetRectTransform()
    {
        return GetComponent<RectTransform>();
    }
}
