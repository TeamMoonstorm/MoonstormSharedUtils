using RoR2;
using System;

namespace Moonstorm.AddressableAssets
{
    /// <summary>
    /// <inheritdoc cref="AddressableAsset{T}"/>
    /// The type of asset this references is a <see cref="SpawnCard"/>, these spawn cards can only be loaded from addressables
    /// </summary>
    [Serializable]
    public class AddressableSpawnCard : AddressableAsset<SpawnCard>
    {
        /// <summary>
        /// Parameterless Constructor for <see cref="AddressableSpawnCard"/>
        /// </summary>
        public AddressableSpawnCard() { }
        /// <summary>
        /// Constructor for <see cref="AddressableSpawnCard"/> that sets the <see cref="SpawnCard"/> asset.
        /// </summary>
        /// <param name="sc">The <see cref="SpawnCard"/> for this <see cref="AddressableSpawnCard"/></param>
        public AddressableSpawnCard(SpawnCard sc)
        {
            asset = sc;
            useDirectReference = true;
        }
        /// <summary>
        /// Constructor for <see cref="AddressableSpawnCard
        /// "/> that sets the address that'll load the asset
        /// </summary>
        /// <param name="address">The Addressables address for loading the spawn card</param>
        public AddressableSpawnCard(string address)
        {
            this.address = address;
            useDirectReference = false;
        }
    }
}
