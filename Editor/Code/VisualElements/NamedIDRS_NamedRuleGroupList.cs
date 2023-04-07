using Moonstorm.AddressableAssets;
using RoR2;
using RoR2EditorKit;
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
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.VisualElements
{
    public class NamedIDRS_NamedRuleGroupList : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<NamedIDRS_NamedRuleGroupList, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }
        public ItemDisplayCatalog Catalog { get; internal set; }
        public Button ForceCatalogUpdate { get; }
        public Button SortByName { get; }
        public Button AddAllEquipmentsButton { get; }
        public Button AddAllItemsButton { get; }
        public Button AddOnlyEliteEquipmentsButton { get; }
        public Button AddMissingEntriesButton { get; }
        public ExtendedListView ExtendedListView { get; }
        public event Action<CollectionButtonEntry> OnNamedRuleGroupButtonClicked;
        public event Action OnForceCatalogUpdate;

        private SerializedObject _serializedObject;
        internal void OnIDRSFieldValueSet(ItemDisplayRuleSet obj)
        {
            this.SetDisplay(obj);
        }

        public void CheckForNamedIDRS(SerializedObject serializedObject)
        {
            _serializedObject = serializedObject;
            if(_serializedObject == null)
            {
                this.SetDisplay(false);
                ExtendedListView.collectionProperty = null;
                return;
            }

            if(_serializedObject.targetObject is NamedIDRS)
            {
                this.SetDisplay(true);
                ExtendedListView.collectionProperty = serializedObject.FindProperty("namedRuleGroups");
            }
        }

        private VisualElement CreateButton()
        {
            var namedRuleGroupEntry = new CollectionButtonEntry();
            return namedRuleGroupEntry;
        }

        private void BindButton(VisualElement visualElement, SerializedProperty property)
        {
            CollectionButtonEntry entry = (CollectionButtonEntry)visualElement;
            entry.style.height = ExtendedListView.listViewItemHeight;
            entry.Button.style.height = ExtendedListView.listViewItemHeight;
            entry.UpdateRepresentation = UpdateButtonDisplay;
            entry.extraData = Catalog.GetKeyAssetDisplays(property.FindPropertyRelative("keyAssetName").stringValue);
            entry.SerializedProperty = property;


            entry.Button.clickable.clicked += () => OnNamedRuleGroupButtonClicked?.Invoke(entry);
        }

        private void UpdateButtonDisplay(CollectionButtonEntry buttonEntry)
        {
            if (buttonEntry.SerializedProperty == null)
                return;

            buttonEntry.SerializedProperty.serializedObject.ApplyModifiedProperties();
            buttonEntry.extraData = Catalog.GetKeyAssetDisplays(buttonEntry.SerializedProperty.FindPropertyRelative("keyAssetName").stringValue);
            if(buttonEntry.extraData == null)
            {
                buttonEntry.HelpBox.SetDisplay(true);
                buttonEntry.HelpBox.messageType = MessageType.Warning;
                buttonEntry.HelpBox.message = "This Entry doesnt have a KeyAsset set.";
                buttonEntry.Button.text = "Invalid Rule Group";
            }
            else
            {
                buttonEntry.HelpBox.SetDisplay(false);
                buttonEntry.HelpBox.messageType = MessageType.None;
                buttonEntry.HelpBox.message = string.Empty;
                buttonEntry.Button.text = buttonEntry.SerializedProperty.FindPropertyRelative("keyAssetName").stringValue;
            }
        }

        private void SortEntries()
        {
            if (_serializedObject == null)
                return;

            if (!(_serializedObject.targetObject is NamedIDRS))
                return;

            NamedIDRS namedIDRS = (NamedIDRS)_serializedObject.targetObject;
            Undo.RecordObject(namedIDRS, "Sort Entries By Name");

            namedIDRS.namedRuleGroups = namedIDRS.namedRuleGroups.OrderBy(entry => entry.keyAssetName).ToList();

            _serializedObject.ApplyAndUpdate();
            ExtendedListView.Refresh();
        }
        private void AddAllEquipments()
        {
            if (_serializedObject == null)
                return;

            if (!(_serializedObject.targetObject is NamedIDRS))
                return;

            NamedIDRS namedIDRS = (NamedIDRS)_serializedObject.targetObject;
            Undo.RecordObject(namedIDRS, "Add Non-Elite Equipments");

            AddMissing(namedIDRS, Catalog.EquipmentToDisplayPrefabs);
        }

        private void AddAllItems()
        {
            if (_serializedObject == null)
                return;

            if (!(_serializedObject.targetObject is NamedIDRS))
                return;

            NamedIDRS namedIDRS = (NamedIDRS)_serializedObject.targetObject;
            Undo.RecordObject(namedIDRS, "Add Items");

            AddMissing(namedIDRS, Catalog.ItemToDisplayPrefabs);
        }

        private void AddEliteEquipments()
        {
            if (_serializedObject == null)
                return;

            if (!(_serializedObject.targetObject is NamedIDRS))
                return;

            NamedIDRS namedIDRS = (NamedIDRS)_serializedObject.targetObject;
            Undo.RecordObject(namedIDRS, "Add Elite Equipments");

            AddMissing(namedIDRS, Catalog.EliteEquipmentToDisplayPrefabs);
        }

        private void AddMissingEntries()
        {
            if (_serializedObject == null)
                return;

            if (!(_serializedObject.targetObject is NamedIDRS))
                return;

            NamedIDRS namedIDRS = (NamedIDRS)_serializedObject.targetObject;
            Undo.RecordObject(namedIDRS, "Add Missing Entries");

            var finalDict = Catalog.EquipmentToDisplayPrefabs.Concat(Catalog.ItemToDisplayPrefabs).Concat
                (Catalog.EliteEquipmentToDisplayPrefabs).ToDictionary(k => k.Key, v => v.Value);
            AddMissing(namedIDRS, new ReadOnlyDictionary<string, ReadOnlyCollection<string>>(finalDict));
        }

        private void AddMissing(NamedIDRS target, ReadOnlyDictionary<string, ReadOnlyCollection<string>> dict)
        {
            foreach(var (keyAsset, displayPrefabs) in dict)
            {
                if (target.namedRuleGroups.Any(x => x.keyAssetName == keyAsset) || displayPrefabs.Count == 0)
                    continue;

                var newEntry = new NamedIDRS.AddressNamedRuleGroup();
                newEntry.keyAssetName = keyAsset;
                newEntry.AddRule(new NamedIDRS.AddressNamedDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    displayPrefabName = displayPrefabs[0],
                    childName = string.Empty,
                    localPos = Vector3.zero,
                    localAngles = Vector3.zero,
                    localScales = Vector3.zero,
                    limbMask = LimbFlags.None
                });
                target.namedRuleGroups.Add(newEntry);
            }
            _serializedObject.ApplyAndUpdate();
            ExtendedListView.Refresh();
        }
        private void OnAttach(AttachToPanelEvent evt)
        {
            ListView listView = ExtendedListView.Q<ListView>();
            listView.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            ExtendedListView.CreateElement = CreateButton;
            ExtendedListView.BindElement = BindButton;

            ForceCatalogUpdate.clicked += () => OnForceCatalogUpdate?.Invoke();
            SortByName.clicked += SortEntries;
            AddAllEquipmentsButton.clicked += AddAllEquipments;
            AddAllItemsButton.clicked += AddAllItems;
            AddOnlyEliteEquipmentsButton.clicked += AddEliteEquipments;
            AddMissingEntriesButton.clicked += AddMissingEntries;
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {

        }

        public NamedIDRS_NamedRuleGroupList()
        {
            TemplateHelpers.GetTemplateInstance(nameof(NamedIDRS_NamedRuleGroupList), this, (pth) => true);
            ExtendedListView = this.Q<ExtendedListView>();
            ForceCatalogUpdate = this.Q<Button>("ForceUpdateCatalog");
            SortByName = this.Q<Button>("SortByNameButton");
            AddAllEquipmentsButton = this.Q<Button>("AddAllEquipments");
            AddAllItemsButton = this.Q<Button>("AddAllItems");
            AddOnlyEliteEquipmentsButton = this.Q<Button>("AddOnlyEliteEquipments");
            AddMissingEntriesButton = this.Q<Button>("AddMissingEntries");

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        ~NamedIDRS_NamedRuleGroupList()
        {
            UnregisterCallback<AttachToPanelEvent>(OnAttach);
            UnregisterCallback<DetachFromPanelEvent>(OnDetach);
        }
    }
}
