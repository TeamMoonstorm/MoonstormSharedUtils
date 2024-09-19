using RoR2;
using RoR2.Editor;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MSU.Editor.UIElements
{
    public class NamedItemDisplayRuleSet_RuleEditor : VisualElement, ISerializedObjectBoundCallback, IItemDisplayCatalogReceiver
    {
        public ExtendedHelpBox helpBox { get; }
        public EnumField ruleType { get; }
        public IMGUIContainer displayPrefabName { get; }
        public EnumFlagsField limbMask { get; }
        public Button pasteFromClipboardButton { get; }
        public TextField childName { get; }
        public Vector3Field localPos { get; }
        public Vector3Field localAngles { get; }
        public Vector3Field localScale { get; }
        public ItemDisplayCatalog catalog { get; set; }
        public PropertySelectorButton currentlyInspectedEntry
        {
            get => _currentlyInspectedEntry;
            set
            {
                if (_currentlyInspectedEntry != value)
                {
                    value?.representingProperty.serializedObject.ApplyModifiedProperties();
                    _currentlyInspectedEntry = value;
                    UpdateBinding();
                }
            }
        }
        private PropertySelectorButton _currentlyInspectedEntry;
        private VisualElement _controlContainer;

        private ReadOnlyCollection<string> _availableDisplayPrefabs;
        private ReadOnlyStringCollectionDropdown _dropdown;
        private SerializedProperty _ruleTypeProp;
        private SerializedProperty _limbMaskProp;
        private SerializedProperty _displayPrefabProp;
        private SerializedProperty _childNameProp;
        private SerializedProperty _localPositionProp;
        private SerializedProperty _localAnglesProp;
        private SerializedProperty _localScaleProp;

        public void OnBoundSerializedObjectChange(SerializedObject so)
        {
            if (so == null)
            {
                this.SetDisplay(false);
                currentlyInspectedEntry = null;
                return;
            }
            this.SetDisplay(true);
            helpBox.SetDisplay(true);
            _controlContainer.SetDisplay(false);
        }

        private void UpdateBinding()
        {
            if (currentlyInspectedEntry == null || currentlyInspectedEntry.representingProperty == null)
            {
                helpBox.SetDisplay(true);
                helpBox.message = "Please Select a NamedRuleGroup Entry from the Sidebar";
                helpBox.messageType = MessageType.Info;
                _controlContainer.SetDisplay(false);
                _availableDisplayPrefabs = null;
                _dropdown = null;

                _ruleTypeProp = null;
                _limbMaskProp = null;
                _displayPrefabProp = null;
                _childNameProp = null;
                _localPositionProp = null;
                _localAnglesProp = null;
                _localScaleProp = null;

                ruleType.Unbind();
                limbMask.Unbind();
                childName.Unbind();
                localPos.Unbind();
                localAngles.Unbind();
                localScale.Unbind();

                return;
            }

            _controlContainer.SetDisplay(true);
            helpBox.SetDisplay(false);

            _availableDisplayPrefabs = currentlyInspectedEntry.extraData as ReadOnlyCollection<string>;
            _dropdown = new ReadOnlyStringCollectionDropdown(new UnityEditor.IMGUI.Controls.AdvancedDropdownState(), _availableDisplayPrefabs, "Display Prefabs");
            _dropdown.onItemSelected += OnDisplayPrefabChange;

            var prop = currentlyInspectedEntry.representingProperty;
            _ruleTypeProp = prop.FindPropertyRelative("ruleType");
            _displayPrefabProp = prop.FindPropertyRelative("displayPrefabName");
            _childNameProp = prop.FindPropertyRelative("childName");
            _limbMaskProp = prop.FindPropertyRelative("limbMask");
            _localPositionProp = prop.FindPropertyRelative("localPos");
            _localAnglesProp = prop.FindPropertyRelative("localAngles");
            _localScaleProp = prop.FindPropertyRelative("localScale");

            ruleType.BindProperty(_ruleTypeProp);
            limbMask.BindProperty(_limbMaskProp);
            limbMask.SetDisplay(_ruleTypeProp.enumValueIndex == (int)ItemDisplayRuleType.LimbMask);
            childName.BindProperty(_childNameProp);
            localPos.BindProperty(_localPositionProp);
            localAngles.BindProperty(_localAnglesProp);
            localScale.BindProperty(_localScaleProp);
        }

        private void OnDisplayPrefabChange(ReadOnlyStringCollectionDropdown.Item obj)
        {
            string newVal = obj.value;

            _displayPrefabProp.stringValue = newVal;
            _displayPrefabProp.serializedObject.ApplyModifiedProperties();

            currentlyInspectedEntry.updateRepresentation?.Invoke(currentlyInspectedEntry);
        }

        private void DrawIMGUI()
        {
            if (_dropdown == null)
                return;

            ReadOnlyStringCollectionDropdown.DrawIMGUI(_dropdown, _displayPrefabProp.stringValue, new GUIContent("Display Prefab"), "No Display Prefab Set");
        }
        private void OnRuleTypeChanged(ChangeEvent<Enum> evt)
        {
            if (evt == null || evt.newValue == null)
                return;

            ItemDisplayRuleType ruleType = (ItemDisplayRuleType)evt.newValue;
            limbMask.SetDisplay(ruleType == ItemDisplayRuleType.LimbMask);
        }

        private void PasteFromClipboard()
        {
            if (currentlyInspectedEntry == null || currentlyInspectedEntry.representingProperty == null)
                return;

            string clipboardContent = GUIUtility.systemCopyBuffer;
            try
            {
                var split = clipboardContent.Split(',').ToArray();
                childName.value = split[0];
                localPos.value = CreateVector3FromArray(new string[3] { split[1], split[2], split[3] });
                localAngles.value = CreateVector3FromArray(new string[3] { split[4], split[5], split[6] });
                localScale.value = CreateVector3FromArray(new string[3] { split[7], split[8], split[9] });
                currentlyInspectedEntry.representingProperty.serializedObject.ApplyModifiedProperties();
            }
            catch (Exception ex)
            {
                MSULog.Error($"Failed to paste clipboard contents to {currentlyInspectedEntry.button.text}'s values! (Content={clipboardContent}).\n{ex}");
            }

            Vector3 CreateVector3FromArray(string[] args)
            {
                var invariant = CultureInfo.InvariantCulture;
                return new Vector3(float.Parse(args[0], invariant), float.Parse(args[1], invariant), float.Parse(args[2], invariant));
            }
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            displayPrefabName.onGUIHandler = DrawIMGUI;
            pasteFromClipboardButton.clicked += PasteFromClipboard;
            ruleType.RegisterValueChangedCallback(OnRuleTypeChanged);
            RegisterCallback<GeometryChangedEvent>(GeoChange);
        }

        private void GeoChange(GeometryChangedEvent evt)
        {
            Debug.Log(style.display);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            displayPrefabName.onGUIHandler = null;
            pasteFromClipboardButton.clicked -= PasteFromClipboard;
            ruleType.UnregisterValueChangedCallback(OnRuleTypeChanged);
            UnregisterCallback<GeometryChangedEvent>(GeoChange);
        }

        public NamedItemDisplayRuleSet_RuleEditor()
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(NamedItemDisplayRuleSet_RuleEditor), this, p => p.ValidateUXMLPath());

            helpBox = this.Q<ExtendedHelpBox>();
            _controlContainer = this.Q<VisualElement>("ControlContainer");
            ruleType = this.Q<EnumField>();
            displayPrefabName = this.Q<IMGUIContainer>();
            var container = this.Q<VisualElement>("LimbMaskContainer");
            limbMask = new EnumFlagsField("Limb Mask", RoR2.LimbFlags.None);
            container.Add(limbMask);
            pasteFromClipboardButton = this.Q<Button>("PasteFromClipboard");
            childName = this.Q<TextField>("ChildName");
            localPos = this.Q<Vector3Field>("LocalPos");
            localAngles = this.Q<Vector3Field>("LocalAngles");
            localScale = this.Q<Vector3Field>("LocalScales");

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        new public class UxmlFactory : UxmlFactory<NamedItemDisplayRuleSet_RuleEditor, UxmlTraits> { }
        new public class UxmlTraits : VisualElement.UxmlTraits { }
    }
}
