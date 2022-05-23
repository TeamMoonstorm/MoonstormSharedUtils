using RoR2EditorKit.Core.EditorWindows;
using RoR2EditorKit.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using RoR2EditorKit.Core.Inspectors;
using System;
using ThunderKit.Markdown;
using Moonstorm.AddressableAssets;
using System.Text.RegularExpressions;
using System.Linq;
using RoR2;
using System.Collections.Generic;
using System.Globalization;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public class NamedIDRSEditorWindow : MSObjectEditingEditorWindow<NamedIDRS>
    {
        VisualElement rootContainer;
        VisualElement namedRuleGroupContainer;
        VisualElement ruleGroupContainer;
        VisualElement ruleDisplayContainer;

        ListViewHelper namedRuleGroupsHelper;
        ListViewHelper rulesHelper;

        SerializedProperty InspectedNamedRuleGroup
        {
            get => _inspectedNamedRuleGroup;
            set
            {
                if(_inspectedNamedRuleGroup != value)
                {
                    _inspectedNamedRuleGroup = value;
                    OnInspectedNamedRuleGroupChanged();
                }
            }
        }
        SerializedProperty _inspectedNamedRuleGroup = null;

        SerializedProperty InspectedRule
        {
            get => _inspectedRule;
            set
            {
                if(_inspectedRule != value)
                {
                    _inspectedRule = value;
                    OnInspectedRuleChanged();
                }
            }
        }
        SerializedProperty _inspectedRule = null;
        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();
            var namedRuleGroupsData = new ListViewHelper.ListViewHelperData(
                SerializedObject.FindProperty(nameof(NamedIDRS.namedRuleGroups)),
                namedRuleGroupContainer.Q<ListView>("buttonView"),
                namedRuleGroupContainer.Q<IntegerField>("arraySize"),
                () => new Button(),
                BindNamedRuleGroupButton);
            namedRuleGroupsHelper = new ListViewHelper(namedRuleGroupsData);

            var ruleGroupsData = new ListViewHelper.ListViewHelperData
            {
                property = null,
                intField = ruleGroupContainer.Q<VisualElement>("SubContainer").Q<IntegerField>("arraySize"),
                listView = ruleGroupContainer.Q<VisualElement>("SubContainer").Q<ListView>("buttonView"),
                createElement = () => new Button(),
                bindElement = BindRuleGroupButton
            };
            rulesHelper = new ListViewHelper(ruleGroupsData);
            SetupCallbacks();
        }
        protected override void CreateGUI()
        {
            base.CreateGUI();
            rootContainer = rootVisualElement.Q<VisualElement>("RootContainer");
            namedRuleGroupContainer = rootContainer.Q<VisualElement>("NamedRuleGroupContainer");
            ruleGroupContainer = rootContainer.Q<VisualElement>("RuleGroupContainer");
            ruleDisplayContainer = rootContainer.Q<VisualElement>("RuleDisplayContainer");

            var subContainer = ruleDisplayContainer.Q<VisualElement>("SubContainer");

            EnumFlagsField enumFlagsField = new EnumFlagsField("Limb Mask", LimbFlags.None);
            enumFlagsField.name = "limbMask";
            subContainer.Add(enumFlagsField);
            enumFlagsField.SendToBack();

            EnumField enumField = new EnumField("Rule Type", ItemDisplayRuleType.ParentedPrefab);
            enumField.name = "ruleType";
            subContainer.Add(enumField);
            enumField.SendToBack();

            var idphContainer = subContainer.Q<VisualElement>("IDPHValueContainer");
            idphContainer.Q<Button>("pasteFromIDPH").clicked += Paste;
        }

        private void Paste()
        {
            try
            {
                var valuesAsString = GUIUtility.systemCopyBuffer;

                List<string> splitValues = valuesAsString.Split(',').ToList();
                InspectedRule.FindPropertyRelative(nameof(NamedIDRS.AddressNamedDisplayRule.childName)).stringValue = splitValues[0];
                InspectedRule.FindPropertyRelative(nameof(NamedIDRS.AddressNamedDisplayRule.localPos))
                    .vector3Value = CreateVector3FromList(new List<string> { splitValues[1], splitValues[2], splitValues[3] });
                InspectedRule.FindPropertyRelative(nameof(NamedIDRS.AddressNamedDisplayRule.localAngles))
                    .vector3Value = CreateVector3FromList(new List<string> { splitValues[4], splitValues[5], splitValues[6] });
                InspectedRule.FindPropertyRelative(nameof(NamedIDRS.AddressNamedDisplayRule.localScales))
                    .vector3Value = CreateVector3FromList(new List<string> { splitValues[5], splitValues[6], splitValues[7] });

                SerializedObject.ApplyModifiedProperties();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to paste clipboard contents to {InspectedRule.name}'s values!" +
                        $"\n{ex}");
            }

            Vector3 CreateVector3FromList(List<string> args)
            {
                return new Vector3(float.Parse(args[0], CultureInfo.InvariantCulture), float.Parse(args[1], CultureInfo.InvariantCulture), float.Parse(args[2], CultureInfo.InvariantCulture));
            }
        }

        private void OnInspectedNamedRuleGroupChanged()
        {
            var mdElement = ruleGroupContainer.Q<MarkdownElement>("noNamedRuleGroupSelected");
            var subContainer = ruleGroupContainer.Q<VisualElement>("SubContainer");

            if(InspectedNamedRuleGroup == null)
            {
                mdElement.SetDisplay(DisplayStyle.Flex);
                subContainer.SetDisplay(DisplayStyle.None);
                return;
            }

            mdElement.SetDisplay(DisplayStyle.None);
            subContainer.SetDisplay(DisplayStyle.Flex);

            rulesHelper.SerializedProperty = InspectedNamedRuleGroup.FindPropertyRelative(nameof(NamedIDRS.AddressNamedRuleGroup.rules));
            var keyAsset = subContainer.Q<PropertyField>("keyAsset");
            keyAsset.Clear();
            keyAsset.bindingPath = InspectedNamedRuleGroup.FindPropertyRelative(nameof(NamedIDRS.AddressNamedRuleGroup.keyAsset)).propertyPath;
            if (keyAsset.childCount == 0)
                keyAsset.Bind(SerializedObject);

            var foldoutContainer = keyAsset.Q<Foldout>().Q<VisualElement>("unity-content");
            foldoutContainer[0]
                .RegisterCallback<ChangeEvent<UnityEngine.Object>>(_ => namedRuleGroupsHelper.TiedListView.Refresh());
            foldoutContainer[1]
                .RegisterCallback<ChangeEvent<string>>(_ => namedRuleGroupsHelper.TiedListView.Refresh());
            foldoutContainer[2]
                .RegisterCallback<ChangeEvent<string>>(_ => namedRuleGroupsHelper.TiedListView.Refresh());
        }

        private void OnInspectedRuleChanged()
        {
            var mdElement = ruleDisplayContainer.Q<MarkdownElement>("noDisplayRuleSelected");
            var subContainer = ruleDisplayContainer.Q<VisualElement>("SubContainer");

            if(InspectedRule == null)
            {
                mdElement.SetDisplay(DisplayStyle.Flex);
                subContainer.SetDisplay(DisplayStyle.None);
                return;
            }
            var inspectedRulesParentGroup = InspectedRule.GetParentProperty().GetParentProperty();
            if(inspectedRulesParentGroup.propertyPath != InspectedNamedRuleGroup.propertyPath)
            {
                mdElement.SetDisplay(DisplayStyle.Flex);
                subContainer.SetDisplay(DisplayStyle.None);
                return;
            }

            mdElement.SetDisplay(DisplayStyle.None);
            subContainer.SetDisplay(DisplayStyle.Flex);
            subContainer.Unbind();

            var enumFlags = subContainer.Q<EnumFlagsField>("limbMask");
            enumFlags.bindingPath = InspectedRule.FindPropertyRelative(nameof(NamedIDRS.AddressNamedDisplayRule.limbMask)).propertyPath;

            var enumField = subContainer.Q<EnumField>("ruleType");
            enumField.bindingPath = InspectedRule.FindPropertyRelative(nameof(NamedIDRS.AddressNamedDisplayRule.ruleType)).propertyPath;

            var idphValuesContainer = subContainer.Q<VisualElement>("IDPHValueContainer");
            idphValuesContainer.Q<TextField>("childName").bindingPath = InspectedRule.FindPropertyRelative(nameof(NamedIDRS.AddressNamedDisplayRule.childName)).propertyPath;
            idphValuesContainer.Q<Vector3Field>("localPos").bindingPath = InspectedRule.FindPropertyRelative(nameof(NamedIDRS.AddressNamedDisplayRule.localPos)).propertyPath;
            idphValuesContainer.Q<Vector3Field>("localAngles").bindingPath = InspectedRule.FindPropertyRelative(nameof(NamedIDRS.AddressNamedDisplayRule.localAngles)).propertyPath;
            idphValuesContainer.Q<Vector3Field>("localScales").bindingPath = InspectedRule.FindPropertyRelative(nameof(NamedIDRS.AddressNamedDisplayRule.localScales)).propertyPath;

            var displayPrefab = subContainer.Q<PropertyField>("displayPrefab");
            displayPrefab.Clear();
            displayPrefab.bindingPath = InspectedRule.FindPropertyRelative(nameof(NamedIDRS.AddressNamedDisplayRule.displayPrefab)).propertyPath;

            
            subContainer.Bind(SerializedObject);

            var foldoutContainer = displayPrefab.Q<Foldout>().Q<VisualElement>("unity-content");
            foldoutContainer[0]
                .RegisterCallback<ChangeEvent<string>>(_ => rulesHelper.TiedListView.Refresh());
            foldoutContainer[1]
                .RegisterCallback<ChangeEvent<UnityEngine.Object>>(_ => rulesHelper.TiedListView.Refresh());
        }
        #region Callbacks
        private void SetupCallbacks()
        {
            var targetIdrs = rootVisualElement.Q<PropertyField>("targetIdrs");
            targetIdrs.RegisterCallback<ChangeEvent<UnityEngine.Object>>(OnIDRSSet);
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
            Button button = arg1 as Button;

            SerializedProperty keyAsset = arg2.FindPropertyRelative(nameof(NamedIDRS.AddressNamedRuleGroup.keyAsset));
            SerializedProperty objectProperty = keyAsset.FindPropertyRelative("asset");
            SerializedProperty addressProperty = keyAsset.FindPropertyRelative("address");
            SerializedProperty enumProp = keyAsset.FindPropertyRelative("loadAssetFrom");

            if (objectProperty.objectReferenceValue)
                button.text = objectProperty.objectReferenceValue.name;
            else if (!addressProperty.stringValue.IsNullOrEmptyOrWhitespace())
                button.text = BuildNameFromAddress(addressProperty.stringValue);
            else
                button.text = $"No KeyAsset Set";

            button.clicked += SetInspectedNamedRuleGroupProperty;

            button.AddManipulator(new ContextualMenuManipulator((builder) =>
            {
                builder.menu.AppendAction("Delete Array Element", DeleteElement);
            }));

            string BuildNameFromAddress(string address)
            {
                var value = (AddressableKeyAsset.KeyAssetAddressType)enumProp.enumValueIndex;
                switch (value)
                {
                    case AddressableKeyAsset.KeyAssetAddressType.Addressables:
                        string[] split = address.Split('/');
                        string name = split[split.Length - 1];
                        return name;
                    case AddressableKeyAsset.KeyAssetAddressType.ItemCatalog:
                        return $"Item: {address}";
                    case AddressableKeyAsset.KeyAssetAddressType.EquipmentCatalog:
                        return $"Equipment: {address}";
                }
                return $"Invalid KeyAssetAddressType";
            }

            void SetInspectedNamedRuleGroupProperty()
            {
                InspectedNamedRuleGroup = arg2;
                InspectedRule = null;
            }

            void DeleteElement(DropdownMenuAction menuAction)
            {
                int arrayIndex = int.Parse(button.name.Substring("element".Length), CultureInfo.InvariantCulture);
                var arrayProp = arg2.GetParentProperty();

                arrayProp.DeleteArrayElementAtIndex(arrayIndex);
                arrayProp.serializedObject.ApplyModifiedProperties();
                namedRuleGroupsHelper.Refresh();
            }
        }

        private void BindRuleGroupButton(VisualElement arg1, SerializedProperty arg2)
        {
            Button button = arg1 as Button;
            SerializedProperty displayPrefab = arg2.FindPropertyRelative(nameof(NamedIDRS.AddressNamedDisplayRule.displayPrefab));
            SerializedProperty objectProperty = displayPrefab.FindPropertyRelative("asset");
            SerializedProperty addressProperty = displayPrefab.FindPropertyRelative("address");

            if (objectProperty.objectReferenceValue)
            {
                button.text = objectProperty.objectReferenceValue.name;
            }
            else if(!addressProperty.stringValue.IsNullOrEmptyOrWhitespace())
            {
                string[] split = addressProperty.stringValue.Split('/');
                button.text = split[split.Length - 1];
            }
            else
            {
                button.text = $"No DisplayPrefab Set";
            }

            button.AddManipulator(new ContextualMenuManipulator((builder) =>
            {
                builder.menu.AppendAction("Delete Array Element", DeleteElement);
            }));

            button.clicked += SetInspectedRule;

            void SetInspectedRule()
            {
                InspectedRule = arg2;
            }

            void DeleteElement(DropdownMenuAction act)
            {
                int arrayIndex = int.Parse(button.name.Substring("element".Length), CultureInfo.InvariantCulture);
                var arrayProp = arg2.GetParentProperty();

                arrayProp.DeleteArrayElementAtIndex(arrayIndex);
                arrayProp.serializedObject.ApplyModifiedProperties();
                namedRuleGroupsHelper.Refresh();
            }
        }
    }
}
