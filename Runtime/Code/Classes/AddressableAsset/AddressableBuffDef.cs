using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    [Obsolete("Use R2API's AddressReferencedBuffDef Instead")]
    [Serializable]
    public class AddressableBuffDef : AddressableAsset<BuffDef>
    {
        protected override async Task LoadAsset()
        {
            BuffIndex index = BuffCatalog.FindBuffIndex(address);
            if (index != BuffIndex.None)
            {
                asset = BuffCatalog.GetBuffDef(index);
            }
            else
            {
                await LoadFromAddress();
            }
        }

        public static implicit operator AddressReferencedBuffDef(AddressableBuffDef bd)
        {
            if (bd.asset)
                return new AddressReferencedBuffDef(bd.asset);
            else
                return new AddressReferencedBuffDef(bd.address);
        }
        public AddressableBuffDef() { }
        public AddressableBuffDef(BuffDef bd)
        {
            asset = bd;
            useDirectReference = true;
        }
        public AddressableBuffDef(string addressOrBuffDefName)
        {
            address = addressOrBuffDefName;
            useDirectReference = false;
        }
    }
}
