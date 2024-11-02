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
    public class ItemDisplayDictionary_DictionaryEntryPicker : VisualElement, ISerializedObjectBoundCallback, IItemDisplayCatalogReceiver
    {
        public Button updateCatalog { get; }
        public Button sortByName { get; }
        public Button addSurvivorIDRS { get; }
        public Button addEnemyIDRS { get; }
        public Button addMissing { get; }

        public ListView buttonListView { get; }

        public SerializedProperty targetProperty { get; set; }
        public ItemDisplayCatalog catalog { get; set; }

        public event Action<PropertySelectorButton> onDisplayDictionaryEntryButtonClicked;
        public event Action onCatalogReloadRequested;

        public void OnBoundSerializedObjectChange(SerializedObject so)
        {
            if (so == null)
            {
                targetProperty = null;
                buttonListView.Unbind();
            }

            var ptr = (IntPtr)typeof(SerializedObject).GetField("m_NativeObjectPtr", ReflectionUtils.all).GetValue(so);
            if ((int)ptr == 0x0)
                return;

            targetProperty = so.FindProperty("displayDictionaryEntries");
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

            PropertySelectorButton propertySelectorButton = (PropertySelectorButton)ve;
            propertySelectorButton.name = "element " + index;
            propertySelectorButton.index = index;
            propertySelectorButton.style.height = buttonListView.fixedItemHeight;
            propertySelectorButton.style.maxHeight = buttonListView.fixedItemHeight;
            propertySelectorButton.button.style.flexGrow = 1;

            propertySelectorButton.button.clicked += () => onDisplayDictionaryEntryButtonClicked?.Invoke(propertySelectorButton);
            propertySelectorButton.updateRepresentation = UpdateButtonDisplay;
            propertySelectorButton.helpBox.messageType = MessageType.Warning;
            propertySelectorButton.helpBox.message = "This entry doesnt have an IDRS set";
            propertySelectorButton.representingProperty = property;
        }

        private void UpdateButtonDisplay(PropertySelectorButton instance)
        {
            var newProperty = instance.representingProperty;
            if (newProperty == null)
                return;

            newProperty.serializedObject.ApplyModifiedProperties();
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            buttonListView.makeItem = null;
            buttonListView.bindItem = null;

            updateCatalog.clicked -= UpdateCatalog;
            sortByName.clicked -= SortEntries;
            addSurvivorIDRS.clicked -= AddSurvivorIDRS;
            addEnemyIDRS.clicked -= AddEnemyIDRS;
            addMissing.clicked -= AddMissing;
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            buttonListView.makeItem = CreateButtonContainer;
            buttonListView.bindItem = BindButtonContainer;

            updateCatalog.clicked += UpdateCatalog;
            sortByName.clicked += SortEntries;
            addSurvivorIDRS.clicked += AddSurvivorIDRS;
            addEnemyIDRS.clicked += AddEnemyIDRS;
            addMissing.clicked += AddMissing;
        }

        private void SortEntries()
        {
            if (targetProperty == null)
                return;

            var serializedObject = targetProperty.serializedObject;
            if (serializedObject.targetObject is not ItemDisplayDictionary idd)
                return;

            Undo.RecordObject(idd, "Sort Entries by Name");

            idd.displayDictionaryEntries = idd.displayDictionaryEntries.OrderBy(entry => entry.idrsName).ToList();

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            buttonListView.Rebuild();
        }

        private void AddSurvivorIDRS()
        {
            if (targetProperty == null)
                return;

            var serializedObject = targetProperty.serializedObject;
            if (serializedObject.targetObject is not ItemDisplayDictionary idd)
                return;

            Undo.RecordObject(idd, "Add Survivor IDRS");

            AddMissing(idd, catalog.survivorItemDisplayRuleSets);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            buttonListView.Rebuild();
        }

        private void AddEnemyIDRS()
        {
            if (targetProperty == null)
                return;

            var serializedObject = targetProperty.serializedObject;
            if (serializedObject.targetObject is not ItemDisplayDictionary idd)
                return;

            Undo.RecordObject(idd, "Add Enemy IDRS");

            AddMissing(idd, catalog.enemyItemDisplayRuleSets);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            buttonListView.Rebuild();
        }

        private void AddMissing()
        {
            if (targetProperty == null)
                return;

            var serializedObject = targetProperty.serializedObject;
            if (serializedObject.targetObject is not ItemDisplayDictionary idd)
                return;

            Undo.RecordObject(idd, "Add Missing IDRS Entries");

            AddMissing(idd, catalog.allItemDisplayRuleSets);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            buttonListView.Rebuild();
        }

        private void AddMissing(ItemDisplayDictionary target, ReadOnlyCollection<string> missing)
        {
            foreach(var entry in missing)
            {
                if (target.displayDictionaryEntries.Any(x => x.idrsName == entry))
                    return;

                var newEntry = new ItemDisplayDictionary.DisplayDictionaryEntry
                {
                    idrsName = entry
                };
                newEntry.AddDisplayRule(new ItemDisplayDictionary.DisplayRule
                {
                    localAngles = Vector3.zero,
                    localPos = Vector3.zero,
                    localScale = Vector3.one,
                    childName = string.Empty,
                    displayPrefabIndex = 0,
                    limbMask = RoR2.LimbFlags.None,
                    ruleType = RoR2.ItemDisplayRuleType.ParentedPrefab
                });
                target.displayDictionaryEntries.Add(newEntry);
            }
        }

        private void UpdateCatalog()
        {
            onCatalogReloadRequested?.Invoke();
        }

        public ItemDisplayDictionary_DictionaryEntryPicker()
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, this, p => p.ValidateUXMLPath());

            updateCatalog = this.Q<Button>("UpdateCatalog");
            sortByName = this.Q<Button>("SortByName");
            addSurvivorIDRS = this.Q<Button>("AddSurvivorIDRS");
            addEnemyIDRS = this.Q<Button>("AddEnemyIDRS");
            addMissing = this.Q<Button>("AddMissing");
            buttonListView = this.Q<ListView>("ButtonListView");


            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }


        new public class UxmlFactory : UxmlFactory<ItemDisplayDictionary_DictionaryEntryPicker, UxmlTraits> { }
        new public class UxmlTraits : VisualElement.UxmlTraits { }
    }
}