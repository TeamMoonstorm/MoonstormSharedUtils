using RoR2EditorKit.Core.EditorWindows;
using RoR2EditorKit.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using RoR2EditorKit.Core.Inspectors;
using System;
using ThunderKit.Markdown;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public class NamedIDRSEditorWindow : MSExtendedEditorWindow
    {
        NamedIDRS TargetType { get; set; }
        VisualElement rootContainer;
        VisualElement namedRuleGroupContainer;
        VisualElement ruleGroupContainer;
        VisualElement ruleDisplayContainer;

        ListViewHelper namedRuleGroupsHelper;

        protected override void OnWindowOpened()
        {
            TargetType = MainSerializedObject.targetObject as NamedIDRS;

            var namedRuleGroupsData = new ListViewHelper.ListViewHelperData(
                MainSerializedObject.FindProperty(nameof(NamedIDRS.namedRuleGroups)),
                namedRuleGroupContainer.Q<ListView>("buttonView"),
                namedRuleGroupContainer.Q<IntegerField>("arraySize"),
                () => new Button(),
                BindNamedRuleGroupButton);

            namedRuleGroupsHelper = new ListViewHelper(namedRuleGroupsData);
            SetupCallbacks();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            rootContainer = rootVisualElement.Q<VisualElement>("RootContainer");
            namedRuleGroupContainer = rootContainer.Q<VisualElement>("NamedRuleGroupContainer");
            ruleGroupContainer = rootContainer.Q<VisualElement>("RuleGroupContainer");
            ruleDisplayContainer = rootContainer.Q<VisualElement>("RuleDisplayContainer");
        }
        protected override void DrawGUI()
        {

        }

        #region Callbacks
        private void SetupCallbacks()
        {
            var targetIdrs = rootVisualElement.Q<ObjectField>("targetIdrs");
            targetIdrs.RegisterValueChangedCallback(OnIDRSSet);
            OnIDRSSet();
        }

        private void OnIDRSSet(ChangeEvent<UnityEngine.Object> evt = null)
        {
            var value = evt == null ? TargetType.idrs : evt.newValue;
            var noTargetMD = rootContainer.Q<MarkdownElement>("noTargetMD");
            if (value)
            {
                noTargetMD.SetDisplay(DisplayStyle.None);
                namedRuleGroupContainer.SetDisplay(DisplayStyle.Flex);
                ruleGroupContainer.SetDisplay(DisplayStyle.Flex);
                ruleDisplayContainer.SetDisplay(DisplayStyle.Flex);
            }
            else
            {
                noTargetMD.SetDisplay(DisplayStyle.Flex);
                namedRuleGroupContainer.SetDisplay(DisplayStyle.None);
                ruleGroupContainer.SetDisplay(DisplayStyle.None);
                ruleDisplayContainer.SetDisplay(DisplayStyle.None);
            }
        }
        #endregion
        private void BindNamedRuleGroupButton(VisualElement arg1, SerializedProperty arg2)
        {
        }
    }
}
