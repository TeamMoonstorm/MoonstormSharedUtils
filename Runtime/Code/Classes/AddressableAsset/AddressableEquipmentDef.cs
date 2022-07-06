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
    /// The type of asset this references is an <see cref="EquipmentDef"/>, and can be loaded either via Addressables or the <see cref="EquipmentCatalog"/>
    /// </summary>
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