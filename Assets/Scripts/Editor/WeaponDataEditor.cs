#if UNITY_EDITOR

using UnityEditor;
using Weapons;

[CustomEditor(typeof(Weapon))]
public class WeaponDataEditor : Editor
{
    private SerializedProperty _bulletTemplate, _weaponOnHit;

    private void OnEnable()
    {
        _bulletTemplate = serializedObject.FindProperty("bulletTemplate");
    }

    public override void OnInspectorGUI()
    {
        DrawPropertiesExcluding(serializedObject, "bulletTemplate" , "weaponOnHit");

        ShotType weaponType = (ShotType)serializedObject.FindProperty("weaponType").enumValueIndex;

        if (weaponType == ShotType.obj)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletTemplate"));
        }

        if (weaponType == ShotType.ray)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponOnHit"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif