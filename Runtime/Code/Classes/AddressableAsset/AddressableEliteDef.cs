using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    [Obsolete("Use R2API's AddressReferencedEliteDef Instead")]
    [Serializable]
    public class AddressableEliteDef : AddressableAsset<EliteDef>
    {
        protected override async Task LoadAsset()
        {
            EliteDef def = EliteCatalog.eliteDefs.FirstOrDefault(x => x.name.Equals(address, StringComparison.OrdinalIgnoreCase));
            if (def != null)
            {
                asset = def;
            }
            else
            {
                await LoadFromAddress();
            }
        }

        public static implicit operator AddressReferencedEliteDef(AddressableEliteDef bd)
        {
            if (bd.asset)
                return new AddressReferencedEliteDef(bd.asset);
            else
                return new AddressReferencedEliteDef(bd.address);
        }
        public AddressableEliteDef() { }
        public AddressableEliteDef(EliteDef ed)
        {
            asset = ed;
            useDirectReference = true;
        }
        public AddressableEliteDef(string addressOrEliteDefName)
        {
            address = addressOrEliteDefName;
            useDirectReference = false;
        }
    }
}