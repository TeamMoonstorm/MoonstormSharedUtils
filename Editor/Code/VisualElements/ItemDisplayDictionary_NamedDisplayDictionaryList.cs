using RoR2;
using RoR2.Editor;
using RoR2.Editor.VisualElements;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MSU.Editor.VisualElements
{
    public class ItemDisplayDictionary_NamedDisplayDictionaryList : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ItemDisplayDictionary_NamedDisplayDictionaryList, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }
        public ItemDisplayCatalog Catalog { get; internal set; }
        public Button ForceCatalogUpdateButton { get; }
        public Button SortByNameButton { get; }
        public Button AddSurvivorIDRSButton { get; }
        public Button AddEnemyIDRSButton { get; }
        public Button AddBasedOnKeyAssetButton { get; }
        public Button AddMissingIDRSButton { get; }
        public ExtendedListView ExtendedListView { get; }
        public event Action<CollectionButtonEntry> OnNamedDisplayDictionaryButtonClicked;
        public event Action OnForceCatalogUpdate;

        private SerializedObject _serializedObject;
        private ScriptableObject _keyAsset;

        public void OnKeyAssetFieldValueSet(ScriptableObject keyAsset)
        {
            _keyAsset = keyAsset;
            this.SetDisplay(keyAsset);
        }

        public void CheckForIDD(SerializedObject serializedObject)
        {
            _serializedObject = serializedObject;
            if (_serializedObject == null)
            {
                this.SetDisplay(false);
                ExtendedListView.collectionProperty = null;
                return;
            }

            if (_serializedObject.targetObject is ItemDisplayDictionary idd)
            {
                this.SetDisplay(idd.keyAsset);
                ExtendedListView.collectionProperty = serializedObject.FindProperty("displayDictionaryEntries");
            }
        }

        private VisualElement CreateButton()
        {
            return new CollectionButtonEntry();
        }

        private void BindButton(VisualElement element, SerializedProperty property)
        {
            CollectionButtonEntry entry = (CollectionButtonEntry)element;
            entry.style.height = ExtendedListView.listViewItemHeight;
            entry.Button.style.height = ExtendedListView.listViewItemHeight;
            entry.UpdateRepresentation = UpdateButtonDisplay;
            entry.SerializedProperty = property;

            entry.Button.clicked += () => OnNamedDisplayDictionaryButtonClicked?.Invoke(entry);
        }

        private void UpdateButtonDisplay(CollectionButtonEntry entry)
        {
            if (entry.SerializedProperty == null || Catalog == null)
                return;


            entry.SerializedProperty.serializedObject.ApplyModifiedProperties();
            var idrsName = entry.SerializedProperty.FindPropertyRelative("idrsName").stringValue;
            bool idrsNameValid = Catalog.DoesIDRSExist(idrsName);
            entry.HelpBox.SetDisplay(!idrsNameValid);
            entry.HelpBox.messageType = idrsNameValid ? MessageType.None : MessageType.Warning;
            entry.HelpBox.message = idrsNameValid ? string.Empty : "This entry doesnt have an IDRS Set";
            entry.Button.text = idrsNameValid ? idrsName : "Invalid Dictionary Entry";
        }

        private void SortEntries()
        {
            if (_serializedObject == null)
                return;

            if (!(_serializedObject.targetObject is ItemDisplayDictionary))
                return;

            ItemDisplayDictionary idd = (ItemDisplayDictionary)_serializedObject.targetObject;
            Undo.RecordObject(idd, "Sort Entries By Name");

            idd.displayDictionaryEntries = idd.displayDictionaryEntries.OrderBy(entry => entry.idrsName).ToList();

            _serializedObject.ApplyAndUpdate();
            ExtendedListView.Refresh();
        }

        private void AddSurvivorIDRS()
        {
            if (_serializedObject == null)
                return;

            if (!(_serializedObject.targetObject is ItemDisplayDictionary))
                return;

            ItemDisplayDictionary idd = (ItemDisplayDictionary)_serializedObject.targetObject;
            Undo.RecordObject(idd, "Add Survivor IDRS");

            AddMissing(idd, Catalog.SurvivorItemDisplayRuleSets);
        }

        private void AddEnemyIDRS()
        {
            if (_serializedObject == null)
                return;

            if (!(_serializedObject.targetObject is ItemDisplayDictionary))
                return;

            ItemDisplayDictionary idd = (ItemDisplayDictionary)_serializedObject.targetObject;
            Undo.RecordObject(idd, "Add Enemy IDRS");

            AddMissing(idd, Catalog.EnemyItemDisplayRuleSets);
        }

        private void AddIDRSBasedOnKeyAsset()
        {
            if (_serializedObject == null)
                return;

            if (!(_serializedObject.targetObject is ItemDisplayDictionary))
                return;

            ItemDisplayDictionary idd = (ItemDisplayDictionary)_serializedObject.targetObject;
            Undo.RecordObject(idd, "Add IDRS Based On Key Asset");

            var entries = Catalog.SurvivorItemDisplayRuleSets.ToList();
            if (idd.keyAsset is ItemDef id)
            {
                if (!id.ContainsTag(ItemTag.AIBlacklist))
                {
                    entries.Add("idrsScav");
                }
            }
            else if (idd.keyAsset is EquipmentDef ed)
            {
                if (ed.passiveBuffDef && ed.passiveBuffDef.eliteDef)
                {
                    entries.AddRange(Catalog.EnemyItemDisplayRuleSets);
                }
                else
                {
                    entries.Add("idrsScav");
                }
            }
            AddMissing(idd, new ReadOnlyCollection<string>(entries));
        }

        private void AddAllIDRS()
        {
            if (_serializedObject == null)
                return;

            if (!(_serializedObject.targetObject is ItemDisplayDictionary))
                return;

            ItemDisplayDictionary idd = (ItemDisplayDictionary)_serializedObject.targetObject;
            Undo.RecordObject(idd, "Add Missing IDRS");

            AddMissing(idd, new ReadOnlyCollection<string>(Catalog.SurvivorItemDisplayRuleSets.Concat(Catalog.EnemyItemDisplayRuleSets).ToList()));
        }

        private void AddMissing(ItemDisplayDictionary target, ReadOnlyCollection<string> idrsCollection)
        {
            foreach (string idrs in idrsCollection)
            {
                if (target.displayDictionaryEntries.Any(x => x.idrsName == idrs))
                    continue;

                var newEntry = new ItemDisplayDictionary.DisplayDictionaryEntry();
                newEntry.idrsName = idrs;
                newEntry.AddDisplayRule(new ItemDisplayDictionary.DisplayRule
                {
                    ruleType = RoR2.ItemDisplayRuleType.ParentedPrefab,
                    displayPrefabIndex = target.displayPrefabs.Length > 0 ? 0 : -1,
                    childName = string.Empty,
                    localPos = Vector3.zero,
                    localAngles = Vector3.zero,
                    localScale = Vector3.zero,
                    limbMask = RoR2.LimbFlags.None
                });
                target.displayDictionaryEntries.Add(newEntry);
            }
            _serializedObject.ApplyAndUpdate();
            ExtendedListView.Refresh();
        }
        private void ForceCatalogUpdate() => OnForceCatalogUpdate?.Invoke();
        private void OnAttach(AttachToPanelEvent evt)
        {
            ListView listView = ExtendedListView.Q<ListView>();
            listView.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            ExtendedListView.CreateElement = CreateButton;
            ExtendedListView.BindElement = BindButton;

            ForceCatalogUpdateButton.clicked += ForceCatalogUpdate;
            SortByNameButton.clicked += SortEntries;
            AddSurvivorIDRSButton.clicked += AddSurvivorIDRS;
            AddEnemyIDRSButton.clicked += AddEnemyIDRS;
            AddBasedOnKeyAssetButton.clicked += AddIDRSBasedOnKeyAsset;
            AddMissingIDRSButton.clicked += AddAllIDRS;
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            ExtendedListView.CreateElement = null;
            ExtendedListView.BindElement = null;

            ForceCatalogUpdateButton.clicked -= ForceCatalogUpdate;
            SortByNameButton.clicked -= SortEntries;
            AddSurvivorIDRSButton.clicked -= AddSurvivorIDRS;
            AddEnemyIDRSButton.clicked -= AddEnemyIDRS;
            AddBasedOnKeyAssetButton.clicked -= AddIDRSBasedOnKeyAsset;
            AddMissingIDRSButton.clicked -= AddAllIDRS;
        }

        public ItemDisplayDictionary_NamedDisplayDictionaryList()
        {
            TemplateHelpers.GetTemplateInstance(nameof(ItemDisplayDictionary_NamedDisplayDictionaryList), this, (pth) => pth.ValidateUXMLPath());
            ForceCatalogUpdateButton = this.Q<Button>("ForceCatalogReload");
            SortByNameButton = this.Q<Button>("SortByName");
            AddSurvivorIDRSButton = this.Q<Button>("AddSurvivorIDRS");
            AddEnemyIDRSButton = this.Q<Button>("AddEnemyIDRS");
            AddBasedOnKeyAssetButton = this.Q<Button>("AddBasedOnTag");
            AddBasedOnKeyAssetButton.tooltip = $"Adds IDRS Based on the Key Asset and it's values.\n\nIf its an ItemDef: All survivor IDRS are added to the Dictionary, if the item is not AIBlackListed, The Scavenger's IDRS is also added.\n\nIf its an EquipmentDef: All Survivor and the Scavenger's IDRS are added to the Dictionary, if the EquipmentDef is for an Elite, All other Enemies are added to the dictionary.";
            AddMissingIDRSButton = this.Q<Button>("AddMissingIDRS");
            ExtendedListView = this.Q<ExtendedListView>();

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        ~ItemDisplayDictionary_NamedDisplayDictionaryList()
        {
            UnregisterCallback<AttachToPanelEvent>(OnAttach);
            UnregisterCallback<DetachFromPanelEvent>(OnDetach);
        }
    }
}