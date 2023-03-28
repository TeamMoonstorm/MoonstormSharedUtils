using R2API;
using RoR2EditorKit;
using RoR2EditorKit.VisualElements;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(EventDirectorCategorySelection))]
    public sealed class EventDirectorCategorySelectionInspector : MSScriptableObjectInspector<EventDirectorCategorySelection>, IObjectNameConvention
    {
        TextField stageNameField;

        public string Prefix => "edcs";

        public bool UsesTokenForPrefix => false;

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

            var container = new HelpBox("The EventCards for categories cannot be added in the Editor\nThey get added at runtime instead by the EventCatalog using the EventCard's Settings", MessageType.Info, true);
            RootVisualElement.Add(container);
            container.BringToFront();
        }

        private void OnStageSet(ChangeEvent<string> evt)
        {
            string val = evt.newValue.Replace(" ", "");
            if (Enum.TryParse<DirectorAPI.Stage>(val, out DirectorAPI.Stage result))
            {
                stageNameField.style.display = result == DirectorAPI.Stage.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public PrefixData GetPrefixData()
        {
            return new PrefixData(() =>
            {
                var origName = TargetType.name;
                TargetType.name = Prefix + origName;
                AssetDatabaseUtils.UpdateNameOfObject(TargetType);
            });
        }
    }
}
