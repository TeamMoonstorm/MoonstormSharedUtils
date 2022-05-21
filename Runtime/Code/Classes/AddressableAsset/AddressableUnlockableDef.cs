using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
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