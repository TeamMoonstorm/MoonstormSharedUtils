using RoR2.Editor;
using UnityEditor;
using UnityEngine;
using static MSU.MaterialVariant;

namespace MSU.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SerializedMaterialProperty))]
    internal class SerializedMaterialPropertyDrawer : IMGUIPropertyDrawer<SerializedMaterialProperty>
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var shaderNameProp = property.FindPropertyRelative("propertyName");

            var foldoutHeader = shaderNameProp.stringValue.IsNullOrEmptyOrWhiteSpace() ? label : new GUIContent(shaderNameProp.stringValue);
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, foldoutHeader);
            if(property.isExpanded)
            {
                var pos = position;
                pos.y += standardPropertyHeight;

                var controlFieldRect = DrawSelectionField(EditorGUI.IndentedRect(pos), property);
            }
        }

        private Rect DrawSelectionField(Rect position, SerializedProperty property)
        {
            var shaderPropName = property.FindPropertyRelative("propertyName");
            var shaderPropertyNames = GetShaderPropertyNames((Material)property.serializedObject.FindProperty("originalMaterial").objectReferenceValue);
        }

        private string[] GetShaderPropertyNames(Material mat)
        {

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight(property, label);
            if(property.isExpanded)
            {
                height += standardPropertyHeight;
                var enumValueProperty = property.FindPropertyRelative("propertyType");
                var enumIndex = enumValueProperty.enumValueIndex;
                switch((UnityEditor.ShaderUtil.ShaderPropertyType)enumIndex)
                {
                    case UnityEditor.ShaderUtil.ShaderPropertyType.Int:
                    case UnityEditor.ShaderUtil.ShaderPropertyType.Color:
                    case UnityEditor.ShaderUtil.ShaderPropertyType.Float:
                    case UnityEditor.ShaderUtil.ShaderPropertyType.Vector:
                        height += standardPropertyHeight;
                        break;
                    case UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv:
                        height += standardPropertyHeight * 2;
                        break;
                }
            }
            return height;
        }
    }
}