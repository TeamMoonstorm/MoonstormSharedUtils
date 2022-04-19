using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2EditorKit.Core.Inspectors;
using UnityEditor;
using Moonstorm;
using UnityEditor.UIElements;
using System;
using UnityEngine.UIElements;
using R2API;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(EventDirectorCategorySelection))]
    public sealed class EventDirectorCategorySelectionInspector : MSScriptableObjectInspector<EventDirectorCategorySelection>
    {
        TextField stageNameField;

        protected override string Prefix => null;

        protected override bool PrefixUsesTokenPrefix => false;

        protected override bool HasVisualTreeAsset => false;

        protected override void DrawInspectorGUI()
        {
            EnumField enumField = new EnumField("Tied Stage", R2API.DirectorAPI.Stage.Custom);
            enumField.name = $"StageEnum";
            enumField.tooltip = $"The stage that this Category Selection is tied to, if set to custom, it'll read from the \"stageName\" field.";
            enumField.BindProperty(serializedObject.FindProperty(nameof(EventDirectorCategorySelection.stage)));
            enumField.RegisterCallback<ChangeEvent<string>>(OnStageSet);
            DrawInspectorElement.Add(enumField);

            stageNameField = new TextField("Stage Name");
            stageNameField.tooltip = $"When the \"Tied Stage\" enum is set to \"Custom\", this value is used instead.";
            stageNameField.name = "StageName";
            stageNameField.BindProperty(serializedObject.FindProperty(nameof(EventDirectorCategorySelection.stageName)));
            stageNameField.style.display = TargetType.stage == DirectorAPI.Stage.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            DrawInspectorElement.Add(stageNameField);

            PropertyField category = new PropertyField(serializedObject.FindProperty(nameof(EventDirectorCategorySelection.categories)), "Event Categories");
            category.tooltip = $"The Event categories for this selection.";
            DrawInspectorElement.Add(category);

            DrawInspectorElement.Add(new IMGUIContainer(() => EditorGUILayout.HelpBox($"The EventCards for categories cannot be added in the editor\nThey get added at runtime instead by the EventCatalog", MessageType.Info)));
        }

        private void OnStageSet(ChangeEvent<string> evt)
        {
            string val = evt.newValue.Replace(" ", "");
            if(Enum.TryParse<DirectorAPI.Stage>(val, out DirectorAPI.Stage result))
            {
                stageNameField.style.display = result == DirectorAPI.Stage.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
