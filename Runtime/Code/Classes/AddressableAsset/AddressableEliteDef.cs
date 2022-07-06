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
    /// The type of asset this refereces is an <see cref="EliteDef"/>, and can be loaded either via Addressables or the <see cref="EliteCatalog"/>
    /// </summary>
    [Serializable]
    public class AddressableEliteDef : AddressableAsset<EliteDef>
    {
        protected override async Task LoadAsset()
        {
            EliteDef def = EliteCatalog.eliteDefs.FirstOrDefault(x => x.name.Equals(address, StringComparison.OrdinalIgnoreCase));
            if(def != null)
            {
                await SetAsset(def);
            }
            else
            {
                await LoadFromAddress();
            }
        }
    }
}