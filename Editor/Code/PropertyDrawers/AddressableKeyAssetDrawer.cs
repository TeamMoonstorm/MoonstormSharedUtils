using Moonstorm.AddressableAssets;
using RoR2;
using RoR2EditorKit;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(AddressableKeyAsset))]
    public sealed class AddressableKeyAssetDrawer : PropertyDrawer
    {
        TextField addressField;
        EnumField addressType;
        ObjectField assetField;

        SerializedProperty property;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            this.property = property;
            Foldout foldout = new Foldout();
            foldout.text = property.displayName;

            assetField = new ObjectField($"Key Asset");
            assetField.bindingPath = property.FindPropertyRelative("asset").propertyPath;
            assetField.name = "keyAsset";
            assetField.SetObjectType<ScriptableObject>();
            assetField.tooltip = $"The KeyAsset to use for this DisplayRule, if left null, the KeyAsset will be loaded from the address field";
            assetField.RegisterValueChangedCallback(OnKeyAssetSet);
            foldout.Add(assetField);

            addressField = new TextField($"Item/Eqp Name || Address");
            addressField.isDelayed = true;
            addressField.bindingPath = property.FindPropertyRelative(nameof(AddressableKeyAsset.address)).propertyPath;
            addressField.name = $"address";
            addressField.tooltip = $"An Address or Item/EquipmentDef that'll be used if the Asset is null";
            addressField.RegisterValueChangedCallback(OnAddressSet);
            foldout.Add(addressField);

            addressType = new EnumField($"Load KeyAsset From", AddressableKeyAsset.KeyAssetAddressType.EquipmentCatalog);
            addressType.bindingPath = property.FindPropertyRelative(nameof(AddressableKeyAsset.loadAssetFrom)).propertyPath;
            addressType.name = $"loadKeyAssetfrom";
            addressType.tooltip = $"From where the asset will be loaded";
            addressType.RegisterValueChangedCallback(OnAddressTypeSet);
            foldout.Add(addressType);

            OnKeyAssetSet();
            OnAddressSet();
            OnAddressTypeSet();

            return foldout;
        }

        private void OnKeyAssetSet(ChangeEvent<UnityEngine.Object> evt = null)
        {
            if (property == null)
                return;
            var newValue = evt == null ? property.FindPropertyRelative("asset").objectReferenceValue : evt.newValue;

            if (newValue)
            {
                var flag0 = newValue is ItemDef;
                var flag1 = newValue is EquipmentDef;
                if (!flag0 && !flag1)
                {
                    EditorUtility.DisplayDialog($"Invalid KeyAsset", $"An IDRS KeyAsset must be either an EquipmentDef or ItemDef, the selected KeyAsset is {newValue.GetType().Name} \nThe field will has been set to \"None\"", "Ok");

                    newValue = null;
                    (evt.target as ObjectField).value = newValue;
                }
            }

            addressField.SetDisplay(newValue ? DisplayStyle.None : DisplayStyle.Flex);
            addressType.SetDisplay(newValue ? DisplayStyle.None : DisplayStyle.Flex);
        }

        private async void OnAddressSet(ChangeEvent<string> evt = null)
        {
            if (property == null)
                return;
            var newValue = evt == null ? property.FindPropertyRelative("address").stringValue : evt.newValue;

            var enumValue = (AddressableKeyAsset.KeyAssetAddressType)property.FindPropertyRelative(nameof(AddressableKeyAsset.loadAssetFrom)).enumValueIndex;

            if (enumValue == AddressableKeyAsset.KeyAssetAddressType.Addressables && !newValue.IsNullOrEmptyOrWhitespace())
            {
                try
                {
                    var asset = await Addressables.LoadAssetAsync<UnityEngine.Object>(newValue).Task;
                    if (asset == null)
                    {
                        newValue = String.Empty;
                        (evt.target as TextField).value = newValue;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Addressables Exception: {ex}");
                    newValue = String.Empty;
                    if (evt != null)
                        (evt.target as TextField).value = newValue;
                }
            }

            assetField.SetDisplay((newValue.IsNullOrEmptyOrWhitespace() || newValue != "Address") ? DisplayStyle.Flex : DisplayStyle.None);
        }

        private void OnAddressTypeSet(ChangeEvent<Enum> evt = null)
        {
            try
            {
                if (property == null)
                    return;
                var type = evt != null ? (AddressableKeyAsset.KeyAssetAddressType)evt.newValue : (AddressableKeyAsset.KeyAssetAddressType)property.FindPropertyRelative(nameof(AddressableKeyAsset.loadAssetFrom)).enumValueIndex;

                switch (type)
                {
                    case AddressableKeyAsset.KeyAssetAddressType.Addressables:
                        addressField.label = $"Address";
                        break;
                    case AddressableKeyAsset.KeyAssetAddressType.ItemCatalog:
                        addressField.label = $"Item Name";
                        break;
                    case AddressableKeyAsset.KeyAssetAddressType.EquipmentCatalog:
                        addressField.label = $"Equipment Name";
                        break;
                }
            }
            catch (Exception ex) { }
        }
    }
}
