using Player.WeaponData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Weapons;

public class PlayerWeaponsMenuUI : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private PlayerWeaponUIElement contentElement;
    private Dictionary<int, RectTransform> items = new();
    private Coroutine menuRoutine;

    private void Start()
    {
        PlayerWeapon.Instance.OnWeaponIndexSwitch += CenterItem;
        PlayerWeapon.Instance.OnAddUIItem += AddItem;

        content.gameObject.SetActive(false);
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

        if(menuRoutine != null) StopCoroutine(menuRoutine);
        menuRoutine = StartCoroutine(WeaponsMenuRoutine());

        RectTransform target = items[key];
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        Vector3 itemCenterLocal = content.InverseTransformPoint(GetWorldCenter(target));
        Vector3 viewportCenterLocal = content.InverseTransformPoint(GetWorldCenter(viewport));

        float offsetY = itemCenterLocal.y - viewportCenterLocal.y;

        Vector2 newPos = content.anchoredPosition;
        newPos.y -= offsetY; 
        content.anchoredPosition = newPos;
    }

    private Vector3 GetWorldCenter(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return (corners[0] + corners[2]) * 0.5f;
    }

    private IEnumerator WeaponsMenuRoutine()
    {
        content.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        content.gameObject.SetActive(false);
    }
}
