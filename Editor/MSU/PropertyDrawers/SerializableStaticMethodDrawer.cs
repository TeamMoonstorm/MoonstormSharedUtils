using RoR2.Editor;
using UnityEditor;
using UnityEngine;

namespace MSU.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SerializableStaticMethod.RequiredArguments))]
    public class SerializableStaticMethodDrawer : IMGUIPropertyDrawer<SerializableStaticMethod.RequiredArguments>
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prefixRect = EditorGUI.PrefixLabel(position, label);
            if(EditorGUI.DropdownButton(prefixRect, CreateDropdownContent(property), FocusType.Passive))
            {
                var dropdown = new SerializableStaticMethodDropdown(new UnityEditor.IMGUI.Controls.AdvancedDropdownState(), propertyDrawerData.returnType, propertyDrawerData.arguments);
                dropdown.onItemSelected += (item) =>
                {
                    SerializedProperty assemblyQualifiedTypeNameProperty = property.FindPropertyRelative("assemblyQualifiedTypeName");
                    SerializedProperty methodNameProperty = property.FindPropertyRelative("methodName");

                    assemblyQualifiedTypeNameProperty.stringValue = item.assemblyQualifiedTypeName;
                    methodNameProperty.stringValue = item.methodName;

                    property.serializedObject.ApplyModifiedProperties();
                };
                dropdown.Show(position);
            }
        }

        private GUIContent CreateDropdownContent(SerializedProperty serializedProperty)
        {
            GUIContent result = new GUIContent();
            SerializedProperty assemblyQualifiedTypeNameProperty = serializedProperty.FindPropertyRelative("assemblyQualifiedTypeName");
            SerializedProperty methodNameProperty = serializedProperty.FindPropertyRelative("methodName");

            if(assemblyQualifiedTypeNameProperty.stringValue.IsNullOrEmptyOrWhiteSpace() || methodNameProperty.stringValue.IsNullOrEmptyOrWhiteSpace())
            {
                result.tooltip = "None";
            }
            else
            {
                result.tooltip = assemblyQualifiedTypeNameProperty.stringValue + "." + methodNameProperty.stringValue + "()";
            }

            if(methodNameProperty.stringValue.IsNullOrEmptyOrWhiteSpace())
            {
                result.text = "None";
            }
            else
            {
                result.text = methodNameProperty.stringValue + "()";
            }
            return result;
        }
    }
}