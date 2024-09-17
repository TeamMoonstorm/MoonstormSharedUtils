using MSU.Editor.UIElements;
using RoR2.Editor;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MSU.Editor.EditorWindows
{
    public class NamedItemDisplayRuleSetEditorWindow : MSObjectEditingEditorWindow<NamedItemDisplayRuleSet>
    {
        private VisualElement _helpBoxContainer;
        private VisualElement _controlContainer;
        private ObjectField _currentlyInspected;
        private NamedItemDisplayRuleSet_TargetRuleSet _targetRuleSet;
        private NamedItemDisplayRuleSet_RuleGroupSelector _ruleGroupSelector;
        private NamedItemDisplayRuleSet_KeyAssetRuleSelector _keyAssetRuleSelector;
        private NamedItemDisplayRuleSet_RuleEditor _ruleEditor;

        private ISerializedObjectBoundCallback[] _boundCallbacks;
        private IItemDisplayCatalogReceiver[] _catalogReceivers;

        private ItemDisplayCatalog _catalog;

        private string _lastEditedNIDRSGUID;
        protected override void OnEnable()
        {
            base.OnEnable();
            _lastEditedNIDRSGUID = windowProjectSettings.GetOrCreateSetting(nameof(_lastEditedNIDRSGUID), string.Empty);

            LoadSerializedObject();

            Selection.selectionChanged += CheckForNamedIDRS;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Selection.selectionChanged -= CheckForNamedIDRS;
        }


        private void LoadSerializedObject()
        {
            if (_sourceSerializedObject)
            {
                if (_sourceSerializedObject is NamedItemDisplayRuleSet)
                    windowProjectSettings.SetSettingValue(nameof(_lastEditedNIDRSGUID), AssetDatabaseUtil.GetAssetGUIDString(_sourceSerializedObject));
                else
                    _sourceSerializedObject = AssetDatabaseUtil.LoadAssetFromGUID(_lastEditedNIDRSGUID);
            }
            else if (!_lastEditedNIDRSGUID.IsNullOrEmptyOrWhiteSpace())
            {
                _sourceSerializedObject = AssetDatabaseUtil.LoadAssetFromGUID(_lastEditedNIDRSGUID);
            }
        }

        private void CheckForNamedIDRS()
        {
            var obj = Selection.activeObject;
            if (!obj)
                return;

            UnityEngine.Object currentTarget = null;
            if (serializedObject != null)
            {
                var ptr = (IntPtr)typeof(SerializedObject).GetField("m_NativeObjectPtr", ReflectionUtils.all).GetValue(serializedObject);
                if (((int)ptr) == 0x0)
                {
                    serializedObject = null;
                    return;
                }
                currentTarget = serializedObject.targetObject;
            }

            if (currentTarget == obj)
            {
                return;
            }

            if (obj is NamedItemDisplayRuleSet nidrs)
            {
                serializedObject?.ApplyModifiedProperties();
                _currentlyInspected.value = nidrs;
                serializedObject = new SerializedObject(nidrs);
                _lastEditedNIDRSGUID = AssetDatabaseUtil.GetAssetGUIDString(nidrs);
                windowProjectSettings.SetSettingValue(nameof(_lastEditedNIDRSGUID), _lastEditedNIDRSGUID);
                return;
            }
            else if (!_lastEditedNIDRSGUID.IsNullOrEmptyOrWhiteSpace())
            {
                nidrs = AssetDatabaseUtil.LoadAssetFromGUID<NamedItemDisplayRuleSet>(_lastEditedNIDRSGUID, null);
                _currentlyInspected.value = nidrs;
                serializedObject = new SerializedObject(nidrs);
                return;
            }
            serializedObject = null;
        }

        protected override void OnSerializedObjectChanged()
        {
            base.OnSerializedObjectChanged();
            rootVisualElement.Unbind();
            if (serializedObject != null)
            {
                rootVisualElement.Bind(serializedObject);
            }
            foreach (var callback in _boundCallbacks)
            {
                try
                {
                    callback.OnBoundSerializedObjectChange(serializedObject);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            if (serializedObject == null)
            {
                Debug.Log("No Serialized Object");
                _helpBoxContainer.SetDisplay(true);
                _controlContainer.SetDisplay(false);
            }
            else if (serializedObject.targetObject is not NamedItemDisplayRuleSet)
            {
                Debug.Log("Target object is not an NIDRS");
                _helpBoxContainer.SetDisplay(true);
                _controlContainer.SetDisplay(false);
            }
            else
            {
                Debug.Log("Target object is NIDRS!");
                _helpBoxContainer.SetDisplay(false);
                _controlContainer.SetDisplay(true);
            }
        }

        protected override void FinalizeUI()
        {
            _catalog = ItemDisplayCatalog.LoadCatalog();
            if (_catalog == null)
            {
                Close();
                return;
            }

            _helpBoxContainer = rootVisualElement.Q<VisualElement>("NoValidSerializedObjectContainer");
            _controlContainer = rootVisualElement.Q<VisualElement>("Controls");
            _currentlyInspected = rootVisualElement.Q<ObjectField>("CurrentlyEditing");
            _currentlyInspected.SetObjectType<NamedItemDisplayRuleSet>();
            _currentlyInspected.SetEnabled(false);

            _targetRuleSet = rootVisualElement.Q<NamedItemDisplayRuleSet_TargetRuleSet>();
            _ruleGroupSelector = rootVisualElement.Q<NamedItemDisplayRuleSet_RuleGroupSelector>();
            _keyAssetRuleSelector = rootVisualElement.Q<NamedItemDisplayRuleSet_KeyAssetRuleSelector>();
            _ruleEditor = rootVisualElement.Q<NamedItemDisplayRuleSet_RuleEditor>();

            _targetRuleSet.onTargetIDRSChanged += OnTargetIDRSChanged;

            _ruleGroupSelector.onCatalogReloadRequessted += ReloadCatalog;
            _ruleGroupSelector.buttonListView.itemsRemoved += OnRuleGroupsRemoved;
            _ruleGroupSelector.onNamedRuleButtonClicked += OnRuleGroupButtonClicked;

            _keyAssetRuleSelector.buttonListView.itemsRemoved += OnRulesRemoved;
            _keyAssetRuleSelector.onNamedRuleButtonClicked += OnRuleButtonClicked;

            _boundCallbacks = rootVisualElement.Query()
                .Where(ve => ve is ISerializedObjectBoundCallback)
                .Build()
                .Select(ve => (ISerializedObjectBoundCallback)ve)
                .ToArray();

            _catalogReceivers = rootVisualElement.Query()
                .Where(ve => ve is IItemDisplayCatalogReceiver)
                .Build()
                .Select(ve => (IItemDisplayCatalogReceiver)ve)
                .ToArray();

            foreach (var catalogReceiver in _catalogReceivers)
            {
                catalogReceiver.catalog = _catalog;
            }

            if (_sourceSerializedObject == null || _sourceSerializedObject is EditorWindow)
                LoadSerializedObject();

            if (_sourceSerializedObject && _sourceSerializedObject is not EditorWindow && serializedObject == null)
            {
                serializedObject = new SerializedObject(_sourceSerializedObject);
            }
            else
            {
                OnSerializedObjectChanged();
            }
        }

        private void OnRuleButtonClicked(PropertySelectorButton button)
        {
            if (_ruleEditor.currentlyInspectedEntry == button)
                return;

            _ruleEditor.currentlyInspectedEntry = button;
        }

        private void OnRulesRemoved(System.Collections.Generic.IEnumerable<int> obj)
        {
            if (_ruleEditor.currentlyInspectedEntry == null)
                return;

            if (obj.Contains(_ruleEditor.currentlyInspectedEntry.index))
            {
                _ruleEditor.currentlyInspectedEntry = null;
            }
        }

        private void OnRuleGroupsRemoved(System.Collections.Generic.IEnumerable<int> obj)
        {
            if (_keyAssetRuleSelector.currentlyInspectedEntry == null)
                return;

            if (obj.Contains(_keyAssetRuleSelector.currentlyInspectedEntry.index))
            {
                _keyAssetRuleSelector.currentlyInspectedEntry = null;
                _ruleEditor.currentlyInspectedEntry = null;
            }
        }

        private void OnRuleGroupButtonClicked(PropertySelectorButton obj)
        {
            if (_keyAssetRuleSelector.currentlyInspectedEntry == obj)
                return;

            _keyAssetRuleSelector.currentlyInspectedEntry = obj;
            _ruleEditor.currentlyInspectedEntry = null;
        }

        private void ReloadCatalog()
        {
            _catalog = ItemDisplayCatalog.LoadCatalog();
            foreach (var catalogReceiver in _catalogReceivers)
            {
                catalogReceiver.catalog = _catalog;
            }
        }

        private void OnTargetIDRSChanged(RoR2.ItemDisplayRuleSet obj)
        {
            _ruleGroupSelector.SetEnabled(obj);
            _keyAssetRuleSelector.SetEnabled(obj);
            _ruleEditor.SetEnabled(obj);
        }

        [MenuItem(MSUConstants.MSU_MENU_ROOT + "Windows/Named Item Display Rule Set Editor Window")]
        private static void Open()
        {
            var selection = Selection.activeObject;
            UnityEngine.Object obj = null;
            if (selection is NamedItemDisplayRuleSet)
            {
                obj = selection;
            }

            Open<NamedItemDisplayRuleSetEditorWindow>(obj);
        }
    }
}