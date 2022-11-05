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
    /// The type of asset this references is an <see cref="EquipmentDef"/>, and can be loaded either via Addressables or the <see cref="EquipmentCatalog"/>
    /// </summary>
    [Serializable]
    public class AddressableEquipmentDef : AddressableAsset<EquipmentDef>
    {
        protected override async Task LoadAsset()
        {
            EquipmentIndex index = EquipmentCatalog.FindEquipmentIndex(address);
            if(index != EquipmentIndex.None)
            {
                asset = EquipmentCatalog.GetEquipmentDef(index);
            }
            else
            {
                await LoadFromAddress();
            }
        }

        /// <summary>
        /// Parameterless Constructor for <see cref="AddressableEquipmentDef"/>
        /// </summary>
        public AddressableEquipmentDef() { }
        /// <summary>
        /// Constructor for <see cref="AddressableEquipmentDef"/> that sets the <see cref="EquipmentDef"/> asset.
        /// </summary>
        /// <param name="ed">The <see cref="EquipmentDef"/> for this <see cref="AddressableEquipmentDef"/></param>
        public AddressableEquipmentDef(EquipmentDef ed)
        {
            asset = ed;
            useDirectReference = true;
        }
        /// <summary>
        /// Constructor for <see cref="AddressableEquipmentDef"/> that sets the address that'll load the asset
        /// </summary>
        /// <param name="addressOrEquipmentDefName">The Address for the <see cref="EquipmentDef"/>, this can also be the asset's name so it can load via the <see cref="EquipmentCatalog"/></param>
        public AddressableEquipmentDef(string addressOrEquipmentDefName)
        {
            address = addressOrEquipmentDefName;
            useDirectReference = false;
        }
    }
}