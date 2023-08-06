using R2API.AddressReferencedAssets;
using RoR2;
using System;

namespace Moonstorm.AddressableAssets
{
    [Obsolete("Use R2API's AddressReferencedSpawnCard Instead")]
    [Serializable]
    public class AddressableSpawnCard : AddressableAsset<SpawnCard>
    {
        public static implicit operator AddressReferencedSpawnCard(AddressableSpawnCard bd)
        {
            if (bd.asset)
                return new AddressReferencedSpawnCard(bd.asset);
            else
                return new AddressReferencedSpawnCard(bd.address);
        }
        public AddressableSpawnCard() { }
        public AddressableSpawnCard(SpawnCard sc)
        {
            asset = sc;
            useDirectReference = true;
        }
        public AddressableSpawnCard(string address)
        {
            this.address = address;
            useDirectReference = false;
        }
    }
}
