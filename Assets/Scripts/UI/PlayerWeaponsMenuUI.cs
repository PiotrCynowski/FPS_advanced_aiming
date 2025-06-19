using Player.WeaponData;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Weapons;

public class PlayerWeaponsMenuUI : MonoBehaviour
{
    public RectTransform content;        
    public RectTransform viewport;       
    public Dictionary<int, RectTransform> items = new(); 

    public PlayerWeaponUIElement contentElement;

    private void Start()
    {
        PlayerWeapon.Instance.OnWeaponIndexSwitch += CenterItem;
        PlayerWeapon.Instance.OnAddUIItem += AddItem;
    }

    private void OnDestroy()
    {
        PlayerWeapon.Instance.OnWeaponIndexSwitch -= CenterItem;
        PlayerWeapon.Instance.OnAddUIItem -= AddItem;
    }

    public void AddItem(Weapon weapon)
    {
        PlayerWeaponUIElement weaponUI = Instantiate(contentElement, content);
        weaponUI.Init(weapon.objName);
        items.Add(weapon.index, weaponUI.GetRectTransform());
    }

    public void CenterItem(int key)
    {
        if (!items.ContainsKey(key)) return;

        RectTransform target = items[key];

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // Get item center in world space
        Vector3 itemCenterWorld = GetWorldCenter(target);
        Vector3 viewportCenterWorld = GetWorldCenter(viewport);

        // Convert both to local space of the content (same coordinate space)
        Vector3 itemCenterLocal = content.InverseTransformPoint(itemCenterWorld);
        Vector3 viewportCenterLocal = content.InverseTransformPoint(viewportCenterWorld);

        // Calculate the offset between item and viewport center
        float offsetY = itemCenterLocal.y - viewportCenterLocal.y;

        // Apply the offset to anchoredPosition
        Vector2 newPos = content.anchoredPosition;
        newPos.y -= offsetY; // content moves opposite to item offset
        content.anchoredPosition = newPos;
    }

    private Vector3 GetWorldCenter(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return (corners[0] + corners[2]) * 0.5f;
    }
}
