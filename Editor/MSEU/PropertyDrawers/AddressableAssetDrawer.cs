using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using RoR2EditorKit.Core.PropertyDrawers;
using RoR2EditorKit.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using System;
using RoR2;
using RoR2.ExpansionManagement;

namespace Moonstorm.EditorUtils.PropertyDrawers
{
    public abstract class AddressableAssetDrawer<T> : PropertyDrawer where T : UnityEngine.Object
    {
        TextField addressField;

        ObjectField assetField;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Foldout foldout = new Foldout();
            foldout.text = property.displayName;

            addressField = new TextField("Address");
            //addressField.BindProperty(serializedProperty.FindPropertyRelative("address"));
            addressField.bindingPath = "address";
            addressField.name = "address";
            addressField.tooltip = "An address that'll be used if the asset is null.";
            addressField.RegisterValueChangedCallback(OnAddressSet);
            foldout.Add(addressField);

            assetField = new ObjectField(ObjectNames.NicifyVariableName(typeof(T).Name));
            //assetField.BindProperty(serializedProperty.FindPropertyRelative("asset"));
            assetField.bindingPath = "asset";
            assetField.name = typeof(T).Name;
            assetField.SetObjectType<T>();
            assetField.tooltip = "The Asset to reference, if left null, the asset will be loaded from the address field.";
            assetField.RegisterValueChangedCallback(OnAssetSet);
            foldout.Add(assetField);
            return foldout;
        }
        private void OnAddressSet(ChangeEvent<string> evt)
        {
            string value = evt.newValue;
            assetField.style.display = value.IsNullOrEmptyOrWhitespace() ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnAssetSet(ChangeEvent<UnityEngine.Object> evt)
        {
            UnityEngine.Object obj = evt.newValue;
            addressField.style.display = obj == null ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    [CustomPropertyDrawer(typeof(AddressableAssets.AddressableUnlockableDef))]
    public sealed class AddressableUnlockableDefDrawer : AddressableAssetDrawer<UnlockableDef> { }
    [CustomPropertyDrawer(typeof(AddressableAssets.AddressableBuffDef))]
    public sealed class AddressableBuffDefDrawer : AddressableAssetDrawer<BuffDef> { }
    [CustomPropertyDrawer(typeof(AddressableAssets.AddressableEquipmentDef))]
    public sealed class AddressableEquipmentDefDrawer : AddressableAssetDrawer<EquipmentDef> { }
    [CustomPropertyDrawer(typeof(AddressableAssets.AddressableExpansionDef))]
    public sealed class AddressableExpansionDefDrawer : AddressableAssetDrawer<ExpansionDef> { }
    [CustomPropertyDrawer(typeof(AddressableAssets.AddressableItemDef))]
    public sealed class AddressableItemDefDrawer : AddressableAssetDrawer<ItemDef> { }
}
