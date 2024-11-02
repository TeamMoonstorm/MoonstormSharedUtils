using MSU.Editor.UIElements;
using RoR2.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MSU.Editor.EditorWindows
{
    public class ItemDisplayDictionaryEditorWindow : MSObjectEditingEditorWindow<ItemDisplayDictionary>
    {
        private VisualElement _helpBoxContainer;
        private VisualElement _controlContainer;
        private ObjectField _currentlyInspected;
        private ItemDisplayDictionary_KeyAssetDisplayPrefabsElement _keyAssetDisplayPrefabs;
        private ItemDisplayDictionary_DictionaryEntryPicker _dictionaryEntryPicker;

        private ISerializedObjectBoundCallback[] _boundCallbacks;
        private IItemDisplayCatalogReceiver[] _catalogReceivers;
        private ItemDisplayCatalog _catalog;
        private string _lastEditedIDDGuid;

        protected override void OnEnable()
        {
            base.OnEnable();
            _lastEditedIDDGuid = windowProjectSettings.GetOrCreateSetting(nameof(_lastEditedIDDGuid), string.Empty);

            LoadSerializedObject();

            Selection.selectionChanged += CheckForIDD;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Selection.selectionChanged -= CheckForIDD;
        }

        private void LoadSerializedObject()
        {
            if(_sourceSerializedObject)
            {
                if (_sourceSerializedObject is ItemDisplayDictionary)
                    windowProjectSettings.SetSettingValue(nameof(_lastEditedIDDGuid), AssetDatabaseUtil.GetAssetGUIDString(_sourceSerializedObject));
                else
                    _sourceSerializedObject = AssetDatabaseUtil.LoadAssetFromGUID(_lastEditedIDDGuid);
            }
            else if(!_lastEditedIDDGuid.IsNullOrEmptyOrWhiteSpace())
            {
                _sourceSerializedObject = AssetDatabaseUtil.LoadAssetFromGUID(_lastEditedIDDGuid);
            }
        }

        private void CheckForIDD()
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

            if (obj is ItemDisplayDictionary idd)
            {
                serializedObject?.ApplyModifiedProperties();
                _currentlyInspected.value = idd;
                serializedObject = new SerializedObject(idd);
                _lastEditedIDDGuid = AssetDatabaseUtil.GetAssetGUIDString(idd);
                windowProjectSettings.SetSettingValue(nameof(_lastEditedIDDGuid), _lastEditedIDDGuid);
                return;
            }
            else if (!_lastEditedIDDGuid.IsNullOrEmptyOrWhiteSpace())
            {
                idd = AssetDatabaseUtil.LoadAssetFromGUID<ItemDisplayDictionary>(_lastEditedIDDGuid, null);
                _currentlyInspected.value = idd;
                serializedObject = new SerializedObject(idd);
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
            else if (serializedObject.targetObject is not ItemDisplayDictionary)
            {
                Debug.Log("Target object is not an IDD");
                _helpBoxContainer.SetDisplay(true);
                _controlContainer.SetDisplay(false);
            }
            else
            {
                Debug.Log("Target object is IDD!");
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
            _currentlyInspected.SetObjectType<ItemDisplayDictionary>();
            _currentlyInspected.SetEnabled(false);

            _keyAssetDisplayPrefabs = rootVisualElement.Q<ItemDisplayDictionary_KeyAssetDisplayPrefabsElement>();
            _dictionaryEntryPicker = rootVisualElement.Q<ItemDisplayDictionary_DictionaryEntryPicker>();

            _keyAssetDisplayPrefabs.onDisplayPrefabsChanged += OnDisplayPrefabsChanged;
            _keyAssetDisplayPrefabs.onKeyAssetChanged += OnKeyAssetChanged;

            _dictionaryEntryPicker.onCatalogReloadRequested += ReloadCatalog;
            _dictionaryEntryPicker.buttonListView.itemsRemoved += OnDictionaryEntryRemoved;
            _dictionaryEntryPicker.onDisplayDictionaryEntryButtonClicked += OnDisplayDictionaryEntryButtonClicked;

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

        private void OnDictionaryEntryRemoved(IEnumerable<int> obj)
        {
        }

        private void OnDisplayDictionaryEntryButtonClicked(PropertySelectorButton obj)
        {
        }

        private void ReloadCatalog()
        {
            _catalog = ItemDisplayCatalog.LoadCatalog();
            foreach (var catalogReceiver in _catalogReceivers)
            {
                catalogReceiver.catalog = _catalog;
            }
        }

        private void OnKeyAssetChanged(ScriptableObject obj)
        {
        }

        private void OnDisplayPrefabsChanged(SerializedProperty obj)
        {
        }

        [MenuItem(MSUConstants.MSU_MENU_ROOT + "Windows/Item Display Dictionary Editor Window")]
        private static void Open()
        {
            var selection = Selection.activeObject;
            UnityEngine.Object obj = null;
            if (selection is ItemDisplayDictionary)
            {
                obj = selection;
            }

            Open<ItemDisplayDictionaryEditorWindow>(obj).SetSourceObject();
        }
    }
}
