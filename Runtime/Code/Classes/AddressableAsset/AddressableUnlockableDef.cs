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
                await SetAsset(unlockable);
            }
            else
            {
                await LoadFromAddress();
            }
        }
    }
}