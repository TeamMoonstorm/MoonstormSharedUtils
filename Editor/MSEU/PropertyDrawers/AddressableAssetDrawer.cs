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
using Moonstorm.AddressableAssets;

namespace Moonstorm.EditorUtils.PropertyDrawers
{
    public abstract class AddressableAssetDrawer<T> : PropertyDrawer where T : UnityEngine.Object
    {
        protected virtual string addressFieldLabel { get; } =  null;
        TextField addressField;

        ObjectField assetField;

        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
        public sealed override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Foldout foldout = new Foldout();
            foldout.text = property.displayName;

            addressField = new TextField(addressFieldLabel ?? "Address");
            //addressField.BindProperty(serializedProperty.FindPropertyRelative("address"));
            addressField.isDelayed = true;
            addressField.bindingPath = property.FindPropertyRelative("address").propertyPath;
            addressField.name = "address";
            addressField.tooltip = "An address that'll be used if the asset is null.";
            addressField.RegisterValueChangedCallback(OnAddressSet);
            foldout.Add(addressField);

            assetField = new ObjectField(ObjectNames.NicifyVariableName(typeof(T).Name));
            //assetField.BindProperty(serializedProperty.FindPropertyRelative("asset"));
            assetField.bindingPath = property.FindPropertyRelative("asset").propertyPath;
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

    #region drawers
    [CustomPropertyDrawer(typeof(AddressableBuffDef))]
    public sealed class AddressableBuffDefDrawer : AddressableAssetDrawer<BuffDef> { protected override string addressFieldLabel => $"BuffDef Name || Address"; }
    //-----
    [CustomPropertyDrawer(typeof(AddressableEquipmentDef))]
    public sealed class AddressableEquipmentDefDrawer : AddressableAssetDrawer<EquipmentDef> { protected override string addressFieldLabel => $"EquipmentDef Name || Address"; }
    //-----
    [CustomPropertyDrawer(typeof(AddressableExpansionDef))]
    public sealed class AddressableExpansionDefDrawer : AddressableAssetDrawer <ExpansionDef> { protected override string addressFieldLabel => $"ExpansionDef Name || Address"; }
    //-----
    [CustomPropertyDrawer(typeof(AddressableItemDef))]
    public sealed class AddressableItemDef : AddressableAssetDrawer<ItemDef> { protected override string addressFieldLabel => $"ItemDef Name || Address"; }
    //-----
    [CustomPropertyDrawer(typeof(AddressableUnlockableDef))]
    public sealed class AddressableUnlockableDef : AddressableAssetDrawer<UnlockableDef> { protected override string addressFieldLabel => $"UnlockableDEf Name || Address"; }
    //-----
    [CustomPropertyDrawer(typeof(AddressableGameObject))]
    public sealed class AddressableGameObjectDrawer : AddressableAssetDrawer<GameObject> { }
    //-----
    [CustomPropertyDrawer(typeof(AddressableIDRS))]
    public sealed class AddressableIDRSDrawer : AddressableAssetDrawer<ItemDisplayRuleSet> { }
    #endregion
}
