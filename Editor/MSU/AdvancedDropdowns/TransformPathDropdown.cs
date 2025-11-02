using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using RoR2;
using UnityEditor;

namespace MSU.Editor
{
    public class TransformPathDropdown : AdvancedDropdown
    {
        public TransformPathDropdown(AdvancedDropdownState state, Transform rootTransform, bool useFullPathAsItemName, Rect displayRect, Type requiredComponentType = null) : base(state)
        {
            this.rootItemKey = rootTransform.name;
            this.rootTransform = rootTransform;
            this.useFullPathAsItemName = useFullPathAsItemName;
            this.requiredComponentType = requiredComponentType;
            this.displayRect = displayRect;
            var minSize = minimumSize;
            minSize.y = 200;
            minimumSize = minSize;
        }

        public event Action<Item> onItemSelected;
        private string rootItemKey;

        public bool useFullPathAsItemName { get; }
        public Transform rootTransform { get; }
        public Type requiredComponentType { get; }
        public Rect displayRect { get; }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            onItemSelected?.Invoke((Item)item);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            //TransformPath to Item
            var items = new Dictionary<string, Item>();
            var rootItem = new Item(rootItemKey, rootTransform);
            items.Add("None", new Item("None", null));

            items.Add(rootItemKey, rootItem);

            try
            {
                AddTransformsInsideTransform(rootTransform, items);
            }
            catch(Exception ex)
            {
                MSULog.Error(ex);
            }

            foreach(var key in items.Keys)
            {
                var item = items[key];

                if (key == rootItemKey)
                    continue;

                //If we have a required component type, and the item's transform entry DOES NOT have a child that has the required component, skip it 
                if(requiredComponentType != null && requiredComponentType.IsSameOrSubclassOf(typeof(Component)) && item.transform && !item.transform.GetComponentInChildren(requiredComponentType))
                {
                    continue;
                }

                if(key.LastIndexOf("/") == -1)
                {
                    rootItem.AddChild(item);
                }
                else
                {
                    var parentName = key.Substring(0, key.LastIndexOf("/"));
                    items[parentName].AddChild(item);
                }
            }

            return rootItem;
        }

        private void AddTransformsInsideTransform(Transform parent, Dictionary<string, Item> items)
        {
            if(parent != rootTransform)
            {
                string name = Util.BuildPrefabTransformPath(rootTransform, parent, false, true);
                string truncatedName = TruncateString(name, displayRect.width, EditorStyles.label);
                items.Add(name, new Item(useFullPathAsItemName ? truncatedName : parent.name, parent));
            }
            foreach(Transform child in parent)
            {
                AddTransformsInsideTransform(child, items);
            }
        }

        private string TruncateString(string input, float conformingWidth, GUIStyle targetStyle)
        {
            GUIContent cachedContent = new GUIContent();
            cachedContent.text = input;
            float width = targetStyle.CalcSize(cachedContent).x;
            if(width <= conformingWidth)
            {
                return input;
            }

            string cpy = input;
            while(true)
            {
                string substring = cpy;
                if(cpy.Contains('/'))
                {
                    substring = cpy.Substring(cpy.IndexOf('/') + 1);
                }
                string result = string.Format("../{0}", substring);
                cachedContent.text = result;
                width = targetStyle.CalcSize(cachedContent).x;
                if(width <= conformingWidth)
                {
                    return result;
                }
                else
                {
                    cpy = substring;
                }
            }
        }

        public class Item : AdvancedDropdownItem
        {
            public Transform transform { get; }
            public Item(string name, Transform transform) : base(name)
            {
                this.transform = transform;
            }
        }
    }
}