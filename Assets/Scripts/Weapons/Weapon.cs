using System;
using UnityEngine;

namespace Weapons
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/NewWeapon")]
    public class Weapon : ScriptableObject
    {
        public ShotType weaponType;
        public WeaponCanDestroySetup[] canDestroy;
        public Bullet bulletTemplate;

        [Serializable]
        public class WeaponCanDestroySetup
        {
            public ObjectMaterials canDestroyMat;
            public int damagePerBullet;
        }

        public ObjectMaterials[] GetMaterialInfo()
        {
            ObjectMaterials[] canDestroyTheseMat = new ObjectMaterials[canDestroy.Length];

            for (int i = 0; i < canDestroy.Length; i++)
            {
                canDestroyTheseMat[i] = canDestroy[i].canDestroyMat;
            }

            return canDestroyTheseMat;
        }

        public int GetDamageInfo(ObjectMaterials objMat)
        {
            foreach (WeaponCanDestroySetup setup in canDestroy)
            {
                if (setup.canDestroyMat == objMat)
                {
                    return setup.damagePerBullet;
                }
            }
            return 0;
        }
    }
}