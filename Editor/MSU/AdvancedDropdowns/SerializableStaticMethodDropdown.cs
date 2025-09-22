using MSU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace RoR2.Editor
{
    public class SerializableStaticMethodDropdown : AdvancedDropdown
    {
        private string rootItemKey;

        public event Action<Item> onItemSelected;

        public bool useFullNameAsItemName { get; }

        public Type requiredReturnType { get; }

        public Type[] arguments { get; }

        protected override AdvancedDropdownItem BuildRoot()
        {
            List<MethodInfo> methods = TypeCache.GetMethodsWithAttribute<SerializableStaticMethod.MethodDetector>().OrderBy(methodInfo => methodInfo.DeclaringType.FullName + "." + methodInfo.Name).ToList();

            var items = new Dictionary<string, Item>();
            var rootItem = new Item(rootItemKey, rootItemKey, rootItemKey, rootItemKey);
            items.Add(rootItemKey, rootItem);

            items.Add("None", new Item("None", string.Empty, string.Empty, string.Empty));

            foreach(var method in methods)
            {
                var itemFullName = method.DeclaringType.FullName + "." + method.Name + "()";

                while(true)
                {
                    var lastDotIndex = itemFullName.LastIndexOf('.');
                    if(!items.ContainsKey(itemFullName))
                    {
                        var typeName = lastDotIndex == -1 ? itemFullName : itemFullName.Substring(lastDotIndex + 1);
                        var item = new Item(typeName, typeName, itemFullName, "");
                        items.Add(itemFullName, item);
                    }

                    if (itemFullName.IndexOf('.') == -1) break;

                    itemFullName = itemFullName.Substring(0, lastDotIndex);
                }
            }

            foreach (var item in items)
            {
                if (item.Key == rootItemKey)
                    continue;

                var fullName = item.Key;
                if (fullName.LastIndexOf('.') == -1)
                {
                    rootItem.AddChild(item.Value);
                }
                else
                {
                    var parentName = fullName.Substring(0, fullName.LastIndexOf('.'));
                    items[parentName].AddChild(item.Value);
                }
            }

            return rootItem;

            /*foreach (var assemblyQualifiedName in types.Select(x => x.AssemblyQualifiedName).OrderBy(x => x))
            {
                var itemFullName = assemblyQualifiedName.Split(',')[0];
                while (true)
                {
                    var lastDotIndex = itemFullName.LastIndexOf('.');
                    if (!items.ContainsKey(itemFullName))
                    {
                        var typeName =
                            lastDotIndex == -1 ? itemFullName : itemFullName.Substring(lastDotIndex + 1);
                        var item = new Item(useFullNameAsItemName ? itemFullName : typeName, typeName, itemFullName, assemblyQualifiedName);
                        items.Add(itemFullName, item);
                    }

                    if (itemFullName.IndexOf('.') == -1) break;

                    itemFullName = itemFullName.Substring(0, lastDotIndex);
                }
            }

            foreach (var item in items)
            {
                if (item.Key == rootItemKey)
                    continue;

                var fullName = item.Key;
                if (fullName.LastIndexOf('.') == -1)
                {
                    rootItem.AddChild(item.Value);
                }
                else
                {
                    var parentName = fullName.Substring(0, fullName.LastIndexOf('.'));
                    items[parentName].AddChild(item.Value);
                }
            }

            return rootItem;*/
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            onItemSelected?.Invoke((Item)item);
        }

        public SerializableStaticMethodDropdown(AdvancedDropdownState state) : this(state, null) { }

        public SerializableStaticMethodDropdown(AdvancedDropdownState state, Type requiredBaseType) : this(state, requiredBaseType, false)
        {

        }

        public SerializableStaticMethodDropdown(AdvancedDropdownState state, Type requiredBaseType, bool useFullName) : base(state)
        {
            //this.requiredBaseType = requiredBaseType;
            rootItemKey = requiredBaseType?.Name ?? "Select Type";
            var minSize = minimumSize;
            minSize.y = 200;
            minimumSize = minSize;
            useFullNameAsItemName = useFullName;
        }


        public class Item : AdvancedDropdownItem
        {

            public string typeName { get; }


            public string fullName { get; }

            public string assemblyQualifiedName { get; }

            internal Item(string displayName, string typeName, string fullName, string assemblyQualifiedName) : base(
    displayName)
            {
                this.typeName = typeName;
                this.fullName = fullName;
                this.assemblyQualifiedName = assemblyQualifiedName;
            }
        }
    }
}