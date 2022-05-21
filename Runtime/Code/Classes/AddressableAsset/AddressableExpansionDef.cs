using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
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
