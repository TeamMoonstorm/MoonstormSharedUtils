using Moonstorm;
using RoR2.Editor;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MSU.Editor.UIElements
{
    public class NamedItemDisplayRuleSet_RuleGroupSelector : VisualElement, ISerializedObjectBoundCallback, IItemDisplayCatalogReceiver
    {
        public Button updateCatalog { get; }
        public Button sortByName { get; }
        public Button addAllItems { get; }
        public Button addAllEquipments { get; }
        public Button addOnlyEliteEquipments { get; }
        public Button addMissing { get; }
        public ListView buttonListView { get; }

        public SerializedProperty targetProperty { get; set; }
        public ItemDisplayCatalog catalog { get; set; }

        public event Action<PropertySelectorButton> onNamedRuleButtonClicked;
        public event Action onCatalogReloadRequessted;
        public void OnBoundSerializedObjectChange(SerializedObject so)
        {
            if (so == null)
            {
                targetProperty = null;
                buttonListView.Unbind();
                return;
            }
            var ptr = (IntPtr)typeof(SerializedObject).GetField("m_NativeObjectPtr", ReflectionUtils.all).GetValue(so);
            if ((int)ptr == 0x0)
                return;

            targetProperty = so.FindProperty("rules");
            buttonListView.BindProperty(targetProperty);
        }

        private VisualElement CreateButtonContainer()
        {
            return new PropertySelectorButton();
        }

        private void BindButtonContainer(VisualElement ve, int index)
        {
            if (index == -1 || index >= targetProperty.arraySize)
                return;

            var property = targetProperty.GetArrayElementAtIndex(index);
            PropertySelectorButton propertySelector = (PropertySelectorButton)ve;
            propertySelector.name = "element " + index;
            propertySelector.index = index;
            propertySelector.style.height = buttonListView.fixedItemHeight;
            propertySelector.style.maxHeight = buttonListView.fixedItemHeight;
            propertySelector.button.style.flexGrow = 1;

            propertySelector.button.clicked += () => onNamedRuleButtonClicked?.Invoke(propertySelector);
            propertySelector.updateRepresentation = UpdateButtonDisplay;
            propertySelector.extraData = catalog.GetKeyAssetDisplays(property.FindPropertyRelative("keyAssetName").stringValue);
            propertySelector.helpBox.messageType = MessageType.Warning;
            propertySelector.helpBox.message = "This Entry doesnt have a KeyAsset set.";
            propertySelector.representingProperty = property;
        }

        private void UpdateButtonDisplay(PropertySelectorButton instance)
        {
            var newProperty = instance.representingProperty;
            if (newProperty == null)
                return;

            newProperty.serializedObject.ApplyModifiedProperties();
            string keyAssetName = newProperty.FindPropertyRelative("keyAssetName").stringValue;
            instance.extraData = catalog.GetKeyAssetDisplays(keyAssetName);
            if(instance.extraData == null)
            {
                instance.helpBox.SetDisplay(true);
                instance.button.text = "Invalid Rule Group";
            }
            else
            {
                instance.helpBox.SetDisplay(false);
                instance.button.text = keyAssetName;
            }
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            buttonListView.makeItem = null;
            buttonListView.bindItem = null;

            updateCatalog.clicked -= UpdateCatalog;
            sortByName.clicked -= SortEntries;
            addAllItems.clicked -= AddAllItems;
            addAllEquipments.clicked -= AddEquipments;
            addOnlyEliteEquipments.clicked -= AddElites;
            addMissing.clicked -= AddMissing;
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            buttonListView.makeItem = CreateButtonContainer;
            buttonListView.bindItem = BindButtonContainer;

            updateCatalog.clicked += UpdateCatalog;
            sortByName.clicked += SortEntries;
            addAllItems.clicked += AddAllItems;
            addAllEquipments.clicked += AddEquipments;
            addOnlyEliteEquipments.clicked += AddElites;
            addMissing.clicked += AddMissing;
        }

        private void AddAllItems()
        {
            if (targetProperty == null)
                return;

            var serializedObject = targetProperty.serializedObject;

            if (serializedObject.targetObject is not NamedItemDisplayRuleSet nidrs)
                return;

            Undo.RecordObject(nidrs, "Add all Items");

            AddMissing(nidrs, catalog.itemToDisplayPrefabs);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            buttonListView.Rebuild();
        }


        private void AddElites()
        {
            if (targetProperty == null)
                return;

            var serializedObject = targetProperty.serializedObject;

            if (serializedObject.targetObject is not NamedItemDisplayRuleSet nidrs)
                return;

            Undo.RecordObject(nidrs, "Add All Elite Equipments");

            AddMissing(nidrs, catalog.eliteEquipmentToDisplayPrefabs);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            buttonListView.Rebuild();
        }

        private void AddEquipments()
        {
            if (targetProperty == null)
                return;

            var serializedObject = targetProperty.serializedObject;

            if (serializedObject.targetObject is not NamedItemDisplayRuleSet nidrs)
                return;

            Undo.RecordObject(nidrs, "Add All Non Elite Equipments");

            AddMissing(nidrs, catalog.equipmentToDisplayPrefabs);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            buttonListView.Rebuild();
        }

        private void SortEntries()
        {
            if (targetProperty == null)
                return;

            var serializedObject = targetProperty.serializedObject;

            if (serializedObject.targetObject is not NamedItemDisplayRuleSet nidrs)
                return;

            Undo.RecordObject(nidrs, "Sort Entries by Name");

            nidrs.rules = nidrs.rules.OrderBy(entry => entry.keyAssetName).ToList();

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            buttonListView.Rebuild();
        }

        private void AddMissing()
        {
            if (targetProperty == null)
                return;

            var serializedObject = targetProperty.serializedObject;

            if (serializedObject.targetObject is not NamedItemDisplayRuleSet nidrs)
                return;

            Undo.RecordObject(nidrs, "Add Missing Entries");

            var finalDict = catalog.equipmentToDisplayPrefabs.Concat(catalog.itemToDisplayPrefabs).Concat(catalog.eliteEquipmentToDisplayPrefabs).ToDictionary(k => k.Key, v => v.Value);

            AddMissing(nidrs, new ReadOnlyDictionary<string, ReadOnlyCollection<string>>(finalDict));
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            buttonListView.Rebuild();
        }

        private void AddMissing(NamedItemDisplayRuleSet target, ReadOnlyDictionary<string, ReadOnlyCollection<string>> dict)
        {
            foreach (var (keyAsset, displayPrefabs) in dict)
            {
                if (target.rules.Any(x => x.keyAssetName == keyAsset) || displayPrefabs.Count == 0)
                    continue;

                var newEntry = new NamedItemDisplayRuleSet.RuleGroup
                {
                    keyAssetName = keyAsset
                };
                newEntry.AddRule(new NamedItemDisplayRuleSet.DisplayRule
                {
                    localAngles = Vector3.zero,
                    localPos = Vector3.zero,
                    localScale = Vector3.one,
                    childName = string.Empty,
                    displayPrefabName = displayPrefabs[0],
                    limbMask = RoR2.LimbFlags.None,
                    ruleType = RoR2.ItemDisplayRuleType.ParentedPrefab
                });
                target.rules.Add(newEntry);
            }
        }

        private void UpdateCatalog()
        {
            onCatalogReloadRequessted?.Invoke();
        }

        public NamedItemDisplayRuleSet_RuleGroupSelector()
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, this, p => p.ValidateUXMLPath());

            updateCatalog = this.Q<Button>("UpdateCatalog");
            sortByName = this.Q<Button>("SortByName");
            addAllItems = this.Q<Button>("AddAllItems");
            addAllEquipments = this.Q<Button>("AddAllEquipments");
            addOnlyEliteEquipments = this.Q<Button>("AddOnlyEliteEquipments");
            addMissing = this.Q<Button>("AddMissing");
            buttonListView = this.Q<ListView>("ButtonListView");

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        new public class UxmlFactory : UxmlFactory<NamedItemDisplayRuleSet_RuleGroupSelector, UxmlTraits> { }
        new public class UxmlTraits : VisualElement.UxmlTraits { }
    }
}