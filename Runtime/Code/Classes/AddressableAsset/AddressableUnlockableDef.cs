using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    [Obsolete("Use R2API's AddressReferencedUnlockableDef Instead")]
    [Serializable]
    public class AddressableUnlockableDef : AddressableAsset<UnlockableDef>
    {
        protected override async Task LoadAsset()
        {
            UnlockableDef unlockable = UnlockableCatalog.GetUnlockableDef(address);
            if (unlockable != null)
            {
                asset = unlockable;
            }
            else
            {
                await LoadFromAddress();
            }
        }

        public static implicit operator AddressReferencedUnlockableDef(AddressableUnlockableDef bd)
        {
            if (bd.asset)
                return new AddressReferencedUnlockableDef(bd.asset);
            else
                return new AddressReferencedUnlockableDef(bd.address);
        }

        public AddressableUnlockableDef() { }
        public AddressableUnlockableDef(UnlockableDef ud)
        {
            asset = ud;
            useDirectReference = true;
        }
        public AddressableUnlockableDef(string addressOrUnlockableDefName)
        {
            address = addressOrUnlockableDefName;
            useDirectReference = false;
        }
    }
}