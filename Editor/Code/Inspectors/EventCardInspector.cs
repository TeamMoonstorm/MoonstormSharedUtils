﻿using R2API;
using RoR2EditorKit;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(EventCard))]
    public sealed class EventCardInspector : MSScriptableObjectInspector<EventCard>, IObjectNameConvention
    {
        VisualElement directorData;
        PropertyField customStages;

        public string Prefix => "ec";

        public bool UsesTokenForPrefix => false;

        protected override void OnEnable()
        {
            base.OnEnable();
            OnVisualTreeCopy += () =>
            {
                directorData = DrawInspectorElement.Q<VisualElement>("DirectorDataHolder");
                customStages = directorData.Q<PropertyField>(nameof(EventCard.customStageNames));
            };
        }
        protected override void DrawInspectorGUI()
        {
            var stages = new EnumFlagsField("Available Stages", DirectorAPI.Stage.Custom);
            stages.tooltip = $"The stages where this event can play";
            stages.RegisterValueChangedCallback(OnStageSet);
            stages.name = nameof(EventCard.availableStages);
            stages.bindingPath = stages.name;
            directorData.Insert(1, stages);

            customStages.style.display = TargetType.availableStages.HasFlag(DirectorAPI.Stage.Custom) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnStageSet(ChangeEvent<Enum> evt)
        {
            customStages.style.display = evt.newValue.HasFlag(DirectorAPI.Stage.Custom) ? DisplayStyle.Flex : DisplayStyle.None;
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
