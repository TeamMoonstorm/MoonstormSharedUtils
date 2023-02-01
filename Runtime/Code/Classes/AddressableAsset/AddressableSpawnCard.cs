using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    [Serializable]
    public class AddressableSpawnCard : AddressableAsset<SpawnCard>
    {
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
