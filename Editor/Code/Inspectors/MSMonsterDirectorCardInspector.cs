using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Utilities;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(MSMonsterDirectorCard))]
    public class MSMonsterDirectorCardInspector : ExtendedInspector<MSMonsterDirectorCard>
    {
        VisualElement inspectorData;
        PropertyField customCategory;

        EnumFlagsField stagesField;
        PropertyField customStages;
        protected override void OnEnable()
        {
            base.OnEnable();
            OnVisualTreeCopy += () =>
            {
                inspectorData = DrawInspectorElement.Q<VisualElement>("InspectorDataContainer");
                customCategory = inspectorData.Q<PropertyField>("customCategory");
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
            inspectorData.Q<PropertyField>("monsterCategory").RegisterCallback<ChangeEvent<string>>(OnCategorySet);
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
            string val = evt == null ? TargetType.monsterCategory.ToString() : evt.newValue;
            bool shouldDisplay = Enum.GetName(typeof(R2API.DirectorAPI.MonsterCategory), R2API.DirectorAPI.MonsterCategory.Custom) == val;

            customCategory.SetDisplay(shouldDisplay);
        }

        private void OnStageSet(ChangeEvent<Enum> evt = null)
        {
            R2API.DirectorAPI.Stage stage = evt == null ? TargetType.stages : (R2API.DirectorAPI.Stage)evt.newValue;
            bool shouldDisplay = stage.HasFlag(R2API.DirectorAPI.Stage.Custom);

            customStages.SetDisplay(shouldDisplay);
        }
    }
}
