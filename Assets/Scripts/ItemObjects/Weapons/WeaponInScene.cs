using UnityEngine;
using Weapons;

public class WeaponInScene : InteractableObj, ICanBeInteracted
{
    public Weapon Weapon;

    public void OnInteracted()
    {
      
    }

    private void Start()
    {
        PositionWeapon();
    }

    private void PositionWeapon()
    {
        GameObject weapon = Instantiate(Weapon.weaponModel, this.transform);
        weapon.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        weapon.transform.localRotation = Quaternion.Euler(0, 90, 90);
    }
}