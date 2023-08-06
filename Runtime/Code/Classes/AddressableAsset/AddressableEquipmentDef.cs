using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    [Obsolete("Use R2API's AddressReferencedEquipmentDef Instead")]
    [Serializable]
    public class AddressableEquipmentDef : AddressableAsset<EquipmentDef>
    {
        protected override async Task LoadAsset()
        {
            EquipmentIndex index = EquipmentCatalog.FindEquipmentIndex(address);
            if (index != EquipmentIndex.None)
            {
                asset = EquipmentCatalog.GetEquipmentDef(index);
            }
            else
            {
                await LoadFromAddress();
            }
        }

        public static implicit operator AddressReferencedEquipmentDef(AddressableEquipmentDef bd)
        {
            if (bd.asset)
                return new AddressReferencedEquipmentDef(bd.asset);
            else
                return new AddressReferencedEquipmentDef(bd.address);
        }

        public AddressableEquipmentDef() { }
        public AddressableEquipmentDef(EquipmentDef ed)
        {
            asset = ed;
            useDirectReference = true;
        }
        public AddressableEquipmentDef(string addressOrEquipmentDefName)
        {
            address = addressOrEquipmentDefName;
            useDirectReference = false;
        }
    }
}