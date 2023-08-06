using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    [Obsolete("Use R2API's AddressReferencedItemDef Instead")]
    [Serializable]
    public class AddressableItemDef : AddressableAsset<ItemDef>
    {
        protected override async Task LoadAsset()
        {
            ItemIndex index = ItemCatalog.FindItemIndex(address);
            if (index != ItemIndex.None)
            {
                asset = ItemCatalog.GetItemDef(index);
            }
            else
            {
                await LoadFromAddress();
            }
        }

        public static implicit operator AddressReferencedItemDef(AddressableItemDef bd)
        {
            if (bd.asset)
                return new AddressReferencedItemDef(bd.asset);
            else
                return new AddressReferencedItemDef(bd.address);
        }


        public AddressableItemDef() { }
        public AddressableItemDef(ItemDef id)
        {
            asset = id;
            useDirectReference = true;
        }
        public AddressableItemDef(string addressOrItemDefName)
        {
            address = addressOrItemDefName;
            useDirectReference = false;
        }
    }
}
