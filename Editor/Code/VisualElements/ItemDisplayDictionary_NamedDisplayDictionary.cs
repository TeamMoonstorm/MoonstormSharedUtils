using RoR2EditorKit;
using RoR2EditorKit.VisualElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.VisualElements
{
    public class ItemDisplayDictionary_NamedDisplayDictionary : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ItemDisplayDictionary_NamedDisplayDictionary, UxmlTraits> { }
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
                if(_currentEntry != value )
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
        public TextField IDRSName { get; }
        public ExtendedListView ExtendedListView { get; }
        public event Action<CollectionButtonEntry> OnDisplayRuleButtonClicked;

        private VisualElement standardViewContainer;
        private SerializedProperty idrsName;

        private void UpdateBinding()
        {
            if(SerializedProperty == null)
            {
                ExtendedListView.collectionProperty = null;
                idrsName = null;
                HelpBox.SetDisplay(true);
                HelpBox.message = "Please Select a NamedDisplayDictionary Entry from the SideBar";
                HelpBox.messageType = MessageType.Info;
                standardViewContainer.SetDisplay(false);
                IDRSName.Unbind();
                return;
            }

            ExtendedListView.collectionProperty = SerializedProperty.FindPropertyRelative("displayRules");
            idrsName = SerializedProperty.FindPropertyRelative("idrsName");
            IDRSName.BindProperty(idrsName);
            HelpBox.SetDisplay(false);
            standardViewContainer.SetDisplay(true);

            OnIDRSNameChange(null, idrsName.stringValue);
        }

        public void OnKeyAssetFieldValueSet(ScriptableObject so)
        {
            this.SetDisplay(so);
        }

        public void CheckForIDD(SerializedObject so)
        {
            if(so == null)
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
            entry.extraData = CurrentEntry?.extraData;
            entry.SerializedProperty = property;
            entry.Button.clicked += () => OnDisplayRuleButtonClicked?.Invoke(entry);
        }

        private void UpdateButtonDisplay(CollectionButtonEntry entry)
        {
            if (entry.SerializedProperty == null)
                return;

            var displayName = entry.SerializedProperty.FindPropertyRelative("displayPrefabIndex");
            var displays = entry.extraData as string[];

            if(displays == null)
            {
                entry.Button.text = "Invalid Rule";
                entry.HelpBox.messageType = MessageType.Warning;
                entry.HelpBox.message = "This Rule may be Invalid, looks like the DisplayPrefabIndex is out of range.";
                return;
            }
            if(displays.Length > displayName.intValue)
            {
                string childName = CheckChildName();
                entry.Button.text = $"{displays[displayName.intValue]}|{childName}";
                return;
            }
            else
            {
                entry.Button.text = "Invalid Rule";
                entry.HelpBox.messageType = MessageType.Warning;
                entry.HelpBox.message = "This Rule may be Invalid, looks like the DisplayPrefabIndex is out of range.";
                return;
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

        private void OnIDRSNameChange(ChangeEvent<string> evt, string defaultVal = null)
        {
            string newVal = evt?.newValue ?? defaultVal;
            idrsName.stringValue = newVal;
            if(!Catalog.DoesIDRSExist(newVal))
            {
                HelpBox.message = "The IDRSName value for this entry may be invalid, as the value wasnt found in the ItemDisplayCatalog, are you sure your ItemDisplayCattalog is up to date?";
                HelpBox.messageType = MessageType.Info;
                HelpBox.SetDisplay(true);
                IDRSName.isReadOnly = false;
            }
            else
            {
                HelpBox.message = "Please Select a NamedDisplayDictionary Entry from the SideBar";
                HelpBox.messageType = MessageType.Info;
                HelpBox.SetDisplay(false);
                IDRSName.isReadOnly = true;
            }
            ExtendedListView.Refresh();
            CurrentEntry.UpdateRepresentation?.Invoke(CurrentEntry);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            ListView listView = ExtendedListView.Q<ListView>();
            listView.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            HelpBox.SetDisplay(SerializedProperty == null);
            standardViewContainer.SetDisplay(SerializedProperty != null);

            IDRSName.isDelayed = true;
            IDRSName.RegisterValueChangedCallback((txt) => OnIDRSNameChange(txt, null));

            ExtendedListView.CreateElement = CreateElement;
            ExtendedListView.BindElement = BindElement;
        }
        private void OnDetach(DetachFromPanelEvent evt) { }

        public ItemDisplayDictionary_NamedDisplayDictionary()
        {
            TemplateHelpers.GetTemplateInstance(nameof(ItemDisplayDictionary_NamedDisplayDictionary), this, (_) => true);
            HelpBox = this.Q<HelpBox>();
            ExtendedListView = this.Q<ExtendedListView>();
            IDRSName = this.Q<TextField>();
            standardViewContainer = this.Q<VisualElement>("StandardView");

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }
        ~ItemDisplayDictionary_NamedDisplayDictionary()
        {
            UnregisterCallback<AttachToPanelEvent>(OnAttach);
            UnregisterCallback<DetachFromPanelEvent>(OnDetach);
        }
    }
}
