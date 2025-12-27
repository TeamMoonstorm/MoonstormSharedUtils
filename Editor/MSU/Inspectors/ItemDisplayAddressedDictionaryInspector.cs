using RoR2.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MSU.Editor.Inspectors
{
    [CustomEditor(typeof(ItemDisplayAddressedDictionary))]
    public class ItemDisplayAddressedDictionaryInspector : VisualElementScriptableObjectInspector<ItemDisplayAddressedDictionary>
    {
        TextField _idrsFilterText;
        ListView _displayEntryListView;
        Button _forceRefreshButton;

        List<SerializedProperty> _filteredListSelection = new List<SerializedProperty>();
        SerializedProperty _displayEntriesProperty;
        int _previousArraySize = -1;

        protected override void InitializeVisualElement(VisualElement templateInstanceRoot)
        {
            _displayEntriesProperty = serializedObject.FindProperty(nameof(ItemDisplayAddressedDictionary.displayEntries));
            _previousArraySize = _displayEntriesProperty.arraySize;

            //We need to listen for the filter set, so we can dynamically change our list view depending wether we're filtering or not.
            _idrsFilterText = templateInstanceRoot.Q<TextField>("IDRSFilter");
            _idrsFilterText.RegisterValueChangedCallback(OnFilterSet);

            //Setup the list view itself.
            _displayEntryListView = templateInstanceRoot.Q<ListView>("DisplayEntries");
            _displayEntryListView.itemsSource = _filteredListSelection;
            _displayEntryListView.makeItem = CreatePropertyField;
            _displayEntryListView.bindItem = BindPropertyField;
            _displayEntryListView.itemIndexChanged += ElementIndexChanged;
            _displayEntryListView.itemsAdded += ItemsAdded;
            _displayEntryListView.itemsRemoved += ItemsRemoved;


            _forceRefreshButton = templateInstanceRoot.Q<Button>("ForceRefresh");
            _forceRefreshButton.clicked += UpdateListViewToFilter;
            UpdateListViewToFilter();

            //There is no way to listen if someone clicks the "delete array element" context menu of a property field, thanks unity.
            EditorApplication.update += CheckForKeyAssetRuleGroupsPropertyChange;
        }

        private void OnDestroy()
        {
            EditorApplication.update -= CheckForKeyAssetRuleGroupsPropertyChange;
        }

        private void CheckForKeyAssetRuleGroupsPropertyChange()
        {
            try
            {
                if (_previousArraySize != _displayEntriesProperty.arraySize)
                {
                    _previousArraySize = _displayEntriesProperty.arraySize;
                    UpdateListViewToFilter();
                }
            }
            catch (Exception ex)
            {
                EditorApplication.update -= CheckForKeyAssetRuleGroupsPropertyChange;
            }
        }

        //since this only would get called when we're not filtering, we can just be stupid about how we do things.
        private void ItemsRemoved(IEnumerable<int> obj)
        {
            List<int> list = (List<int>)obj;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                _displayEntriesProperty.DeleteArrayElementAtIndex(i);
            }
            _displayEntriesProperty.serializedObject.ApplyModifiedProperties();
            UpdateListViewToFilter();
        }

        private void ItemsAdded(IEnumerable<int> obj)
        {
            int biggest = -1;

            List<int> list = (List<int>)obj;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] > biggest)
                {
                    biggest = list[i];
                }
            }

            if (_displayEntriesProperty.arraySize < biggest + 1)
            {
                _displayEntriesProperty.arraySize = biggest + 1;
                _displayEntriesProperty.serializedObject.ApplyModifiedProperties();
                UpdateListViewToFilter();
            }
        }

        private void ElementIndexChanged(int arg1, int arg2)
        {
            _displayEntriesProperty.MoveArrayElement(arg1, arg2);
        }

        private void BindPropertyField(VisualElement element, int arg2)
        {
            PropertyField propField = (PropertyField)element;
            propField.BindProperty(_filteredListSelection[arg2]);
        }

        private VisualElement CreatePropertyField()
        {
            return new PropertyField();
        }

        private void OnFilterSet(ChangeEvent<string> evt)
        {
            string newValue = evt.newValue;

            if (string.IsNullOrWhiteSpace(newValue))
            {
                RestrictListViewCapabilities(isRestricted: false);
            }
            else
            {
                RestrictListViewCapabilities(isRestricted: true);
            }

            UpdateListViewToFilter();
        }

        /*
         * Restricted means that we cant reorder, select, add or modify the collection itself, only interact with the displayed entries.
         */
        private void RestrictListViewCapabilities(bool isRestricted)
        {
            _displayEntryListView.reorderable = isRestricted == false;
            _displayEntryListView.selectionType = isRestricted ? SelectionType.None : SelectionType.Single;
            _displayEntryListView.showFoldoutHeader = isRestricted == false;
            _displayEntryListView.showAddRemoveFooter = isRestricted == false;
            _displayEntryListView.showBoundCollectionSize = isRestricted == false;
        }

        private void UpdateListViewToFilter()
        {
            string newFilter = _idrsFilterText.value;

            _filteredListSelection.Clear();

            //We need to check what matches the filter, if there is no filter, add everything.
            for (int i = 0; i < _displayEntriesProperty.arraySize; i++)
            {
                var displayDictionaryEntry = _displayEntriesProperty.GetArrayElementAtIndex(i);

                if (string.IsNullOrWhiteSpace(newFilter))
                {
                    _filteredListSelection.Add(displayDictionaryEntry);
                    continue;
                }

                var targetIDRSAddress = displayDictionaryEntry.FindPropertyRelative("targetIDRS");

                //check the GUID, get the path to the asset, and see if the asset name has the filter.
                var assetGUID = targetIDRSAddress.FindPropertyRelative("m_AssetGUID");
                string guid = assetGUID.stringValue;

                if (AddressablesPathDictionary.instance.TryGetPathFromGUID(guid, out var path))
                {
                    //Malformed paths return null, guard against that.
                    string assetName = System.IO.Path.GetFileName(path);
                    if (string.IsNullOrEmpty(assetName))
                    {
                        continue;
                    }

                    if (assetName.Contains(newFilter))
                    {
                        _filteredListSelection.Add(displayDictionaryEntry);
                    }
                }
            }

            //Finally, rebuild.
            _displayEntryListView.Rebuild();
        }

        protected override bool ValidatePath(string path)
        {
            return path.Contains(MSUConstants.PACKAGE_NAME);
        }
    }
}
