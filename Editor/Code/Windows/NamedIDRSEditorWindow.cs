using Moonstorm.AddressableAssets;
using Moonstorm.EditorUtils.VisualElements;
using RoR2;
using RoR2EditorKit.Inspectors;
using RoR2EditorKit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ThunderKit.Markdown;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public class NamedIDRSEditorWindow : MSObjectEditingEditorWindow<NamedIDRS>
    {
        private NamedIDRS_IDRSField namedIDRSField;
        private NamedIDRS_NamedRuleGroupList namedRuleGroupList;
        private NamedIDRS_NamedRuleGroup namedRuleGroup;
        private NamedIDRS_NamedRule namedRule;

        private ItemDisplayCatalog catalog;
        [SerializeField]
        private string serializedObjectGUID;

        private bool NamedIDRSSelected => SerializedObject?.targetObject is NamedIDRS;

        private void OnEnable()
        {
            Selection.selectionChanged += CheckForNamedIDRS;
            catalog = ItemDisplayCatalog.LoadCatalog();
            if (catalog == null)
            {
                Close();
                return;
            }
        }
        protected override void OnDisable()
        {
            Selection.selectionChanged -= CheckForNamedIDRS;
        }

        private void CheckForNamedIDRS()
        {
            var obj = Selection.activeObject;
            if(obj == null && SerializedObject == null)
            {
                OnIDRSFieldValueSet(null);
                namedIDRSField.CheckForNamedIDRS(null);
                namedRuleGroupList.CheckForNamedIDRS(null);
                namedRuleGroup.CheckForNamedIDRS(null);
                namedRule.CheckForNamedIDRS(null);
                return;
            }

            if(SerializedObject != null && SerializedObject.targetObject == obj)
            {
                return;
            }

            if(obj is NamedIDRS namedIDRS)
            {
                SetSerializedObject(namedIDRS);
                serializedObjectGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(namedIDRS));
            }
            else if(!serializedObjectGUID.IsNullOrEmptyOrWhitespace())
            {
                SetSerializedObject(AssetDatabase.LoadAssetAtPath<NamedIDRS>(AssetDatabase.GUIDToAssetPath(serializedObjectGUID)));
            }

            void SetSerializedObject(NamedIDRS _namedIDRS)
            {
                SerializedObject = new SerializedObject(_namedIDRS);
                OnIDRSFieldValueSet(TargetType.idrs);
                namedIDRSField.CheckForNamedIDRS(SerializedObject);
                namedRuleGroupList.CheckForNamedIDRS(SerializedObject);
                namedRuleGroup.CheckForNamedIDRS(SerializedObject);
                namedRule.CheckForNamedIDRS(SerializedObject);
            }
        }
        protected override void CreateGUI()
        {
            base.CreateGUI();
            namedIDRSField = rootVisualElement.Q<NamedIDRS_IDRSField>(nameof(NamedIDRS_IDRSField));
            namedRuleGroupList = rootVisualElement.Q<NamedIDRS_NamedRuleGroupList>(nameof(NamedIDRS_NamedRuleGroupList));
            namedRuleGroup = rootVisualElement.Q<NamedIDRS_NamedRuleGroup>(nameof(NamedIDRS_NamedRuleGroup));
            namedRule = rootVisualElement.Q<NamedIDRS_NamedRule>(nameof(NamedIDRS_NamedRule));

            CheckForNamedIDRS();
            OnIDRSFieldValueSet(TargetType?.idrs);
            namedIDRSField.OnIDRSFieldValueSet += OnIDRSFieldValueSet;
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
            if(e.newValue < index || e.newValue == 0)
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
