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
    public class ItemDisplayDictionaryEditorWindow : MSObjectEditingEditorWindow<ItemDisplayDictionary>
    {
        VisualElement keyAssetContainer;
        VisualElement rootContainer;
        VisualElement dictionaryContainer;
        VisualElement displayRulesContainer;
        VisualElement ruleDisplayContainer;

        ListViewHelper dictionaryHelper;
        ListViewHelper displayRulesHelper;

        SerializedProperty InspectedDictionaryEntry
        {
            get => _inspectedDictionaryEntry;
            set
            {
                if(_inspectedDictionaryEntry != value)
                {
                    _inspectedDictionaryEntry = value;
                    OnInspectedDictionaryEntryChanged();
                }
            }
        }
        SerializedProperty _inspectedDictionaryEntry;

        SerializedProperty InspectedRule
        {
            get => _inspectedRule;
            set
            {
                if (_inspectedRule != value)
                {
                    _inspectedRule = value;
                    OnInspectedRuleChanged();
                }
            }
        }
        SerializedProperty _inspectedRule = null;
        public void OnDisable()
        {
            SerializedObject.ApplyModifiedProperties();
        }

        protected override void CreateGUI()
        {
            base.CreateGUI();
            keyAssetContainer = rootVisualElement.Q<VisualElement>("KeyAssetPrefabContainer");
            rootContainer = rootVisualElement.Q<VisualElement>("RootContainer");
            dictionaryContainer = rootContainer.Q<VisualElement>("DictionaryContainer");
            displayRulesContainer = rootContainer.Q<VisualElement>("DisplayRulesContainer");
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

            var idphContainer = subContainer.Q<VisualElement>("IDPHValuesContainer");
            idphContainer.Q<Button>("pasteFromClipboard").clicked += Paste;

            void Paste()
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
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();
            var dictionaryData = new ListViewHelper.ListViewHelperData
            {
                property = SerializedObject.FindProperty(nameof(ItemDisplayDictionary.namedDisplayDictionary)),
                intField = dictionaryContainer.Q<IntegerField>("arraySize"),
                listView = dictionaryContainer.Q<ListView>("buttonView"),
                createElement = () => new Button(),
                bindElement = BindDictionaryEntryButton
            };
            dictionaryHelper = new ListViewHelper(dictionaryData);

            var displayRulesData = new ListViewHelper.ListViewHelperData
            {
                property = null,
                intField = displayRulesContainer.Q<VisualElement>("SubContainer").Q<IntegerField>("arraySize"),
                listView = displayRulesContainer.Q<VisualElement>("SubContainer").Q<ListView>("buttonView"),
                createElement = () => new Button(),
                bindElement = BindDisplayRuleButton
            };
            displayRulesHelper = new ListViewHelper(displayRulesData);

            SetupCallbacks();
        }

        private void SetupCallbacks()
        {
            var keyAsset = keyAssetContainer.Q<ObjectField>("keyAsset");
            keyAsset.SetObjectType<ScriptableObject>();
            keyAsset.RegisterValueChangedCallback(OnKeyAssetSet);

            //This is beyond stupid
            var displayPrefab = keyAssetContainer.Q<ObjectField>("displayPrefab");
            displayPrefab.SetObjectType<GameObject>();
            displayPrefab.BindProperty(SerializedObject.FindProperty(nameof(ItemDisplayDictionary.displayPrefab)));
            displayPrefab.RegisterValueChangedCallback(OnDisplayPrefabSet);

            OnKeyAssetSet();
            OnDisplayPrefabSet();
            SerializedObject.ApplyModifiedProperties();

            void OnKeyAssetSet(ChangeEvent<UnityEngine.Object> evt = null)
            {
                var value = evt == null ? TargetType.keyAsset : evt.newValue;

                if(value)
                {
                    var flag0 = value is ItemDef;
                    var flag1 = value is EquipmentDef;
                    if(!flag0 && !flag1)
                    {
                        EditorUtility.DisplayDialog($"Invalid KeyAsset", $"An IDRS KeyAsset must be either an EquipmentDef or ItemDef, the selected KeyAsset is {value.GetType().Name} \nThe field will has been set to \"None\"", "Ok");

                        value = null;
                        if (evt.target != null && (evt.target as VisualElement).name == "keyAsset")
                            (evt.target as ObjectField).value = value;
                    }
                }

                var noKADP = rootContainer.Q<MarkdownElement>("noKADP");

                if(value && TargetType.displayPrefab)
                {
                    noKADP.SetDisplay(false);
                    dictionaryContainer.SetDisplay(true);
                    displayRulesContainer.SetDisplay(true);
                    ruleDisplayContainer.SetDisplay(true);
                }
                else
                {
                    noKADP.SetDisplay(true);
                    dictionaryContainer.SetDisplay(false);
                    displayRulesContainer.SetDisplay(false);
                    ruleDisplayContainer.SetDisplay(false);
                }
            }

            void OnDisplayPrefabSet(ChangeEvent<UnityEngine.Object> evt = null)
            {
                var value = evt == null ? TargetType.displayPrefab : evt.newValue;

                var noKADP = rootContainer.Q<MarkdownElement>("noKADP");

                if (value && TargetType.keyAsset)
                {
                    noKADP.SetDisplay(false);
                    dictionaryContainer.SetDisplay(true);
                    displayRulesContainer.SetDisplay(true);
                    ruleDisplayContainer.SetDisplay(true);
                }
                else
                {
                    noKADP.SetDisplay(true);
                    dictionaryContainer.SetDisplay(false);
                    displayRulesContainer.SetDisplay(false);
                    ruleDisplayContainer.SetDisplay(false);
                }
            }
        }

        private void BindDictionaryEntryButton(VisualElement element, SerializedProperty prop)
        {
            Button button = element as Button;

            var addressableAssetProp = prop.FindPropertyRelative(nameof(ItemDisplayDictionary.NamedDisplayDictionary.idrs));
            var objectProp = addressableAssetProp.FindPropertyRelative("asset");
            var addressProp = addressableAssetProp.FindPropertyRelative("address");

            if (objectProp.objectReferenceValue)
                button.text = objectProp.objectReferenceValue.name;
            else if (!addressProp.stringValue.IsNullOrEmptyOrWhitespace())
                button.text = BuildNameFromAddress(addressProp.stringValue);
            else
                button.text = "No IDRS Set";

            button.clicked += SetInspectedDictionaryEntry;

            button.AddManipulator(new ContextualMenuManipulator((builder) =>
            {
                builder.menu.AppendAction("Delete Array Element", DeleteElement);
            }));

            string BuildNameFromAddress(string address)
            {
                string[] split = address.Split('/');
                string name = split[split.Length - 1];
                return name;
            }
            
            void SetInspectedDictionaryEntry()
            {
                InspectedDictionaryEntry = prop;
                InspectedRule = null;
            }

            void DeleteElement(DropdownMenuAction act)
            {
                int arrayIndex = int.Parse(button.name.Substring("element".Length), CultureInfo.InvariantCulture);
                var arrayProp = prop.GetParentProperty();

                arrayProp.DeleteArrayElementAtIndex(arrayIndex);
                arrayProp.serializedObject.ApplyModifiedProperties();
                dictionaryHelper.Refresh();
            }
        }

        private void BindDisplayRuleButton(VisualElement element, SerializedProperty prop)
        {
            Button button = element as Button;
            SerializedProperty childName = prop.FindPropertyRelative(nameof(ItemDisplayDictionary.DisplayRule.childName));

            button.text = childName.stringValue.IsNullOrEmptyOrWhitespace() ? "No ChildName Set" : childName.stringValue;

            button.clicked += SetInspectedRule;

            button.AddManipulator(new ContextualMenuManipulator((builder) =>
            {
                builder.menu.AppendAction("Delete Array Element", DeleteElement);
            }));

            void SetInspectedRule()
            {
                InspectedRule = prop;
            }

            void DeleteElement(DropdownMenuAction act)
            {
                int arrayIndex = int.Parse(button.name.Substring("element".Length), CultureInfo.InvariantCulture);
                var arrayProp = prop.GetParentProperty();

                arrayProp.DeleteArrayElementAtIndex(arrayIndex);
                arrayProp.serializedObject.ApplyModifiedProperties();
                displayRulesHelper.Refresh();
            }
        }

        private void OnInspectedDictionaryEntryChanged()
        {
            var mdElement = displayRulesContainer.Q<MarkdownElement>("noEntrySelected");
            var subContainer = displayRulesContainer.Q<VisualElement>("SubContainer");

            if(InspectedDictionaryEntry == null)
            {
                mdElement.SetDisplay(true);
                subContainer.SetDisplay(false);
                return;
            }

            mdElement.SetDisplay(false);
            subContainer.SetDisplay(true);

            displayRulesHelper.SerializedProperty = InspectedDictionaryEntry.FindPropertyRelative(nameof(ItemDisplayDictionary.NamedDisplayDictionary.displayRules));
            var idrs = subContainer.Q<PropertyField>("idrs");
            idrs.Clear();
            idrs.bindingPath = InspectedDictionaryEntry.FindPropertyRelative(nameof(ItemDisplayDictionary.NamedDisplayDictionary.idrs)).propertyPath;
            if (idrs.childCount == 0)
                idrs.Bind(SerializedObject);

            var foldoutContainer = idrs.Q<Foldout>().Q<VisualElement>("unity-content");
            foldoutContainer[0]
                .RegisterCallback<ChangeEvent<string>>(_ => dictionaryHelper.TiedListView.Refresh());
            foldoutContainer[1]
                .RegisterCallback<ChangeEvent<UnityEngine.Object>>(_ => dictionaryHelper.TiedListView.Refresh());

            SerializedObject.ApplyModifiedProperties();
        }

        private void OnInspectedRuleChanged()
        {
            var mdElement = ruleDisplayContainer.Q<MarkdownElement>("noDisplayRuleSelected");
            var subContainer = ruleDisplayContainer.Q<VisualElement>("SubContainer");

            if(InspectedRule == null || InspectedRule.GetParentProperty().GetParentProperty().propertyPath != InspectedDictionaryEntry.propertyPath)
            {
                mdElement.SetDisplay(true);
                subContainer.SetDisplay(false);
                return;
            }

            mdElement.SetDisplay(false);
            subContainer.SetDisplay(true);
            subContainer.Unbind();

            var enumFlags = subContainer.Q<EnumFlagsField>("limbMask");
            enumFlags.bindingPath = InspectedRule.FindPropertyRelative(nameof(ItemDisplayDictionary.DisplayRule.limbMask)).propertyPath;

            var enumField = subContainer.Q<EnumField>("ruleType");
            enumField.bindingPath = InspectedRule.FindPropertyRelative(nameof(ItemDisplayDictionary.DisplayRule.ruleType)).propertyPath;

            var idphValuesContainer = subContainer.Q<VisualElement>("IDPHValuesContainer");
            idphValuesContainer.Q<TextField>("childName").bindingPath = InspectedRule.FindPropertyRelative(nameof(ItemDisplayDictionary.DisplayRule.childName)).propertyPath;
            idphValuesContainer.Q<Vector3Field>("localPos").bindingPath = InspectedRule.FindPropertyRelative(nameof(ItemDisplayDictionary.DisplayRule.localPos)).propertyPath;
            idphValuesContainer.Q<Vector3Field>("localAngles").bindingPath = InspectedRule.FindPropertyRelative(nameof(ItemDisplayDictionary.DisplayRule.localAngles)).propertyPath;
            idphValuesContainer.Q<Vector3Field>("localScale").bindingPath = InspectedRule.FindPropertyRelative(nameof(ItemDisplayDictionary.DisplayRule.localScales)).propertyPath;

            subContainer.Bind(SerializedObject);

            idphValuesContainer.Q<TextField>("childName").RegisterValueChangedCallback(_ => displayRulesHelper.TiedListView.Refresh());

            SerializedObject.ApplyModifiedProperties();
        }
    }
}
