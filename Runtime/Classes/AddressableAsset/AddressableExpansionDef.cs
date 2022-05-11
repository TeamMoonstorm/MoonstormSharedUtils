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
        public override void LoadAsset()
        {
            ExpansionDef expansionDef = ExpansionCatalog.expansionDefs.FirstOrDefault(ed => ed.name == address);
            if(expansionDef != null)
            {
                SetAsset(expansionDef);
            }
            else
            {
                LoadFromAddress();
            }
        }
    }
}
