using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    /// <summary>
    /// <inheritdoc cref="AddressableAsset{T}"/>
    /// the type of asset this references is an <see cref="UnlockableDef"/>, and can be loaded either via Addressables or the <see cref="UnlockableCatalog"/>
    /// </summary>
    [Serializable]
    public class AddressableUnlockableDef : AddressableAsset<UnlockableDef>
    {
        protected override async Task LoadAsset()
        {
            UnlockableDef unlockable = UnlockableCatalog.GetUnlockableDef(address);
            if(unlockable != null)
            {
                asset = unlockable;
            }
            else
            {
                await LoadFromAddress();
            }
        }

        /// <summary>
        /// Parameterless Constructor for <see cref="AddressableUnlockableDef"/>
        /// </summary>
        public AddressableUnlockableDef() { }
        /// <summary>
        /// Constructor for <see cref="AddressableUnlockableDef"/> that sets the <see cref="UnlockableDef"/> asset.
        /// </summary>
        /// <param name="ud">The <see cref="UnlockableDef"/> for this <see cref="AddressableUnlockableDef"/></param>
        public AddressableUnlockableDef(UnlockableDef ud)
        {
            asset = ud;
            useDirectReference = true;
        }
        /// <summary>
        /// Constructor for <see cref="AddressableUnlockableDef"/> that sets the address that'll load the asset
        /// </summary>
        /// <param name="addressOrUnlockableDefName">The Address for the <see cref="UnlockableDef"/>, this can also be the asset's name so it can load via the <see cref="UnlockableCatalog"/></param>
        public AddressableUnlockableDef(string addressOrUnlockableDefName)
        {
            address = addressOrUnlockableDefName;
            useDirectReference = false;
        }
    }
}