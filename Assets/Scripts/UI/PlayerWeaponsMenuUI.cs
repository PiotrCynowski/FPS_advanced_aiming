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

        Vector3 localCenterPos = content.InverseTransformPoint(GetWorldCenter(target));
        Vector3 viewportLocalCenter = content.InverseTransformPoint(GetWorldCenter(viewport));

        float offsetY = localCenterPos.y - viewportLocalCenter.y;

        Vector2 newAnchoredPos = content.anchoredPosition;
        newAnchoredPos.y += offsetY;
        content.anchoredPosition = newAnchoredPos;
    }

    private Vector3 GetWorldCenter(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return (corners[0] + corners[2]) * 0.5f; 
    }
}
