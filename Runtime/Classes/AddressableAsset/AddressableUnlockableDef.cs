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
        protected override void LoadAsset()
        {
            UnlockableDef unlockable = UnlockableCatalog.GetUnlockableDef(address);
            if(unlockable != null)
            {
                SetAsset(unlockable);
            }
            else
            {
                LoadFromAddress();
            }
        }
    }
}