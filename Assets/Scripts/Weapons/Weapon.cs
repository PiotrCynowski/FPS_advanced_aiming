using System;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/NewWeapon")]
    public class Weapon : ScriptableObject
    {
        public ShotType weaponType;
        public WeaponCanDestroySetup[] canDestroy;
        public Dictionary<ObjectType, int> canDestroyDict;
        public Bullet bulletTemplate;
        public PoolableOnHit weaponOnHit;
        public ParticleSystem muzzle;
        public float onHitDelayMultiplayer;

        [Serializable]
        public class WeaponCanDestroySetup
        {
            public ObjectType canDestroyMat;
            public int damagePerBullet;
        }

        public ObjectType[] GetMaterialInfo()
        {
            ObjectType[] canDestroyTheseMat = new ObjectType[canDestroy.Length];

            for (int i = 0; i < canDestroy.Length; i++)
            {
                canDestroyTheseMat[i] = canDestroy[i].canDestroyMat;
            }

            return canDestroyTheseMat;
        }

        public void PrepareWeapon()
        {
            canDestroyDict = new Dictionary<ObjectType, int>();
            foreach (WeaponCanDestroySetup setup in canDestroy)
            {
                canDestroyDict.Add(setup.canDestroyMat, setup.damagePerBullet);
            }
        }

        public int GetDamageInfo(ObjectType objMat)
        {
            if (canDestroyDict.TryGetValue(objMat, out int value))
                return value;

            if(canDestroyDict.TryGetValue(ObjectType.Everything, out int all))
                return all;

            return 0;
        }
    }
}