using RoR2;
using System;

namespace Moonstorm.AddressableAssets
{
    /// <summary>
    /// <inheritdoc cref="AddressableAsset{T}"/>
    /// The type of asset this references is an <see cref="ItemDisplayRuleSet"/> and can only be loaded via Addressables
    /// </summary>
    [Serializable]
    public class AddressableIDRS : AddressableAsset<ItemDisplayRuleSet>
    {
        /// <summary>
        /// Parameterless Constructor for <see cref="AddressableIDRS"/>
        /// </summary>
        public AddressableIDRS() { }
        /// <summary>
        /// Constructor for <see cref="AddressableIDRS"/> that sets the <see cref="ItemDisplayRuleSet"/> asset.
        /// </summary>
        /// <param name="id">The <see cref="ItemDisplayRuleSet"/> for this <see cref="AddressableIDRS"/></param>
        public AddressableIDRS(ItemDisplayRuleSet id)
        {
            asset = id;
            useDirectReference = true;
        }
        /// <summary>
        /// Constructor for <see cref="AddressableIDRS"/> that sets the address that'll load the asset
        /// </summary>
        /// <param name="address">The Address for the <see cref="ItemDisplayRuleSet"/></param>
        public AddressableIDRS(string address)
        {
            this.address = address;
            useDirectReference = false;
        }
    }
}
