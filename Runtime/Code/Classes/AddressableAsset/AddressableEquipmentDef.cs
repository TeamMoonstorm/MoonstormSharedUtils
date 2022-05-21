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
        protected override async Task LoadAsset()
        {
            EquipmentIndex index = EquipmentCatalog.FindEquipmentIndex(address);
            if(index != EquipmentIndex.None)
            {
                await SetAsset(EquipmentCatalog.GetEquipmentDef(index));
            }
            else
            {
                await LoadFromAddress();
            }
        }
    }
}