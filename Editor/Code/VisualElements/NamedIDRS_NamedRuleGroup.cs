using Moonstorm.AddressableAssets;
using RoR2;
using RoR2EditorKit;
using RoR2EditorKit.Data;
using RoR2EditorKit.VisualElements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.VisualElements
{
	public class NamedIDRS_NamedRuleGroup : VisualElement
	{
        public new class UxmlFactory : UxmlFactory<NamedIDRS_NamedRuleGroup, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }
        public ItemDisplayCatalog Catalog { get; internal set; }
        public CollectionButtonEntry CurrentEntry
        {
            get
            {
                return _currentEntry;
            }
            set
            {
                if(_currentEntry != value)
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
        public TextField KeyAsset { get; }
        public ReadOnlyCollection<string> DisplayPrefabs
        {
            get
            {
                return CurrentEntry?.extraData as ReadOnlyCollection<string>;
            }
            set
            {
                if(CurrentEntry != null)
                {
                    CurrentEntry.extraData = value;
                }
            }
        }
        public ExtendedListView ExtendedListView { get; }
        public event Action<CollectionButtonEntry> OnNamedRuleButtonClicked;

        private VisualElement standardViewContainer;
        private SerializedProperty keyAsset;
        private void UpdateBinding()
        {
            if(SerializedProperty == null)
            {
                ExtendedListView.collectionProperty = null;
                keyAsset = null;
                HelpBox.SetDisplay(true);
                HelpBox.message = "Please Select a NamedRuleGroup Entry from the SideBar";
                HelpBox.messageType = MessageType.Info;
                standardViewContainer.SetDisplay(false);
                KeyAsset.Unbind();
                return;
            }
            ExtendedListView.collectionProperty = SerializedProperty.FindPropertyRelative("rules");
            keyAsset = SerializedProperty.FindPropertyRelative("keyAssetName");
            KeyAsset.BindProperty(keyAsset);
            HelpBox.SetDisplay(false);
            standardViewContainer.SetDisplay(true);

            OnKeyAssetNameChange(null, keyAsset.stringValue);
        }

        public void OnIDRSFieldValueSet(ItemDisplayRuleSet obj)
        {
            this.SetDisplay(obj);
        }

        public void CheckForNamedIDRS(SerializedObject serializedObject)
        {
            if (serializedObject == null)
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

        private VisualElement CreateElement()
        {
            return new CollectionButtonEntry();
        }

        private void BindElement(VisualElement element, SerializedProperty property)
        {
            CollectionButtonEntry entry = (CollectionButtonEntry)element;
            entry.style.height = ExtendedListView.listViewItemHeight;
            entry.Button.style.height = ExtendedListView.listViewItemHeight;
            entry.UpdateRepresentation = UpdateButtonDisplay;
            entry.extraData = DisplayPrefabs;
            entry.SerializedProperty = property;
            entry.Button.clickable.clicked += () => OnNamedRuleButtonClicked?.Invoke(entry);
        }

        private void UpdateButtonDisplay(CollectionButtonEntry entry)
        {
            if (entry.SerializedProperty == null)
                return;

            var displayName = entry.SerializedProperty.FindPropertyRelative("displayPrefabName");
            var displays = entry.extraData as ReadOnlyCollection<string>;

            if(displays == null)
            {
                entry.Button.text = "Invalid Rule";
                entry.HelpBox.messageType = MessageType.Warning;
                entry.HelpBox.message = "This Rule may be Invalid, looks like the DisplayPrefab could not be found in the ItemDisplayCatalog, are you sure your catalog is up to date?";
                return;
            }
            displayName.stringValue = displayName.stringValue.IsNullOrEmptyOrWhitespace() ? displays.FirstOrDefault() : displayName.stringValue;
            if(displays.Contains(displayName.stringValue))
            {
                string childName = CheckChildName();
                entry.Button.text = $"{displayName.stringValue}|{childName}";
                return;
            }
            else
            {
                entry.Button.text = "Invalid Rule";
                entry.HelpBox.messageType = MessageType.Warning;
                entry.HelpBox.message = "This Rule may be Invalid, looks like the DisplayPrefab could not be found in the ItemDisplayCatalog, are you sure your catalog is up to date?";
            }

            string CheckChildName()
            {
                var childNameProp = entry.SerializedProperty.FindPropertyRelative("childName");
                var childName = childNameProp.stringValue.IsNullOrEmptyOrWhitespace() ? "RuntimeSetup" : childNameProp.stringValue;

                entry.HelpBox.SetDisplay(childName == "RuntimeSetup");
                entry.HelpBox.messageType = childName == "RuntimeSetup" ? MessageType.Info : MessageType.None;
                entry.HelpBox.message = childName == "RuntimeSetup" ? "No child specified, this entry will be modified to attach to the first childLocator entry of the model, useful for getting the values from the IDPH" : string.Empty;

                return childName;
            }
        }

        private void OnKeyAssetNameChange(ChangeEvent<string> evt, string defaultVal = null)
        {
            string newVal = evt?.newValue ?? defaultVal;
            keyAsset.stringValue = newVal;
            var potentialCollection = Catalog.GetKeyAssetDisplays(newVal);
            if(potentialCollection == null)
            {
                HelpBox.message = "The KeyAsset value for this entry may be invalid, as the value wasnt found in the ItemDisplayCatalog, are your sure your ItemDisplayCatalog is up to date?";
                HelpBox.messageType = MessageType.Info;
                HelpBox.SetDisplay(true);
                KeyAsset.isReadOnly = false;
            }
            else
            {
                HelpBox.message = "Please Select a NamedRuleGroup Entry from the SideBar";
                HelpBox.messageType = MessageType.Info;
                HelpBox.SetDisplay(false);
                KeyAsset.isReadOnly = true;
            }
            DisplayPrefabs = potentialCollection;
            ExtendedListView.Refresh();
            CurrentEntry.UpdateRepresentation?.Invoke(CurrentEntry);
        }
        private void KeyAssetNameChange(ChangeEvent<string> evt) => OnKeyAssetNameChange(evt, null);
        private void OnAttach(AttachToPanelEvent evt)
        {
            ListView listView = ExtendedListView.Q<ListView>();
            listView.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            HelpBox.SetDisplay(SerializedProperty == null);
            standardViewContainer.SetDisplay(SerializedProperty != null);

            KeyAsset.isDelayed = true;
            KeyAsset.RegisterValueChangedCallback(KeyAssetNameChange);

            ExtendedListView.CreateElement = CreateElement;
            ExtendedListView.BindElement = BindElement;
        }
        private void OnDetach(DetachFromPanelEvent evt)
        {
            KeyAsset.UnregisterValueChangedCallback(KeyAssetNameChange);

            ExtendedListView.CreateElement = null;
            ExtendedListView.BindElement = null;
        }

        public NamedIDRS_NamedRuleGroup()
        {
            TemplateHelpers.GetTemplateInstance(nameof(NamedIDRS_NamedRuleGroup), this, (pth) => pth.ValidateUXMLPath());
            HelpBox = this.Q<HelpBox>();
            ExtendedListView = this.Q<ExtendedListView>();
            KeyAsset = this.Q<TextField>();
            standardViewContainer = this.Q<VisualElement>("StandardView");

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }
        ~NamedIDRS_NamedRuleGroup()
        {
            UnregisterCallback<AttachToPanelEvent>(OnAttach);
            UnregisterCallback<DetachFromPanelEvent>(OnDetach);
        }
    }
}