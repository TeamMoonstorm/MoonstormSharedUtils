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
        public override void LoadAsset()
        {
            BuffIndex index = BuffCatalog.FindBuffIndex(address);
            if(index != BuffIndex.None)
            {
                SetAsset(BuffCatalog.GetBuffDef(index));
            }
            else
            {
                LoadFromAddress();
            }
        }
    }
}
