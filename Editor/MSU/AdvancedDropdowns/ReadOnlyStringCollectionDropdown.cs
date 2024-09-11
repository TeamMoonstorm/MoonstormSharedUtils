using RoR2.Editor;
using System;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MSU.Editor
{
    public class ReadOnlyStringCollectionDropdown : AdvancedDropdown
    {
        public event Action<Item> onItemSelected;
        private ReadOnlyCollection<string> _collection;
        private string _rootItemKey;

        protected override AdvancedDropdownItem BuildRoot()
        {
            var rootItem = new Item(_rootItemKey, string.Empty);
            rootItem.AddChild(new Item("None", ""));
            foreach(var entry in _collection)
            {
                rootItem.AddChild(new Item(ObjectNames.NicifyVariableName(entry), entry));
            }
            return rootItem;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            onItemSelected?.Invoke((Item)item);
        }

        public static void DrawIMGUI(ReadOnlyStringCollectionDropdown instance, string displayValue, GUIContent label, string noValueSetString)
        {
            var displayName = displayValue.IsNullOrEmptyOrWhiteSpace() ? noValueSetString : displayValue;

            var rect = EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            if(EditorGUILayout.DropdownButton(new GUIContent(displayValue), FocusType.Passive))
            {
                Rect labelRect = GUILayoutUtility.GetLastRect();

                var rectToUse = new Rect
                {
                    x = rect.x + labelRect.width,
                    y = rect.y,
                    height = rect.height,
                    width = rect.width - labelRect.width
                };

                instance.Show(rectToUse);
            }
            EditorGUILayout.EndHorizontal();
        }

        public ReadOnlyStringCollectionDropdown(AdvancedDropdownState state, ReadOnlyCollection<string> collection, string rootItemKey) : base(state)
        {
            _rootItemKey = rootItemKey;
            _collection = collection;
        }

        public class Item : AdvancedDropdownItem
        {
            public string value { get; }
            public Item(string displayName, string value) : base(displayName)
            {
                this.value = value;
            }
        }

    }
}