using RoR2;
using RoR2.Editor;
using System;
using System.Globalization;
using System.Linq;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MSU.Editor.VisualElements
{
    public class ItemDisplayDictionary_DisplayRule : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ItemDisplayDictionary_DisplayRule, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }
        public CollectionButtonEntry CurrentEntry
        {
            get
            {
                return _currentEntry;
            }
            set
            {
                if (_currentEntry != value)
                {
                    _currentEntry = value;
                    SerializedProperty?.serializedObject.ApplyModifiedProperties();
                    SerializedProperty = value?.SerializedProperty;
                    UpdateBinding();
                }
            }
        }
        private CollectionButtonEntry _currentEntry;
        public SerializedProperty SerializedProperty { get; private set; }
        public HelpBox HelpBox { get; }
        public EnumField ItemDisplayRuleType { get; }
        public IMGUIContainer DisplayPrefab { get; }
        public Button PasteButton { get; }
        public TextField ChildName { get; }
        public Vector3Field LocalPos { get; }
        public Vector3Field LocalRot { get; }
        public Vector3Field LocalScale { get; }
        public EnumFlagsField LimbMask { get; }
        public string[] DisplayPrefabsAsString => (string[])(CurrentEntry?.extraData);

        private VisualElement standardViewContainer;
        private SerializedProperty ruleType;
        private SerializedProperty displayPrefabIndex;
        private SerializedProperty childName;
        private SerializedProperty localPos;
        private SerializedProperty localRot;
        private SerializedProperty localScale;
        private SerializedObject _serializedObject;
        private void UpdateBinding()
        {
            if (SerializedProperty == null)
            {
                ruleType = null;
                displayPrefabIndex = null;
                childName = null;
                localPos = null;
                localRot = null;
                localScale = null;
                HelpBox.SetDisplay(true);
                standardViewContainer.SetDisplay(false);
                DisplayPrefab.onGUIHandler = null;
                ItemDisplayRuleType.Unbind();
                PasteButton.Unbind();
                ChildName.Unbind();
                LocalPos.Unbind();
                LocalRot.Unbind();
                LocalScale.Unbind();
                LimbMask.Unbind();
                return;
            }

            ruleType = SerializedProperty.FindPropertyRelative("ruleType");
            displayPrefabIndex = SerializedProperty.FindPropertyRelative("displayPrefabIndex");
            childName = SerializedProperty.FindPropertyRelative("childName");
            localPos = SerializedProperty.FindPropertyRelative("localPos");
            localRot = SerializedProperty.FindPropertyRelative("localAngles");
            localScale = SerializedProperty.FindPropertyRelative("localScale");

            ItemDisplayRuleType.BindProperty(ruleType);
            DisplayPrefab.onGUIHandler = DrawDropDown;
            ChildName.BindProperty(childName);
            LocalPos.BindProperty(localPos);
            LocalRot.BindProperty(localRot);
            LocalScale.BindProperty(localScale);
            LimbMask.BindProperty(SerializedProperty.FindPropertyRelative("limbMask"));

            HelpBox.SetDisplay(false);
            standardViewContainer.SetDisplay(true);

            OnRuleTypeChange(null, (ItemDisplayRuleType?)ruleType.enumValueIndex);
        }

        public void OnKeyAssetFieldValueSet(ScriptableObject so)
        {
            this.SetDisplay(so);
        }

        public void CheckForIDD(SerializedObject serializedObject)
        {
            _serializedObject = serializedObject;
            if (_serializedObject == null)
            {
                this.SetDisplay(false);
                SerializedProperty = null;
            }
            else
            {
                HelpBox.SetDisplay(true);
                standardViewContainer.SetDisplay(false);
            }
        }

        private void OnRuleTypeChange(ChangeEvent<Enum> evt, ItemDisplayRuleType? defaultValue)
        {
            if ((evt == null || evt.newValue == null) && !defaultValue.HasValue)
            {
                return;
            }

            ItemDisplayRuleType ruleType = (ItemDisplayRuleType)(evt?.newValue ?? defaultValue.Value);

            LimbMask.SetDisplay(ruleType == RoR2.ItemDisplayRuleType.LimbMask);
        }

        private void PasteValues()
        {
            if (SerializedProperty == null)
                return;

            string clipboardContent = GUIUtility.systemCopyBuffer;
            try
            {
                var split = clipboardContent.Split(',').ToArray();
                childName.stringValue = split[0];
                localPos.vector3Value = CreateVector3FromArray(new string[3] { split[1], split[2], split[3] });
                localRot.vector3Value = CreateVector3FromArray(new string[3] { split[4], split[5], split[6] });
                localScale.vector3Value = CreateVector3FromArray(new string[3] { split[7], split[8], split[9] });
                SerializedProperty.serializedObject.ApplyModifiedProperties();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to paste clipboard contents ({clipboardContent}) to {CurrentEntry.Button.text}'s values!\n{ex}");
            }

            Vector3 CreateVector3FromArray(string[] args)
            {
                return new Vector3(float.Parse(args[0], CultureInfo.InvariantCulture), float.Parse(args[1], CultureInfo.InvariantCulture), float.Parse(args[2], CultureInfo.InvariantCulture));
            }
        }

        private void DrawDropDown()
        {
            if (_serializedObject == null)
                return;

            if (!(_serializedObject.targetObject is ItemDisplayDictionary idd))
                return;

            int currentIndex = displayPrefabIndex.intValue;
            if (currentIndex == -1)
            {
                displayPrefabIndex.intValue = 0;
                displayPrefabIndex.serializedObject.ApplyModifiedProperties();
                return;
            }
            else if (currentIndex >= idd.displayPrefabs.Length)
            {
                EditorGUILayout.LabelField(new GUIContent($"Display Prefab Index is Out of Range.", $"Display Prefab Index is {displayPrefabIndex.intValue}, Display Prefabs array length is {idd.displayPrefabs.Length}"), EditorStyles.boldLabel);
                return;
            }
            int newIndex = EditorGUILayout.Popup("Display Prefab Index", currentIndex, DisplayPrefabsAsString);
            displayPrefabIndex.intValue = newIndex;
            if (displayPrefabIndex.serializedObject.ApplyModifiedProperties())
            {
                CurrentEntry?.UpdateRepresentation?.Invoke(CurrentEntry);
            }
        }
        private void RuleTypeChange(ChangeEvent<Enum> evt) => OnRuleTypeChange(evt, null);
        private void UpdateRepresentation(ChangeEvent<string> _) => CurrentEntry?.UpdateRepresentation?.Invoke(CurrentEntry);
        private void OnAttach(AttachToPanelEvent evt)
        {
            HelpBox.SetDisplay(SerializedProperty == null);
            standardViewContainer.SetDisplay(SerializedProperty != null);

            ChildName.isDelayed = true;
            DisplayPrefab.onGUIHandler = DrawDropDown;
            PasteButton.clicked += PasteValues;
            ItemDisplayRuleType.RegisterValueChangedCallback(RuleTypeChange);
            ChildName.RegisterValueChangedCallback(UpdateRepresentation);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            DisplayPrefab.onGUIHandler = null;
            PasteButton.clicked -= PasteValues;
            ItemDisplayRuleType.UnregisterValueChangedCallback(RuleTypeChange);
            ChildName.UnregisterValueChangedCallback(UpdateRepresentation);
        }

        public ItemDisplayDictionary_DisplayRule()
        {
            TemplateHelpers.GetTemplateInstance(nameof(ItemDisplayDictionary_DisplayRule), this, (pth) => pth.ValidateUXMLPath());
            HelpBox = this.Q<HelpBox>();
            standardViewContainer = this.Q<VisualElement>("StandardView");
            DisplayPrefab = this.Q<IMGUIContainer>();
            PasteButton = this.Q<Button>();
            ChildName = this.Q<TextField>();
            LocalPos = this.Q<Vector3Field>("LocalPos");
            LocalRot = this.Q<Vector3Field>("LocalRot");
            LocalScale = this.Q<Vector3Field>("LocalScale");

            ItemDisplayRuleType = new EnumField("Item Display Rule Type", RoR2.ItemDisplayRuleType.ParentedPrefab);
            standardViewContainer.Insert(0, ItemDisplayRuleType);

            LimbMask = new EnumFlagsField("Limb Mask", RoR2.LimbFlags.None);
            standardViewContainer.Add(LimbMask);

            RegisterCallback<DetachFromPanelEvent>(OnDetach);
            RegisterCallback<AttachToPanelEvent>(OnAttach);
        }

        ~ItemDisplayDictionary_DisplayRule()
        {
            UnregisterCallback<DetachFromPanelEvent>(OnDetach);
            UnregisterCallback<AttachToPanelEvent>(OnAttach);
        }
    }
}
