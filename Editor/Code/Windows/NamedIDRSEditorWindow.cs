using MSU.Editor.VisualElements;
using R2API.Utils;
using RoR2;
using RoR2.Editor;
using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MSU.Editor.EditorWindows
{
    public class NamedIDRSEditorWindow : MSObjectEditingEditorWindow<NamedItemDisplayRuleSet>
    {
        private NamedIDRS_IDRSField namedIDRSField;
        private NamedIDRS_NamedRuleGroupList namedRuleGroupList;
        private NamedIDRS_NamedRuleGroup namedRuleGroup;
        private NamedIDRS_NamedRule namedRule;

        private ItemDisplayCatalog catalog;
        [SerializeField]
        private string serializedObjectGUID;


        protected override void OnEnable()
        {
            base.OnEnable();
            Selection.selectionChanged += CheckForNamedIDRS;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            Selection.selectionChanged -= CheckForNamedIDRS;
        }

        private void CheckForNamedIDRS()
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
            {
                return;
            }

            if (obj is NamedItemDisplayRuleSet namedIDRS)
            {
                serializedObject?.ApplyModifiedProperties();
                SetSerializedObject(namedIDRS);
                serializedObjectGUID = AssetDatabaseUtil.GetAssetGUID(obj);
                return;
            }
            else if (!serializedObjectGUID.IsNullOrEmptyOrWhiteSpace())
            {
                SetSerializedObject(AssetDatabase.LoadAssetAtPath<NamedItemDisplayRuleSet>(AssetDatabase.GUIDToAssetPath(serializedObjectGUID)));
                return;
            }
            SetSerializedObject(null);
            void SetSerializedObject(NamedItemDisplayRuleSet _namedIDRS)
            {
                var so = _namedIDRS ? new SerializedObject(_namedIDRS) : null;
                OnIDRSFieldValueSet(targetType.AsValidOrNull()?.targetItemDisplayRuleSet);
                namedIDRSField.CheckForNamedIDRS(so);
                namedRuleGroupList.CheckForNamedIDRS(so);
                namedRuleGroup.CheckForNamedIDRS(so);
                namedRule.CheckForNamedIDRS(so);
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

            namedIDRSField = rootVisualElement.Q<NamedIDRS_IDRSField>(nameof(NamedIDRS_IDRSField));
            namedRuleGroupList = rootVisualElement.Q<NamedIDRS_NamedRuleGroupList>(nameof(NamedIDRS_NamedRuleGroupList));
            namedRuleGroup = rootVisualElement.Q<NamedIDRS_NamedRuleGroup>(nameof(NamedIDRS_NamedRuleGroup));
            namedRule = rootVisualElement.Q<NamedIDRS_NamedRule>(nameof(NamedIDRS_NamedRule));

            CheckForNamedIDRS();
            OnIDRSFieldValueSet(targetType?.targetItemDisplayRuleSet);
            namedIDRSField.OnIDRSFieldValueSet += OnIDRSFieldValueSet;
            namedRuleGroupList.Catalog = catalog;
            namedRuleGroupList.OnForceCatalogUpdate += UpdateCatalog;
            namedRuleGroupList.OnNamedRuleGroupButtonClicked += OnNamedRuleGroupClicked;
            namedRuleGroupList.ExtendedListView.collectionSizeField.RegisterValueChangedCallback(OnNamedRuleGroupListSizeChanged);
            namedRuleGroupList.Catalog = catalog;
            namedRuleGroup.OnNamedRuleButtonClicked += OnNamedRuleClicked;
            namedRuleGroup.ExtendedListView.collectionSizeField.RegisterValueChangedCallback(OnNamedRuleGroupSizeChanged);
            namedRuleGroup.Catalog = catalog;
            namedRule.Catalog = catalog;
        }

        private void UpdateCatalog()
        {
            catalog = ItemDisplayCatalog.LoadCatalog();
            namedRuleGroupList.Catalog = catalog;
            namedRuleGroup.Catalog = catalog;
            namedRule.Catalog = catalog;
        }

        private void OnNamedRuleClicked(CollectionButtonEntry obj)
        {
            namedRule.CurrentEntry = obj;
        }

        private void OnNamedRuleGroupClicked(CollectionButtonEntry obj)
        {
            if (namedRuleGroup.CurrentEntry == obj)
                return;

            namedRuleGroup.CurrentEntry = obj;
            namedRule.CurrentEntry = null;
        }

        private void OnIDRSFieldValueSet(ItemDisplayRuleSet obj)
        {
            namedRuleGroup.OnIDRSFieldValueSet(obj);
            namedRuleGroupList.OnIDRSFieldValueSet(obj);
            namedRule.OnIDRSFieldValueSet(obj);
        }

        private void OnNamedRuleGroupListSizeChanged(ChangeEvent<int> e)
        {
            if (namedRuleGroup.CurrentEntry == null)
                return;

            string indexString = namedRuleGroup.CurrentEntry.name.Substring("element".Length);
            int index = int.Parse(indexString, CultureInfo.InvariantCulture);
            if (e.newValue < index || e.newValue == 0)
            {
                namedRuleGroup.CurrentEntry = null;
                namedRule.CurrentEntry = null;
            }
        }

        private void OnNamedRuleGroupSizeChanged(ChangeEvent<int> e)
        {
            if (namedRule.CurrentEntry == null)
                return;

            string indexString = namedRule.CurrentEntry.name.Substring("element".Length);
            int index = int.Parse(indexString, CultureInfo.InvariantCulture);
            if (e.newValue < index || e.newValue == 0)
            {
                namedRule.CurrentEntry = null;
            }
        }
    }
}
