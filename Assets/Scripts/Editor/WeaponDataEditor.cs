#if UNITY_EDITOR

using UnityEditor;
using Weapons;

[CustomEditor(typeof(Weapon))]
public class WeaponDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawPropertiesExcluding(serializedObject, "bulletTemplate", "onHitDelayMultiplayer");

        ShotType weaponType = (ShotType)serializedObject.FindProperty("weaponType").enumValueIndex;

        if (weaponType == ShotType.obj || weaponType == ShotType.grenade)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletTemplate"));
        }

        if (weaponType == ShotType.ray)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onHitDelayMultiplayer"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif