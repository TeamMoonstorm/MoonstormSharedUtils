using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    [Serializable]
    public class AddressableKeyAsset : AddressableAsset<UnityEngine.Object>
    {
        public enum KeyAssetAddressType
        {
            EquipmentCatalog,
            ItemCatalog,
            Addressables
        }

        public KeyAssetAddressType loadAssetFrom;

        protected override void LoadAsset()
        {
            switch (loadAssetFrom)
            {
                case KeyAssetAddressType.EquipmentCatalog:
                    {
                        EquipmentIndex eqpIndex = EquipmentCatalog.FindEquipmentIndex(address);
                        if(eqpIndex != EquipmentIndex.None)
                        {
                            SetAsset(EquipmentCatalog.GetEquipmentDef(eqpIndex));
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
                            SetAsset(ItemCatalog.GetItemDef(itemIndex));
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
                        LoadFromAddress();
                        break;
                    }
            }
        }

        private Exception AddressableKeyAssetException(string message) => new NullReferenceException(message);
    }
}
