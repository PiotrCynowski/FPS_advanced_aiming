using System;

namespace Player.WeaponData
{
    public class PlayerWeaponInfo
    {
        public static event Action<TargetType[]> OnPlayerWeaponChanged;
        public static event Action<TargetType, int> OnPlayerDestrObjChanged;

        private TargetType[] currentWeaponMatInfo;

        internal TargetType[] CurrentWeaponMatInfo
        {
            get { return currentWeaponMatInfo; }
            set
            {
                if (currentWeaponMatInfo != value)
                {
                    currentWeaponMatInfo = value;
                    OnPlayerWeaponChanged?.Invoke(currentWeaponMatInfo);
                }
            }
        }

        private TargetType objMat;

        internal TargetType ObjMat
        {
            get { return objMat; }
            set
            {
                if (objMat != value)
                {
                    objMat = value;
                    OnPlayerDestrObjChanged?.Invoke(objMat, objHP);
                }
            }
        }

        private int objHP;

        internal int ObjHP
        {
            get { return objHP; }
            set
            {
                if (objHP != value)
                {
                    objHP = value;
                    OnPlayerDestrObjChanged?.Invoke(objMat, objHP);
                }
            }
        }
    }
}