using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    [Serializable]
    public class AddressableBuffDef : AddressableAsset<BuffDef>
    {
        protected override async Task LoadAsset()
        {
            BuffIndex index = BuffCatalog.FindBuffIndex(address);
            if(index != BuffIndex.None)
            {
                await SetAsset(BuffCatalog.GetBuffDef(index));
            }
            else
            {
                await LoadFromAddress();
            }
        }
    }
}
