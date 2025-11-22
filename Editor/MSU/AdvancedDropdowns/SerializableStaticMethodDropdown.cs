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
            var rootItem = new Item(rootItemKey, rootItemKey, rootItemKey);
            items.Add(rootItemKey, rootItem);

            items.Add("None", new Item("None", string.Empty, string.Empty));

            foreach(var method in methods)
            {
                var itemFullName = method.DeclaringType.FullName + "." + method.Name + "()";

                if (!method.IsStatic)
                    continue;

                if (requiredReturnType != null && method.ReturnType != requiredReturnType)
                    continue;

                var methodParameters = method.GetParameters();
                if(arguments != null)
                {
                    if(methodParameters.Length != arguments.Length)
                        continue;

                    bool shouldSkip = false;
                    for(int i = 0; i < methodParameters.Length; i++)
                    {
                        if (methodParameters[i].ParameterType != arguments[i])
                        {
                            shouldSkip = true;
                            break;
                        }
                    }

                    if(shouldSkip)
                    {
                        continue;
                    }
                }


                while(true)
                {
                    var lastDotIndex = itemFullName.LastIndexOf('.');
                    if(!items.ContainsKey(itemFullName))
                    {
                        var displayName = lastDotIndex == -1 ? itemFullName : itemFullName.Substring(lastDotIndex + 1);
                        var item = new Item(displayName, method.Name, method.DeclaringType.AssemblyQualifiedName);
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
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            onItemSelected?.Invoke((Item)item);
        }

        public SerializableStaticMethodDropdown(AdvancedDropdownState state) : this(state, null, null, false) { }

        public SerializableStaticMethodDropdown(AdvancedDropdownState state, Type requiredReturnType, Type[] arguments) : this(state, requiredReturnType, arguments, false)
        {

        }

        public SerializableStaticMethodDropdown(AdvancedDropdownState state, Type requiredReturnType, Type[] arguments, bool useFullName) : base(state)
        {
            this.requiredReturnType = requiredReturnType;
            this.arguments = arguments;
            rootItemKey = "Select Method";
            var minSize = minimumSize;
            minSize.y = 200;
            minimumSize = minSize;
            useFullNameAsItemName = useFullName;
        }


        public class Item : AdvancedDropdownItem
        { 
            public string methodName { get; }
            public string assemblyQualifiedTypeName { get; }

            internal Item(string displayName, string methodName, string assemblyQualifiedTypeName) : base(displayName)
            {
                this.methodName = methodName;
                this.assemblyQualifiedTypeName = assemblyQualifiedTypeName;
            }
        }
    }
}