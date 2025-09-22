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
            if(EditorGUI.DropdownButton(position, label, FocusType.Passive))
            {
                var dropdown = new SerializableStaticMethodDropdown(new UnityEditor.IMGUI.Controls.AdvancedDropdownState());
                dropdown.Show(position);
            }
        }
    }
}