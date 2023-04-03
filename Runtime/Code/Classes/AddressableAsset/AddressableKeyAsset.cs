using RoR2;
using System;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    [Serializable, Obsolete("Made obsoslete due to the introduction of the ItemDisplayCatalog on 1.4.0")]
    public class AddressableKeyAsset : AddressableAsset<UnityEngine.Object>
    {
        public enum KeyAssetAddressType
        {
            EquipmentCatalog = 0,
            ItemCatalog = 1,
            Addressables = 2,
            UsingDirectReference = 3,
        }
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
                            if (eqpIndex != EquipmentIndex.None)
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
                            if (itemIndex != ItemIndex.None)
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

        public AddressableKeyAsset() { }
        public AddressableKeyAsset(ItemDef id)
        {
            asset = id;
            useDirectReference = true;
            loadAssetFrom = KeyAssetAddressType.UsingDirectReference;
        }
        public AddressableKeyAsset(EquipmentDef ed)
        {
            asset = ed;
            useDirectReference = true;
            loadAssetFrom = KeyAssetAddressType.UsingDirectReference;
        }
        public AddressableKeyAsset(string addressOrEquipmentNameOrItemName, KeyAssetAddressType addressType)
        {
            useDirectReference = false;
            address = addressOrEquipmentNameOrItemName;
            loadAssetFrom = addressType;
        }
    }
}
