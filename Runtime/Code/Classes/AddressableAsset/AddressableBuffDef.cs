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
    /// The type of asset this references is a <see cref="BuffDef"/>, and can be loaded either via Addressables or the <see cref="BuffCatalog"/>
    /// </summary>
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
