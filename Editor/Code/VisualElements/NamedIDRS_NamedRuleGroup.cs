using Moonstorm.AddressableAssets;
using RoR2;
using RoR2EditorKit;
using RoR2EditorKit.Data;
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
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.VisualElements
{
	public class NamedIDRS_NamedRuleGroup : VisualElement
	{
        public new class UxmlFactory : UxmlFactory<NamedIDRS_NamedRuleGroup, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }
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
        public Toggle UseDirectReference { get; }
        public ObjectField AssetField { get; }
        public TextField AddressField { get; }
        public EnumField LoadAssetFromField { get; }
        public ExtendedListView ExtendedListView { get; }
        public event Action<CollectionButtonEntry> OnNamedRuleButtonClicked;

        private VisualElement standardViewContainer;
        private VisualElement keyAssetContainer;
        private VisualElement addressContainer;
        private SerializedProperty useDirectReference;
        private SerializedProperty asset;
        private SerializedProperty address;
        private SerializedProperty loadAssetFrom;
        private void UpdateBinding()
        {
            if(SerializedProperty == null)
            {
                ExtendedListView.collectionProperty = null;
                useDirectReference = null;
                asset = null;
                address = null;
                loadAssetFrom = null;
                HelpBox.SetDisplay(true);
                standardViewContainer.SetDisplay(false);
                UseDirectReference.Unbind();
                AssetField.Unbind();
                AddressField.Unbind();
                LoadAssetFromField.Unbind();
                return;
            }
            ExtendedListView.collectionProperty = SerializedProperty.FindPropertyRelative("rules");
            var addressableKeyAsset = SerializedProperty.FindPropertyRelative("keyAsset");
            useDirectReference = addressableKeyAsset.FindPropertyRelative("useDirectReference");
            asset = addressableKeyAsset.FindPropertyRelative("asset");
            address = addressableKeyAsset.FindPropertyRelative("address");
            loadAssetFrom = addressableKeyAsset.FindPropertyRelative("loadAssetFrom");
            UseDirectReference.BindProperty(useDirectReference);
            AssetField.BindProperty(asset);
            AddressField.BindProperty(address);
            LoadAssetFromField.BindProperty(loadAssetFrom);
            HelpBox.SetDisplay(false);
            standardViewContainer.SetDisplay(true);

            OnUseDirectReferenceChange(null, useDirectReference.boolValue);
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

        private void OnUseDirectReferenceChange(ChangeEvent<bool> changeEvent, bool? defaultValue = null)
        {
            if (changeEvent == null && !defaultValue.HasValue)
            {
                UpdateButton(changeEvent);
                return;
            }
            bool newValue = changeEvent?.newValue ?? defaultValue.Value;
            useDirectReference.boolValue = newValue;
            keyAssetContainer.SetDisplay(newValue);
            addressContainer.SetDisplay(!newValue);
            LoadAssetFromField.SetValueWithoutNotify(newValue ? AddressableAssets.AddressableKeyAsset.KeyAssetAddressType.UsingDirectReference : AddressableAssets.AddressableKeyAsset.KeyAssetAddressType.Addressables);

            UpdateButton(changeEvent);
        }

        private void OnKeyAssetSet(ChangeEvent<UnityEngine.Object> evt)
        {
            var value = evt.newValue;
            if(!value)
            {
                asset.objectReferenceValue = value;
                UpdateButton(evt);
                return;
            }

            if((value is ItemDef) || (value is EquipmentDef))
            {
                asset.objectReferenceValue = value;
                UpdateButton(evt);
                return;
            }

            Debug.LogWarning("Only EquipmentDefs or ItemDefs can be used as key assets");
            INotifyValueChanged<ScriptableObject> obj = evt.target as INotifyValueChanged<ScriptableObject>;
            obj.SetValueWithoutNotify((ScriptableObject)evt.previousValue);
        }

        private void OnAddressSet(ChangeEvent<string> evt)
        {
            AddressableKeyAsset.KeyAssetAddressType addressType = (AddressableKeyAsset.KeyAssetAddressType)LoadAssetFromField.value;

            if (addressType == AddressableKeyAsset.KeyAssetAddressType.EquipmentCatalog || addressType == AddressableKeyAsset.KeyAssetAddressType.ItemCatalog)
            {
                address.stringValue = evt.newValue;
                UpdateButton(evt);
                return;
            }

            var newAddress = evt.newValue;
            if(AddressablesUtils.AddressableCatalogExists && !useDirectReference.boolValue)
            {
                try
                {
                    Addressables.LoadAssetAsync<ScriptableObject>(newAddress);
                }
                catch(InvalidKeyException ex)
                {
                    Debug.LogError($"The current address is not a valid address. {ex}");
                }
            }

            address.stringValue = evt.newValue;
            UpdateButton(evt);
        }

        private void UpdateButton(EventBase changeEvent)
        {
            if(CurrentEntry != null && CurrentEntry.parent != null)
            {
                CurrentEntry.UpdateRepresentation?.Invoke(CurrentEntry);
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
            entry.SerializedProperty = property;
            entry.Button.clickable.clicked += () => OnNamedRuleButtonClicked?.Invoke(entry);
        }

        private void UpdateButtonDisplay(CollectionButtonEntry entry)
        {
            if (entry.SerializedProperty == null)
                return;

            bool validEntry = false;
            string prefabName = string.Empty;
            entry.SerializedProperty.serializedObject.ApplyModifiedProperties();
            var addressableGameObject = entry.SerializedProperty.FindPropertyRelative("displayPrefab");
            if(addressableGameObject.FindPropertyRelative("useDirectReference").boolValue)
            {
                var asset = addressableGameObject.FindPropertyRelative("asset");
                if(asset.objectReferenceValue)
                {
                    validEntry = true;
                    prefabName = asset.objectReferenceValue.name;
                }
                else
                {
                    validEntry = false;
                    prefabName = null;
                }
            }
            else
            {
                var address = addressableGameObject.FindPropertyRelative("address").stringValue;
                if(address.IsNullOrEmptyOrWhitespace())
                {
                    validEntry = false;
                    prefabName = null;
                }
                else
                {
                    validEntry = true;
                    string[] split = address.Split('/');
                    prefabName = split[split.Length - 1];
                }
            }

            if(validEntry)
            {
                var childNameProp = entry.SerializedProperty.FindPropertyRelative("childName");
                string childName = childNameProp.stringValue.IsNullOrEmptyOrWhitespace() ? "RuntimeSetup" : childNameProp.stringValue;
                entry.Button.text = prefabName + " | " + childName;
                entry.HelpBox.SetDisplay(childName == "RuntimeSetup");
                entry.HelpBox.messageType = childName == "RuntimeSetup" ? MessageType.Info : MessageType.None ;
                entry.HelpBox.message = childName == "RuntimeSetup" ? "No child specified, this entry will be modified to attach to the first childLocator entry of the model, useful for getting the values from the IDPH" : string.Empty;
            }
            else
            {
                entry.HelpBox.SetDisplay(true);
                entry.HelpBox.messageType = MessageType.Warning;
                entry.HelpBox.message = "Display Prefab not Set";
                entry.Button.text = "Invalid Rule";
            }
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            ListView listView = ExtendedListView.Q<ListView>();
            listView.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            HelpBox.SetDisplay(SerializedProperty == null);
            standardViewContainer.SetDisplay(SerializedProperty != null);

            AssetField.SetObjectType<ScriptableObject>();
            AssetField.RegisterValueChangedCallback(OnKeyAssetSet);
            UseDirectReference.RegisterValueChangedCallback((x) => OnUseDirectReferenceChange(x));
            AddressField.RegisterValueChangedCallback(OnAddressSet);
            AddressField.isDelayed = true;

            ExtendedListView.CreateElement = CreateElement;
            ExtendedListView.BindElement = BindElement;
        }
        private void OnDetach(DetachFromPanelEvent evt)
        {

        }

        public NamedIDRS_NamedRuleGroup()
        {
            TemplateHelpers.GetTemplateInstance(nameof(NamedIDRS_NamedRuleGroup), this, (_) => true);
            HelpBox = this.Q<HelpBox>();
            ExtendedListView = this.Q<ExtendedListView>();
            AddressField = this.Q<TextField>();
            AssetField = this.Q<ObjectField>();
            UseDirectReference = this.Q<Toggle>();
            keyAssetContainer = this.Q<VisualElement>("KeyAssetContainer");
            addressContainer = this.Q<VisualElement>("AddressContainer");
            standardViewContainer = this.Q<VisualElement>("StandardView");

            LoadAssetFromField = new EnumField("Load Asset From", AddressableAssets.AddressableKeyAsset.KeyAssetAddressType.UsingDirectReference);
            addressContainer.Add(LoadAssetFromField);

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