using RoR2.Editor;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MSU.Editor.UIElements
{
    public class NamedItemDisplayRuleSet_KeyAssetRuleSelector : VisualElement, ISerializedObjectBoundCallback, IItemDisplayCatalogReceiver
    {
        public ExtendedHelpBox helpBox { get; }
        public VisualElement controlContainer { get; }
        public IMGUIContainer keyAssetIMGUIContainer { get; }
        public ListView buttonListView { get; }
        public ItemDisplayCatalog catalog { get; set; }

        public PropertySelectorButton currentlyInspectedEntry
        {
            get => _currentlyInspectedEntry;
            set
            {
                if(_currentlyInspectedEntry != value)
                {
                    value?.representingProperty.serializedObject.ApplyModifiedProperties();
                    _currentlyInspectedEntry = value;
                    UpdateBinding();
                }
            }
        }
        private PropertySelectorButton _currentlyInspectedEntry;
        private SerializedProperty _keyAssetNameProperty;
        private SerializedProperty _rulesProperty;
        private ReadOnlyCollection<string> _displayPrefabs;
        private ReadOnlyStringCollectionDropdown _dropdown;

        public event Action<PropertySelectorButton> onNamedRuleButtonClicked;
        public void OnBoundSerializedObjectChange(SerializedObject so)
        {
            if(so == null)
            {
                this.SetDisplay(false);
                currentlyInspectedEntry = null;
            }
            else
            {
                this.SetDisplay(true);
                helpBox.SetDisplay(true);
                controlContainer.SetDisplay(false);
            }
        }

        private VisualElement CreateButtonContainer()
        {
            return new PropertySelectorButton();
        }

        private void BindButtonContainer(VisualElement element, int index)
        {
            if (index == -1 || index >= _rulesProperty.arraySize)
                return;

            var property = _rulesProperty.GetArrayElementAtIndex(index);
            PropertySelectorButton propertySelector = (PropertySelectorButton)element;
            element.name = "element " + index;
            propertySelector.index = index;
            propertySelector.style.height = buttonListView.fixedItemHeight;
            propertySelector.style.maxHeight = buttonListView.fixedItemHeight;
            propertySelector.button.style.flexGrow = 1;

            propertySelector.button.clicked += () => onNamedRuleButtonClicked?.Invoke(propertySelector);
            propertySelector.updateRepresentation = UpdateButtonDisplay;
            propertySelector.extraData = _displayPrefabs;
            propertySelector.representingProperty = property;
        }

        private void UpdateButtonDisplay(PropertySelectorButton instance)
        {
            var newProperty = instance.representingProperty;
            if(newProperty == null)
            {
                return;
            }

            var displayName = newProperty.FindPropertyRelative("displayPrefabName");
            var displays = instance.extraData as ReadOnlyCollection<string>;

            if (displays == null)
            {
                instance.button.text = "Invalid Rule";
                instance.helpBox.messageType = MessageType.Warning;
                instance.helpBox.message = $"This Rule may be Invalid, looks like the DisplayPrefab could not be found in the ItemDisplayCatalog. Are you sure your catalog is up to date?";
                return;
            }

            displayName.stringValue = displayName.stringValue.IsNullOrEmptyOrWhiteSpace() ? displays.FirstOrDefault() : displayName.stringValue;
            if(displays.Contains(displayName.stringValue))
            {
                string childName = CheckChildName();
                instance.button.text = $"{displayName.stringValue}|{childName}";
            }
            else
            {
                instance.button.text = "Invalid Rule";
                instance.helpBox.messageType = MessageType.Warning;
                instance.helpBox.message = $"This Rule may be Invalid, looks like the DisplayPrefab could not be found in the ItemDisplayCatalog. Are you sure your catalog is up to date?";
                return;
            }

            string CheckChildName()
            {
                var childNameProp = newProperty.FindPropertyRelative("childName");
                var childName = childNameProp.stringValue.IsNullOrEmptyOrWhiteSpace() ? "RuntimeSetup" : childNameProp.stringValue;

                instance.helpBox.SetDisplay(childName == "RuntimeSetup");
                instance.helpBox.messageType = childName == "RuntimeSetup" ? MessageType.Info : MessageType.None;
                instance.helpBox.message = childName == "RuntimeSetup" ? "No child specified, this entry will be modified to attach to the first childLocator entry of the model, useful for getting the values from the IDPH" : string.Empty;

                return childName;
            }
        }

        private void DrawDropdown()
        {
            if (_dropdown == null)
                return;

            ReadOnlyStringCollectionDropdown.DrawIMGUI(_dropdown, _keyAssetNameProperty.stringValue, new UnityEngine.GUIContent("Key Asset"), "No Key Asset Set");
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            keyAssetIMGUIContainer.onGUIHandler = DrawDropdown;
            buttonListView.makeItem = CreateButtonContainer;
            buttonListView.bindItem = BindButtonContainer;
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            keyAssetIMGUIContainer.onGUIHandler = null;
            buttonListView.makeItem = null;
            buttonListView.bindItem = null;
        }

        private void UpdateBinding()
        {
            if(currentlyInspectedEntry == null || currentlyInspectedEntry.representingProperty == null)
            {
                buttonListView.Unbind();
                helpBox.SetDisplay(true);
                helpBox.message = "Please Select a NamedRuleGroup Entry from the Sidebar";
                helpBox.messageType = MessageType.Info;
                controlContainer.SetDisplay(false);
                _displayPrefabs = null;
                _dropdown = null;
                return;
            }

            _rulesProperty = currentlyInspectedEntry.representingProperty.FindPropertyRelative("rules");
            _keyAssetNameProperty = currentlyInspectedEntry.representingProperty.FindPropertyRelative("keyAssetName");
            _displayPrefabs = (ReadOnlyCollection<string>)currentlyInspectedEntry.extraData;
            _dropdown = new ReadOnlyStringCollectionDropdown(new AdvancedDropdownState(), catalog.allKeyAssets, "Key Assets");
            _dropdown.onItemSelected += OnKeyAssetChange;

            buttonListView.BindProperty(_rulesProperty);
            helpBox.SetDisplay(false);
            controlContainer.SetDisplay(true);
        }

        private void OnKeyAssetChange(ReadOnlyStringCollectionDropdown.Item item)
        {
            string newVal = item.value;
            var potentialCollection = catalog.GetKeyAssetDisplays(newVal);
            bool shouldUpdate = false;
            if(potentialCollection == null)
            {
                helpBox.message = "The KeyAsset value for this entry may be invalid, as the value wasnt found in the ItemDisplayCatalog, are your sure your ItemDisplayCatalog is up to date?";
                helpBox.messageType = MessageType.Info;
                helpBox.SetDisplay(true);
            }
            else
            {
                helpBox.message = "Please Select a NamedRuleGroup Entry from the SideBar";
                helpBox.messageType = MessageType.Info;
                helpBox.SetDisplay(false);
                _displayPrefabs = potentialCollection;
                buttonListView.Rebuild();
                shouldUpdate = true;
            }

            _keyAssetNameProperty.stringValue = newVal;
            _keyAssetNameProperty.serializedObject.ApplyModifiedProperties();
            if (shouldUpdate)
                currentlyInspectedEntry.updateRepresentation?.Invoke(currentlyInspectedEntry);
        }

        public NamedItemDisplayRuleSet_KeyAssetRuleSelector()
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, this, p => p.ValidateUXMLPath());

            keyAssetIMGUIContainer = this.Q<IMGUIContainer>();
            buttonListView = this.Q<ListView>();
            helpBox = this.Q<ExtendedHelpBox>();
            controlContainer = this.Q<VisualElement>("ControlContainer");

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        new public class UxmlFactory : UxmlFactory<NamedItemDisplayRuleSet_KeyAssetRuleSelector, UxmlTraits> { }
        new public class UxmlTraits : VisualElement.UxmlTraits { }
    }
}