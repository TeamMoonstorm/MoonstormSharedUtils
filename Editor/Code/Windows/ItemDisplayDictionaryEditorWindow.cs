using MSU.Editor.VisualElements;
using R2API.Utils;
using RoR2EditorKit;
using System;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MSU.Editor.EditorWindows
{
    public class ItemDisplayDictionaryEditorWindow : MSObjectEditingEditorWindow<ItemDisplayDictionary>
    {
        private ItemDisplayDictionary_KeyAssetField keyAssetField;
        private ItemDisplayDictionary_NamedDisplayDictionaryList namedDisplayDictionaryList;
        private ItemDisplayDictionary_NamedDisplayDictionary namedDisplayDictionary;
        private ItemDisplayDictionary_DisplayRule displayRule;

        private ItemDisplayCatalog catalog;
        [SerializeField]
        private string serializedObjectGUID;

        private void OnEnable()
        {
            Selection.selectionChanged += CheckForItemDisplayDictionary;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Selection.selectionChanged -= CheckForItemDisplayDictionary;
        }

        private void CheckForItemDisplayDictionary()
        {
            var obj = Selection.activeObject;

            if (!obj)
                return;

            UnityEngine.Object currentTarget = null;
            if (SerializedObject != null)
            {
                var ptr = SerializedObject.GetFieldValue<IntPtr>("m_NativeObjectPtr");
                if (((int)ptr) == 0x0)
                {
                    SerializedObject = null;
                    SetSerializedObject(null);
                    return;
                }
                currentTarget = SerializedObject.targetObject;
            }

            if (currentTarget == obj)
                return;

            if (obj is ItemDisplayDictionary idd)
            {
                SerializedObject?.ApplyModifiedProperties();
                SetSerializedObject(idd);
                serializedObjectGUID = AssetDatabaseUtils.GetGUIDFromAsset(obj);
                return;
            }
            else if (!serializedObjectGUID.IsNullOrEmptyOrWhitespace())
            {
                SetSerializedObject(AssetDatabaseUtils.LoadAssetFromGUID<ItemDisplayDictionary>(serializedObjectGUID));
                return;
            }
            SetSerializedObject(null);

            void SetSerializedObject(ItemDisplayDictionary dictionary)
            {
                var so = dictionary ? new SerializedObject(dictionary) : null;
                OnKeyAssetSet(TargetType.AsValidOrNull()?.keyAsset as ScriptableObject);
                keyAssetField.CheckForIDD(so);
                namedDisplayDictionaryList.CheckForIDD(so);
                namedDisplayDictionary.CheckForIDD(so);
                displayRule.CheckForIDD(so);
                SerializedObject = so;
            }
        }

        protected override void CreateGUI()
        {
            base.CreateGUI();
            catalog = ItemDisplayCatalog.LoadCatalog();
            if (catalog == null)
            {
                Close();
                return;
            }

            keyAssetField = rootVisualElement.Q<ItemDisplayDictionary_KeyAssetField>();
            namedDisplayDictionaryList = rootVisualElement.Q<ItemDisplayDictionary_NamedDisplayDictionaryList>();
            namedDisplayDictionary = rootVisualElement.Q<ItemDisplayDictionary_NamedDisplayDictionary>();
            displayRule = rootVisualElement.Q<ItemDisplayDictionary_DisplayRule>();

            CheckForItemDisplayDictionary();
            OnKeyAssetSet(TargetType.AsValidOrNull()?.keyAsset as ScriptableObject);

            keyAssetField.OnKeyAssetValueSet += OnKeyAssetSet;
            namedDisplayDictionaryList.Catalog = catalog;
            namedDisplayDictionaryList.OnForceCatalogUpdate += UpdateCatalog;
            namedDisplayDictionaryList.OnNamedDisplayDictionaryButtonClicked += OnNamedDisplayDictionaryButtonClick;
            namedDisplayDictionaryList.ExtendedListView.collectionSizeField.RegisterValueChangedCallback(OnNamedDisplayDictionaryListSizeChange);
            namedDisplayDictionary.Catalog = catalog;
            namedDisplayDictionary.OnDisplayRuleButtonClicked += OnDisplayRuleButtonClick;
            namedDisplayDictionary.ExtendedListView.collectionSizeField.RegisterValueChangedCallback(OnNamedDisplayDictionarySizeChange);
        }

        private void OnNamedDisplayDictionaryButtonClick(CollectionButtonEntry obj)
        {
            if (namedDisplayDictionary.CurrentEntry == obj)
                return;

            obj.extraData = TargetType.displayPrefabs.Where(x => x).Select(x => x.name).ToArray();
            namedDisplayDictionary.CurrentEntry = obj;
            displayRule.CurrentEntry = null;
        }

        private void OnDisplayRuleButtonClick(CollectionButtonEntry obj)
        {
            obj.extraData = TargetType.displayPrefabs.Where(x => x).Select(x => x.name).ToArray();
            displayRule.CurrentEntry = obj;
        }

        private void OnKeyAssetSet(ScriptableObject scriptableObject)
        {
            namedDisplayDictionaryList.OnKeyAssetFieldValueSet(scriptableObject);
            namedDisplayDictionary.OnKeyAssetFieldValueSet(scriptableObject);
            displayRule.OnKeyAssetFieldValueSet(scriptableObject);
        }
        private void UpdateCatalog()
        {
            catalog = ItemDisplayCatalog.LoadCatalog();
            namedDisplayDictionaryList.Catalog = catalog;
            namedDisplayDictionary.Catalog = catalog;
        }
        private void OnNamedDisplayDictionaryListSizeChange(ChangeEvent<int> evt)
        {
            if (namedDisplayDictionary.CurrentEntry == null)
                return;

            string indexString = namedDisplayDictionary.CurrentEntry.name.Substring("element".Length);
            int index = int.Parse(indexString, CultureInfo.InvariantCulture);
            if (evt.newValue < index || evt.newValue == 0)
            {
                namedDisplayDictionary.CurrentEntry = null;
                displayRule.CurrentEntry = null;
            }
        }
        private void OnNamedDisplayDictionarySizeChange(ChangeEvent<int> evt)
        {
            if (displayRule.CurrentEntry == null)
                return;

            string indexString = displayRule.CurrentEntry.name.Substring("element".Length);
            int index = int.Parse(indexString, CultureInfo.InvariantCulture);
            if (evt.newValue < index || evt.newValue == 0)
            {
                displayRule.CurrentEntry = null;
            }
        }
    }
}
