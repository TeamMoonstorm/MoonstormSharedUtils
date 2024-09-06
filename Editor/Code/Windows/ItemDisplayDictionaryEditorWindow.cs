using MSU.Editor.VisualElements;
using R2API.Utils;
using RoR2.Editor;
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

        protected override void OnEnable()
        {
            base.OnEnable();
            serializedObject = null;
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
            if (serializedObject != null)
            {
                var ptr = serializedObject.GetFieldValue<IntPtr>("m_NativeObjectPtr");
                if (((int)ptr) == 0x0)
                {
                    serializedObject = null;
                    SetSerializedObject(null);
                    return;
                }
                currentTarget = serializedObject.targetObject;
            }

            if (currentTarget == obj)
                return;

            if (obj is ItemDisplayDictionary idd)
            {
                serializedObject?.ApplyModifiedProperties();
                SetSerializedObject(idd);
                serializedObjectGUID = AssetDatabaseUtil.GetAssetGUIDString(obj);
                return;
            }
            else if (!serializedObjectGUID.IsNullOrEmptyOrWhiteSpace())
            {
                SetSerializedObject(AssetDatabaseUtil.LoadAssetFromGUID<ItemDisplayDictionary>(serializedObjectGUID));
                return;
            }
            SetSerializedObject(null);

            void SetSerializedObject(ItemDisplayDictionary dictionary)
            {
                var so = dictionary ? new SerializedObject(dictionary) : null;
                OnKeyAssetSet(targetType.AsValidOrNull()?.keyAsset as ScriptableObject);
                keyAssetField.CheckForIDD(so);
                namedDisplayDictionaryList.CheckForIDD(so);
                namedDisplayDictionary.CheckForIDD(so);
                displayRule.CheckForIDD(so);
                serializedObject = so;
            }
        }

        protected override void FinalizeUI()
        {
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
            OnKeyAssetSet(targetType.AsValidOrNull()?.keyAsset as ScriptableObject);

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

            obj.extraData = targetType.displayPrefabs.Where(x => x).Select(x => x.name).ToArray();
            namedDisplayDictionary.CurrentEntry = obj;
            displayRule.CurrentEntry = null;
        }

        private void OnDisplayRuleButtonClick(CollectionButtonEntry obj)
        {
            obj.extraData = targetType.displayPrefabs.Where(x => x).Select(x => x.name).ToArray();
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
