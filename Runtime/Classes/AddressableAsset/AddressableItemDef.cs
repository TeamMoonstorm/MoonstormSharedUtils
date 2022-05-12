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
        protected override void LoadAsset()
        {
            ItemIndex index = ItemCatalog.FindItemIndex(address);
            if(index != ItemIndex.None)
            {
                SetAsset(ItemCatalog.GetItemDef(index));
            }
            else
            {
                LoadFromAddress();
            }
        }
    }
}
