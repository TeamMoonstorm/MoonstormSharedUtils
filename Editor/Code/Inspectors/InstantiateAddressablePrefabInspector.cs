using Moonstorm.Components.Addressables;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(InstantiateAddressablePrefab))]
    public class InstantiateAddressablePrefabInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.DelayedTextField(serializedObject.FindProperty("address"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ((InstantiateAddressablePrefab)target).Refresh();
            }

            EditorGUI.BeginChangeCheck();
            SerializedProperty posAndRotTo0 = serializedObject.FindProperty("setPositionAndRotationToZero");
            posAndRotTo0.boolValue = EditorGUILayout.Toggle(new GUIContent(posAndRotTo0.displayName, posAndRotTo0.tooltip), posAndRotTo0.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                var targetType = (InstantiateAddressablePrefab)target;
                if (targetType.Instance)
                {
                    Transform t = targetType.Instance.transform;
                    t.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                }
            }
            DrawPropertiesExcluding(serializedObject, "address", "setPositionAndRotationToZero", "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}