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
            Addressables = 2
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
                                await SetAsset(EquipmentCatalog.GetEquipmentDef(eqpIndex));
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
                                await SetAsset(ItemCatalog.GetItemDef(itemIndex));
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
    }
}
