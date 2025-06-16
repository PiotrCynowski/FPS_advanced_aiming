using Player.WeaponData;
using UnityEngine;
using Weapons;

public class AmmoInteractable : TriggerObjects
{
    [SerializeField] private Weapon weapon;

    public void Start()
    {
        Instantiate(weapon.ammoModel, transform);
    }

    public override void OnTriggerEnter(Collider other)
    {
        PlayerWeapon.Instance.OnAddAmunition(weapon.GetIndex());

        base.OnTriggerEnter(other);
    }
}