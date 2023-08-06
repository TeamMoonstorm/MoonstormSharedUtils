using R2API.AddressReferencedAssets;
using RoR2.ExpansionManagement;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    [Obsolete("Use R2API's AddressReferencedExpansionDef Instead")]
    [Serializable]
    public class AddressableExpansionDef : AddressableAsset<ExpansionDef>
    {
        protected override async Task LoadAsset()
        {
            ExpansionDef expansionDef = ExpansionCatalog.expansionDefs.FirstOrDefault(ed => ed.name == address);
            if (expansionDef != null)
            {
                asset = expansionDef;
            }
            else
            {
                await LoadFromAddress();
            }
        }

        public static implicit operator AddressReferencedExpansionDef(AddressableExpansionDef bd)
        {
            if (bd.asset)
                return new AddressReferencedExpansionDef(bd.asset);
            else
                return new AddressReferencedExpansionDef(bd.address);
        }

        public AddressableExpansionDef() { }
        public AddressableExpansionDef(ExpansionDef ed)
        {
            asset = ed;
            useDirectReference = true;
        }
        public AddressableExpansionDef(string addressOrExpansionDefName)
        {
            address = addressOrExpansionDefName;
            useDirectReference = false;
        }
    }
}
