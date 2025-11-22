using RoR2.Editor;
using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MSU.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SerializableStaticMethod.RequiredArguments))]
    public class SerializableStaticMethodDrawer : IMGUIPropertyDrawer<SerializableStaticMethod.RequiredArguments>
    {
        StringBuilder argumentsStringBuilder = new StringBuilder();
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
            argumentsStringBuilder.Clear();
            GUIContent result = new GUIContent();
            SerializedProperty assemblyQualifiedTypeNameProperty = serializedProperty.FindPropertyRelative("assemblyQualifiedTypeName");
            SerializedProperty methodNameProperty = serializedProperty.FindPropertyRelative("methodName");

            Type returnType = propertyDrawerData.returnType;
            Type[] arguments = propertyDrawerData.arguments;
            for (int i = 0; i < arguments.Length; i++)
            {
                argumentsStringBuilder.Append(arguments[i].Name);

                if (i != arguments.Length - 1)
                {
                    argumentsStringBuilder.Append(", ");
                }
            }

            if(assemblyQualifiedTypeNameProperty.stringValue.IsNullOrEmptyOrWhiteSpace() || methodNameProperty.stringValue.IsNullOrEmptyOrWhiteSpace())
            {
                result.tooltip = "None";
            }
            else
            {
                string typeName = assemblyQualifiedTypeNameProperty.stringValue.Substring(0, assemblyQualifiedTypeNameProperty.stringValue.IndexOf(","));

                result.tooltip = string.Format("{0} {1}.{2}({3})", returnType.Name, typeName, methodNameProperty.stringValue, argumentsStringBuilder);
            }

            if(methodNameProperty.stringValue.IsNullOrEmptyOrWhiteSpace())
            {
                result.text = "None";
            }
            else
            {
                result.text = string.Format("{0} {1}({2})", returnType.Name, methodNameProperty.stringValue, argumentsStringBuilder);
            }
            return result;
        }
    }
}