using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2EditorKit.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Moonstorm.EditorUtils.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(VanillaSkinDefinition.MSGameObjectActivation))]
    public class MSGameObjectActivationPropertyDrawer : PropertyDrawer
    {
        PropertyField intField;
        PropertyField gameObjectPrefab;
        PropertyField childName;
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            ThunderKit.Core.UIElements.TemplateHelpers.GetTemplateInstance(GetType().Name, root, (path) =>
            {
                return path.StartsWith("Packages/teammoonstorm-moonstormsharedutils/Editor");
            });

            Foldout foldout = root.Q<Foldout>("FoldOut");
            foldout.text = property.displayName;

            SerializedProperty isCustomActivation = property.FindPropertyRelative("isCustomActivation");
            PropertyField toggle = foldout.Q<PropertyField>("isCustomActivation");
            toggle.bindingPath = isCustomActivation.propertyPath;
            toggle.RegisterCallback<ChangeEvent<bool>>(OnToggled);

            intField = foldout.Q<PropertyField>("rendererIndex");
            intField.bindingPath = property.FindPropertyRelative("rendererIndex").propertyPath;
            intField.SetDisplay(!isCustomActivation.boolValue);

            gameObjectPrefab = foldout.Q<PropertyField>("gameObjectPrefab");
            gameObjectPrefab.bindingPath = property.FindPropertyRelative("gameObjectPrefab").propertyPath;
            gameObjectPrefab.SetDisplay(isCustomActivation.boolValue);

            childName = foldout.Q<PropertyField>("childName");
            childName.bindingPath = property.FindPropertyRelative("childName").propertyPath;
            childName.SetDisplay(isCustomActivation.boolValue);

            toggle = foldout.Q<PropertyField>("shouldActivate");
            toggle.bindingPath = property.FindPropertyRelative("shouldActivate").propertyPath;

            return foldout;
        }

        private void OnToggled(ChangeEvent<bool> evt)
        {
            var isCustomActivation = evt.newValue;
            intField.SetDisplay(!isCustomActivation);
            gameObjectPrefab.SetDisplay(isCustomActivation);
            childName.SetDisplay(isCustomActivation);
        }
    }
}