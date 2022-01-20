using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core.PropertyDrawers
{
    /// <summary>
    /// Used for lazy creation of property drawer using editor gui layout instead of editor gui.
    ///<para>This shouldnt be used unless you want a very simple property drawer that doesnt need to be all specific</para>
    ///<para>May cause spamming in the unity editor console that's caused by using EditorGUILayout instead of EditorGUI.</para>
    /// </summary>
    public class EditorGUILayoutPropertyDrawer : PropertyDrawer
    {
        SerializedProperty serializedProperty;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            serializedProperty = property;
            EditorGUI.BeginProperty(position, label, property);
            DrawPropertyDrawer(property);
            EditorGUI.EndProperty();
        }

        protected virtual void DrawPropertyDrawer(SerializedProperty property) { }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return -2f;
        }

        protected void DrawField(string propName) => EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(propName), true);
        protected void DrawField(SerializedProperty property, string propName) => EditorGUILayout.PropertyField(property.FindPropertyRelative(propName), true);
        protected void DrawField(SerializedProperty property) => EditorGUILayout.PropertyField(property, true);
        protected void Header(string label) => EditorGUILayout.LabelField(new GUIContent(label), EditorStyles.boldLabel);
        protected void Header(string label, string tooltip) => EditorGUILayout.LabelField(new GUIContent(label, tooltip), EditorStyles.boldLabel);
    }
}
