#if UNITY_EDITOR

using UnityEditor;
using Weapons;

[CustomEditor(typeof(Weapon))]
public class WeaponDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawPropertiesExcluding(serializedObject, "bulletTemplate", "onHitDelayMultiplayer", "shotInterval");

        ShotType weaponType = (ShotType)serializedObject.FindProperty("weaponType").enumValueIndex;
        RifleType rifleType = (RifleType)serializedObject.FindProperty("rifleType").enumValueIndex;

        if (weaponType == ShotType.obj || weaponType == ShotType.grenade)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletTemplate"));
        }

        if (weaponType == ShotType.ray)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onHitDelayMultiplayer"));
        }

        if(rifleType == RifleType.automatic)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shotInterval"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif