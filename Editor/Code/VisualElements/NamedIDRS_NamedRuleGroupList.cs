using Moonstorm.AddressableAssets;
using RoR2;
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
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.VisualElements
{
    public class NamedIDRS_NamedRuleGroupList : VisualElement
    {
        public const string COMMANDO_IDRS = "RoR2/Base/Commando/idrsCommando.asset";
        public new class UxmlFactory : UxmlFactory<NamedIDRS_NamedRuleGroupList, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        public Button AddAllEquipmentsButton { get; }
        public Button AddAllItemsButton { get; }
        public Button AddOnlyEliteEquipmentsButton { get; }
        public Button AddMissingEntriesButton { get; }
        public ExtendedListView ExtendedListView { get; }
        public event Action<CollectionButtonEntry> OnNamedRuleGroupButtonClicked;

        private VisualElement _buttonContainer;
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
            entry.SerializedProperty = property;
            entry.Button.clickable.clicked += () => OnNamedRuleGroupButtonClicked?.Invoke(entry);
        }

        private void UpdateButtonDisplay(CollectionButtonEntry buttonEntry)
        {
            if (buttonEntry.SerializedProperty == null)
                return;

            buttonEntry.SerializedProperty.serializedObject.ApplyModifiedProperties();
            var addressableKeyAsset = buttonEntry.SerializedProperty.FindPropertyRelative("keyAsset");
            if (addressableKeyAsset.FindPropertyRelative("useDirectReference").boolValue)
            {
                var asset = addressableKeyAsset.FindPropertyRelative("asset");
                if (asset.objectReferenceValue)
                {
                    buttonEntry.HelpBox.SetDisplay(false);
                    buttonEntry.Button.text = asset.objectReferenceValue.name;
                }
                else
                {
                    buttonEntry.HelpBox.SetDisplay(true);
                    buttonEntry.HelpBox.messageType = MessageType.Warning;
                    buttonEntry.HelpBox.message = "Key Asset Not Set";
                    buttonEntry.Button.text = "Invalid Entry";
                }
            }
            else
            {
                var address = addressableKeyAsset.FindPropertyRelative("address").stringValue;
                var loadAssetFrom = (AddressableKeyAsset.KeyAssetAddressType)addressableKeyAsset.FindPropertyRelative("loadAssetFrom").enumValueIndex;
                if (address.IsNullOrEmptyOrWhitespace())
                {
                    buttonEntry.HelpBox.SetDisplay(true);
                    buttonEntry.HelpBox.messageType = MessageType.Warning;
                    buttonEntry.HelpBox.message = "Key Asset Not Set";
                    buttonEntry.Button.text = "Invalid Entry";
                    return;
                }
                else
                {
                    buttonEntry.HelpBox.SetDisplay(false);
                    if (loadAssetFrom == AddressableKeyAsset.KeyAssetAddressType.Addressables)
                    {
                        string[] split = address.Split('/');
                        buttonEntry.Button.text = split[split.Length - 1];
                        return;
                    }
                    buttonEntry.Button.text = address;
                }
            }
        }

        private async void AddAllEquipments()
        {
            if (_serializedObject == null)
                return;

            var commandoIDRS = await AddressablesUtils.LoadAssetFromCatalog<ItemDisplayRuleSet>(COMMANDO_IDRS);
            NamedIDRS namedIDRS = (NamedIDRS)_serializedObject.targetObject;
            var catalog = await Addressables.LoadContentCatalogAsync(System.IO.Path.GetFullPath("Assets/StreamingAssets/aa/catalog.json")).Task;
            Undo.RecordObject(namedIDRS, "Add All Equipments");
            for(int i = 0; i < commandoIDRS.keyAssetRuleGroups.Length; i++)
            {
                var keyAssetRuleGroup = commandoIDRS.keyAssetRuleGroups[i];
                if(keyAssetRuleGroup.keyAsset is EquipmentDef ed)
                {
                    if (ed.passiveBuffDef.AsValidOrNull()?.eliteDef)
                        continue;

                    string keyAssetName = ed.name;
                    if (!namedIDRS.namedRuleGroups.Any(x => x.keyAsset.address == keyAssetName))
                    {
                        var addressNamedRuleGroup = new NamedIDRS.AddressNamedRuleGroup
                        {
                            keyAsset = new AddressableKeyAsset(keyAssetName, AddressableKeyAsset.KeyAssetAddressType.EquipmentCatalog),
                            rules = new List<NamedIDRS.AddressNamedDisplayRule>()
                        };

                        foreach(var rule in keyAssetRuleGroup.displayRuleGroup.rules)
                        {

                        }
                        namedIDRS.namedRuleGroups.Add(new NamedIDRS.AddressNamedRuleGroup
                        {
                            keyAsset = new AddressableKeyAsset(keyAssetName, AddressableKeyAsset.KeyAssetAddressType.EquipmentCatalog)
                        });
                    }
                }
            }
        }

        private void AddAllItems()
        {
            if (_serializedObject == null)
                return;
        }

        private void AddOnlyEliteEquipments()
        {
            if (_serializedObject == null)
                return;

            var cancer = new Cancer();
            try
            {
                Addressables.AddResourceLocator(cancer);
                var cat = Addressables.LoadContentCatalogAsync(System.IO.Path.GetFullPath("Assets/StreamingAssets/aa/catalog.json")).WaitForCompletion();
                var str = FindDisplayPrefabKey(cat, "DisplayPotion");
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                Addressables.RemoveResourceLocator(cancer);
            }
        }

        private void AddMissingEntries()
        {
            if (_serializedObject == null)
                return;
        }

        private string FindDisplayPrefabKey(IResourceLocator locator, string prefab)
        {
            string key = (string)locator.Keys.FirstOrDefault(obj =>
            {
                if (obj is string address)
                {
                    return address.Contains(prefab);
                }
                return false;
            });
            return key;
        }
        private void OnAttach(AttachToPanelEvent evt)
        {
            ListView listView = ExtendedListView.Q<ListView>();
            listView.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            ExtendedListView.CreateElement = CreateButton;
            ExtendedListView.BindElement = BindButton;

            AddAllEquipmentsButton.clicked += AddAllEquipments;
            AddAllItemsButton.clicked += AddAllItems;
            AddOnlyEliteEquipmentsButton.clicked += AddOnlyEliteEquipments;
            AddMissingEntriesButton.clicked += AddMissingEntries;
            _buttonContainer.SetDisplay(AddressablesUtils.AddressableCatalogExists);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {

        }

        public NamedIDRS_NamedRuleGroupList()
        {
            TemplateHelpers.GetTemplateInstance(nameof(NamedIDRS_NamedRuleGroupList), this, (pth) => true);
            ExtendedListView = this.Q<ExtendedListView>();
            AddAllEquipmentsButton = this.Q<Button>("AddAllEquipments");
            AddAllItemsButton = this.Q<Button>("AddAllItems");
            AddOnlyEliteEquipmentsButton = this.Q<Button>("AddOnlyEliteEquipments");
            AddMissingEntriesButton = this.Q<Button>("AddMissingEntries");
            _buttonContainer = this.Q<VisualElement>("Buttons");

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
