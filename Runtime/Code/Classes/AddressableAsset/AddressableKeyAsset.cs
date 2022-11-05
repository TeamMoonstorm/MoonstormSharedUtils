using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    /// <summary>
    /// <inheritdoc cref="AddressableAsset{T}"/>
    /// The type of asset this references should be either an <see cref="ItemDef"/> or <see cref="EquipmentDef"/>, and these can be loaded either via Addressables or their respective catalogs
    /// </summary>
    [Serializable]
    public class AddressableKeyAsset : AddressableAsset<UnityEngine.Object>
    {
        /// <summary>
        /// How the key asset should be loaded
        /// </summary>
        public enum KeyAssetAddressType
        {
            /// <summary>
            /// Address is treated as an Equipment Name
            /// </summary>
            EquipmentCatalog = 0,
            /// <summary>
            /// Address is ttreated as an Item name
            /// </summary>
            ItemCatalog = 1,
            /// <summary>
            /// Address is used with Addressables loading
            /// </summary>
            Addressables = 2,
            /// <summary>
            /// The AddressableKeyAsset is using a direct reference
            /// </summary>
            UsingDirectReference = 3,
        }

        /// <summary>
        /// <inheritdoc cref="KeyAssetAddressType"/>
        /// </summary>
        public KeyAssetAddressType loadAssetFrom;

        protected override async Task LoadAsset()
        {
            try
            {
                switch (loadAssetFrom)
                {
                    case KeyAssetAddressType.EquipmentCatalog:
                        {
                            EquipmentIndex eqpIndex = EquipmentCatalog.FindEquipmentIndex(address);
                            if(eqpIndex != EquipmentIndex.None)
                            {
                                asset = EquipmentCatalog.GetEquipmentDef(eqpIndex);
                            }
                            else
                            {
                                throw AddressableKeyAssetException($"Could not load EquipmentDef from catalog with name {address}" +
                                    $"\n(AddressableKeyAsset has loadAssetFrom set to {loadAssetFrom})");
                            }
                            break;
                        }
                    case KeyAssetAddressType.ItemCatalog:
                        {
                            ItemIndex itemIndex = ItemCatalog.FindItemIndex(address);
                            if(itemIndex != ItemIndex.None)
                            {
                                asset = ItemCatalog.GetItemDef(itemIndex);
                            }
                            else
                            {
                                throw AddressableKeyAssetException($"Could not load ItemDef from catalog with name {address}" +
                                    $"\n(AddressableKeyAsset has loadAssetFrom set to {loadAssetFrom})");
                            }
                            break;
                        }
                    case KeyAssetAddressType.Addressables:
                        {
                            await LoadFromAddress();
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MSULog.Error(ex);
            }
        }

        private Exception AddressableKeyAssetException(string message) => new NullReferenceException(message);

        /// <summary>
        /// Parameterless Constructor for <see cref="AddressableKeyAsset"/>
        /// </summary>
        public AddressableKeyAsset() { }

        /// <summary>
        /// Constructor for <see cref="AddressableKeyAsset"/> that sets the Key Asset to the <see cref="ItemDef"/> specified in <paramref name="id"/>
        /// </summary>
        /// <param name="id">The <see cref="ItemDef"/> for this <see cref="AddressableKeyAsset"/></param>
        public AddressableKeyAsset(ItemDef id)
        {
            asset = id;
            useDirectReference = true;
            loadAssetFrom = KeyAssetAddressType.UsingDirectReference;
        }

        /// <summary>
        /// Constructor for <see cref="AddressableKeyAsset"/> that sets the Key Asset to the <see cref="EquipmentDef"/> specified in <paramref name="ed"/>
        /// </summary>
        /// <param name="ed">The <see cref="EquipmentDef"/> for this <see cref="AddressableKeyAsset"/></param>
        public AddressableKeyAsset(EquipmentDef ed)
        {
            asset = ed;
            useDirectReference = true;
            loadAssetFrom = KeyAssetAddressType.UsingDirectReference;
        }

        /// <summary>
        /// Constructor for <see cref="AddressableKeyAsset"/> that sets the address that'll load the asset
        /// </summary>
        /// <param name="addressOrEquipmentNameOrItemName">The Address for the Key Asset, this can also be the ItemDef's or EquipmentDef's name so it can load via their respective catalogs.</param>
        /// <param name="addressType">Specifies how the address is used for loading the key asset.</param>
        public AddressableKeyAsset(string addressOrEquipmentNameOrItemName, KeyAssetAddressType addressType)
        {
            useDirectReference = false;
            address = addressOrEquipmentNameOrItemName;
            loadAssetFrom = addressType;
        }
    }
}
