using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    /// <summary>
    /// <inheritdoc cref="AddressableAsset{T}"/>
    /// The type of asset this references is an <see cref="ExpansionDef"/>, and can be loaded either via Addressables or the <see cref="ExpansionCatalog"/>
    /// </summary>
    [Serializable]
    public class AddressableExpansionDef : AddressableAsset<ExpansionDef>
    {
        protected override async Task LoadAsset()
        {
            ExpansionDef expansionDef = ExpansionCatalog.expansionDefs.FirstOrDefault(ed => ed.name == address);
            if(expansionDef != null)
            {
                await SetAsset(expansionDef);
            }
            else
            {
                await LoadFromAddress();
            }
        }
    }
}
