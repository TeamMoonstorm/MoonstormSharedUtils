using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    [Serializable]
    public class AddressableItemDef : AddressableAsset<ItemDef>
    {
        protected override async Task LoadAsset()
        {
            ItemIndex index = ItemCatalog.FindItemIndex(address);
            if(index != ItemIndex.None)
            {
                await SetAsset(ItemCatalog.GetItemDef(index));
            }
            else
            {
                await LoadFromAddress();
            }
        }
    }
}
