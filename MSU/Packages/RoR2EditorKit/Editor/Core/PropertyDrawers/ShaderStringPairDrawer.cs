using RoR2EditorKit.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Core.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(MaterialEditorSettings.ShaderStringPair))]
    public sealed class ShaderStringPairPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var objRefProperty = property.FindPropertyRelative("shader");
            objRefProperty.objectReferenceValue = EditorGUI.ObjectField(position, ObjectNames.NicifyVariableName(property.FindPropertyRelative("shaderName").stringValue), objRefProperty.objectReferenceValue, typeof(Shader), false);
            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 16;
        }
    }
}
