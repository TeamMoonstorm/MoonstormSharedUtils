using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Editor;
using UnityEditor;

namespace MSU.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(NullableRef<>), true)]
    public class NullableRefPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty _valueProperty = property.FindPropertyRelative("_value");

            using var _0 = new EditorGUI.PropertyScope(position, label, property);
            Rect rectForProperty = new Rect(position.x, position.y, position.width - 16, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rectForProperty, _valueProperty, label);

            Rect rectForLabel = new Rect(rectForProperty.xMax + 4, rectForProperty.y, 12, rectForProperty.height);
            EditorGUI.LabelField(rectForLabel, new GUIContent("?", "This field can be left null."), EditorStyles.boldLabel);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_value"), label);
        }
    }
}
