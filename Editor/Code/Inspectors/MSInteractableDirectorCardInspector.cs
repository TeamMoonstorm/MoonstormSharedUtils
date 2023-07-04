using RoR2EditorKit;
using RoR2EditorKit.Inspectors;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(MSInteractableDirectorCard))]
    public class MSInteractableDirectorCardInspector : ExtendedInspector<MSInteractableDirectorCard>
    {
        VisualElement inspectorData;
        PropertyField customCategory;
        PropertyField customCategoryWeight;

        EnumFlagsField stagesField;
        PropertyField customStages;
        protected override void OnEnable()
        {
            base.OnEnable();
            OnVisualTreeCopy += () =>
            {
                inspectorData = DrawInspectorElement.Q<VisualElement>("InspectorDataContainer");
                customCategory = inspectorData.Q<PropertyField>("customCategory");
                customCategoryWeight = inspectorData.Q<PropertyField>("customCategoryWeight");
                customStages = inspectorData.Q<PropertyField>("customStages");
            };
        }
        protected override bool ValidateUXMLPath(string path)
        {
            return path.StartsWith("Packages/teammoonstorm-moonstormsharedutils/Editor");
        }
        protected override void DrawInspectorGUI()
        {
            inspectorData.Q<PropertyField>("m_Script").SetEnabled(false);
            inspectorData.Q<PropertyField>("interactableCategory").RegisterCallback<ChangeEvent<string>>(OnCategorySet);
            OnCategorySet();

            int index = inspectorData.IndexOf(customStages);
            stagesField = new EnumFlagsField((R2API.DirectorAPI.Stage)0);
            stagesField.label = "Stages";
            stagesField.bindingPath = "stages";
            stagesField.RegisterValueChangedCallback(OnStageSet);
            OnStageSet();
            inspectorData.Insert(index++, stagesField);
        }

        private void OnCategorySet(ChangeEvent<string> evt = null)
        {
            string val = evt == null ? TargetType.interactableCategory.ToString() : evt.newValue;
            bool shouldDisplay = Enum.GetName(typeof(R2API.DirectorAPI.InteractableCategory), R2API.DirectorAPI.InteractableCategory.Custom) == val;

            customCategory.SetDisplay(shouldDisplay);
            customCategoryWeight.SetDisplay(shouldDisplay);
        }

        private void OnStageSet(ChangeEvent<Enum> evt = null)
        {
            R2API.DirectorAPI.Stage stage = evt == null ? TargetType.stages : (R2API.DirectorAPI.Stage)evt.newValue;
            bool shouldDisplay = stage.HasFlag(R2API.DirectorAPI.Stage.Custom);

            customStages.SetDisplay(shouldDisplay);
        }
    }
}
