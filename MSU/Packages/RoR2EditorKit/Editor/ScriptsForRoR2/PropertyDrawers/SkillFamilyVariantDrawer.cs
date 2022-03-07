using RoR2.Skills;
using RoR2EditorKit.Core.PropertyDrawers;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SkillFamily.Variant))]
    public class SkillFamilyVariantDrawer : IMGUIPropertyDrawer
    {
        protected override void DrawCustomDrawer()
        {
            Begin();
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, property.displayName);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                Rect skillRect = new Rect(rect);
                skillRect.height /= 3;
                skillRect.y += 16;
                EditorGUI.PropertyField(skillRect, property.FindPropertyRelative("skillDef"));

                Rect unlockableRect = new Rect(skillRect);
                unlockableRect.y += 16;
                EditorGUI.PropertyField(unlockableRect, property.FindPropertyRelative("unlockableDef"));
                EditorGUI.indentLevel--;
            }
            End();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
                return base.GetPropertyHeight(property, label) - 16f;
            return 16;
        }
    }
}