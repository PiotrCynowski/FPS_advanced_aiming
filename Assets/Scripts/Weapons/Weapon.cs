using System;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/NewWeapon")]
    public class Weapon : ScriptableObject
    {
        private int index;
        public string objName;
        public ShotType weaponType;
        public RifleType rifleType;
        public float shotInterval;
        public WeaponCanDestroySetup[] canDestroy;
        public Dictionary<TargetType, int> canDestroyDict;
        public Bullet bulletTemplate;
        public PoolableOnHit weaponOnHit;
        public ParticleSystem muzzle;
        public float onHitDelayMultiplayer;
        public GameObject weaponModel;
        public Vector3 gunBarrelPos, weaponPos;
        public int magazine, maxMagazines;
        public float reloadTime;

        [Serializable]
        public class WeaponCanDestroySetup
        {
            public TargetType canDestroyMat;
            public int damagePerBullet;
        }

        public TargetType[] GetMaterialInfo()
        {
            TargetType[] canDestroyTheseMat = new TargetType[canDestroy.Length];

            for (int i = 0; i < canDestroy.Length; i++)
            {
                canDestroyTheseMat[i] = canDestroy[i].canDestroyMat;
            }

            return canDestroyTheseMat;
        }

        public void PrepareWeapon(int index)
        {
            this.index = index;
            canDestroyDict = new Dictionary<TargetType, int>();
            foreach (WeaponCanDestroySetup setup in canDestroy)
            {
                canDestroyDict.Add(setup.canDestroyMat, setup.damagePerBullet);
            }
        }

        public int GetDamageInfo(TargetType objMat)
        {
            if (canDestroyDict.TryGetValue(objMat, out int value))
                return value;

            if(canDestroyDict.TryGetValue(TargetType.Everything, out int all))
                return all;

            return 0;
        }

        public int GetIndex() { return index; }
    }
}