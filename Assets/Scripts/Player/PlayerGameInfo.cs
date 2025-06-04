using System;

public class PlayerGameInfo
{
    public static event Action<ObjectType[]> OnPlayerWeaponChanged;
    public static event Action<ObjectType, int> OnPlayerDestrObjChanged;

    private ObjectType[] currentWeaponMatInfo;

    public ObjectType[] CurrentWeaponMatInfo
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

    private ObjectType objMat;

    public ObjectType ObjMat
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

    public int ObjHP
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