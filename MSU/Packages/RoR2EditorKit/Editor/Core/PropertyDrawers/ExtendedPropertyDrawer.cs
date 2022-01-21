using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core.PropertyDrawers
{
    public abstract class ExtendedPropertyDrawer : PropertyDrawer
    {
        public Rect rect;
        public GUIContent label;
        public SerializedProperty property;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            rect = position;
            this.label = label;
            this.property = property;

            DrawCustomDrawer();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float num = 16;
            foreach (var prop in property)
            {
                num += 16;
            }
            return num;
        }

        public float GetDefaultPropertyHeight()
        {
            return base.GetPropertyHeight(property, label);
        }

        protected abstract void DrawCustomDrawer();
        protected GUIContent Begin() => EditorGUI.BeginProperty(rect, label, property);
        protected void End() => EditorGUI.EndProperty();
        protected string NicifyName(string name) => ObjectNames.NicifyVariableName(name);
    }
}
