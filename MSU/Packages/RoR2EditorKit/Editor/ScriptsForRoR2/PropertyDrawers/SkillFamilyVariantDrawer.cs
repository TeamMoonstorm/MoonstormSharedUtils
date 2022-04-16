using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using RoR2EditorKit.Core.PropertyDrawers;
using RoR2.Skills;
using RoR2;

namespace RoR2EditorKit.RoR2Related.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SkillFamily.Variant))]
    public sealed class SkillFamilyVariantDrawer : PropertyDrawer
    {
        PropertyField skillDefField;
        PropertyField unlockableDefField;
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.name = "Container";

            SerializedProperty skillDefProp = property.FindPropertyRelative("skillDef");
            skillDefField = new PropertyField(skillDefProp);
            skillDefField.name = skillDefProp.name;
            skillDefField.tooltip = CreateTooltip<SkillDef>(skillDefProp.objectReferenceValue);
            skillDefField.RegisterCallback<ChangeEvent<SkillDef>>(OnSkillSet);
            container.Add(skillDefField);

            SerializedProperty unlockDefProp = property.FindPropertyRelative("unlockableDef");
            unlockableDefField = new PropertyField(unlockDefProp);
            unlockableDefField.name = unlockDefProp.name;
            unlockableDefField.tooltip = CreateTooltip<UnlockableDef>(unlockDefProp.objectReferenceValue);
            unlockableDefField.RegisterCallback<ChangeEvent<UnlockableDef>>(OnUnlockSet);
            container.Add(unlockableDefField);
            return container;
        }

        private void OnSkillSet(ChangeEvent<SkillDef> evt)
        {
            skillDefField.tooltip = CreateTooltip<SkillDef>(evt.newValue);
        }

        private void OnUnlockSet(ChangeEvent<UnlockableDef> evt)
        {
            unlockableDefField.tooltip = CreateTooltip<UnlockableDef>(evt.newValue);
        }

        private string CreateTooltip<T>(Object obj) where T : Object
        {
            if (obj == null)
                return $"{typeof(T).Name}: Null\nType: Null";
            else
                return $"{typeof(T).Name}: {obj.name}\nType: {obj.GetType().Name}";
        }
    }
}