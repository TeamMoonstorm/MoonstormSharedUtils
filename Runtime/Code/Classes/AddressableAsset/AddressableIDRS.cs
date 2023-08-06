using RoR2;
using System;

namespace Moonstorm.AddressableAssets
{
    [Obsolete("Made Obsolete due to the introduction of the ItemDisplayCatalog on 1.4.0")]
    [Serializable]
    public class AddressableIDRS : AddressableAsset<ItemDisplayRuleSet>
    {
        public AddressableIDRS() { }
        public AddressableIDRS(ItemDisplayRuleSet id)
        {
            asset = id;
            useDirectReference = true;
        }
        public AddressableIDRS(string address)
        {
            this.address = address;
            useDirectReference = false;
        }
    }
}
