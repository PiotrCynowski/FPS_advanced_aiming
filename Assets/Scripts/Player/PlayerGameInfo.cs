using System;

public class PlayerGameInfo
{
    public static event Action<ObjectMaterials[]> OnPlayerWeaponChanged;
    public static event Action<ObjectMaterials, int> OnPlayerDestrObjChanged;

    private ObjectMaterials[] currentWeaponMatInfo;

    public ObjectMaterials[] CurrentWeaponMatInfo
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

    private ObjectMaterials objMat;

    public ObjectMaterials ObjMat
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