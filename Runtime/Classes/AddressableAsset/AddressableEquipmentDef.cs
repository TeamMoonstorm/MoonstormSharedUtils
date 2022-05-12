using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    [Serializable]
    public class AddressableEquipmentDef : AddressableAsset<EquipmentDef>
    {
        protected override void LoadAsset()
        {
            EquipmentIndex index = EquipmentCatalog.FindEquipmentIndex(address);
            if(index != EquipmentIndex.None)
            {
                SetAsset(EquipmentCatalog.GetEquipmentDef(index));
            }
            else
            {
                LoadFromAddress();
            }
        }
    }
}