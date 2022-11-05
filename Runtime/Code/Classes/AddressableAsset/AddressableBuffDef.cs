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
    /// The type of asset this references is a <see cref="BuffDef"/>, and can be loaded either via Addressables or the <see cref="BuffCatalog"/>
    /// </summary>
    [Serializable]
    public class AddressableBuffDef : AddressableAsset<BuffDef>
    {
        protected override async Task LoadAsset()
        {
            BuffIndex index = BuffCatalog.FindBuffIndex(address);
            if(index != BuffIndex.None)
            {
                asset = BuffCatalog.GetBuffDef(index);
            }
            else
            {
                await LoadFromAddress();
            }
        }

        /// <summary>
        /// Parameterless Constructor for <see cref="AddressableBuffDef"/>
        /// </summary>
        public AddressableBuffDef() { }
        /// <summary>
        /// Constructor for <see cref="AddressableBuffDef"/> that sets the <see cref="BuffDef"/> asset.
        /// </summary>
        /// <param name="bd">The <see cref="BuffDef"/> for this <see cref="AddressableBuffDef"/></param>
        public AddressableBuffDef(BuffDef bd)
        {
            asset = bd;
            useDirectReference = true;
        }
        /// <summary>
        /// Constructor for <see cref="AddressableBuffDef"/> that sets the address that'll load the asset
        /// </summary>
        /// <param name="addressOrBuffDefName">The Address for the <see cref="BuffDef"/>, this can also be the asset's name so it can load via the <see cref="BuffCatalog"/></param>
        public AddressableBuffDef(string addressOrBuffDefName)
        {
            address = addressOrBuffDefName;
            useDirectReference = false;
        }
    }
}
