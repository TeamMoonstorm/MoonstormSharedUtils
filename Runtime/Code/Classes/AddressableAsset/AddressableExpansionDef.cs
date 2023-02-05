using RoR2.ExpansionManagement;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    /// <summary>
    /// <inheritdoc cref="AddressableAsset{T}"/>
    /// The type of asset this references is an <see cref="ExpansionDef"/>, and can be loaded either via Addressables or the <see cref="ExpansionCatalog"/>
    /// </summary>
    [Serializable]
    public class AddressableExpansionDef : AddressableAsset<ExpansionDef>
    {
        protected override async Task LoadAsset()
        {
            ExpansionDef expansionDef = ExpansionCatalog.expansionDefs.FirstOrDefault(ed => ed.name == address);
            if (expansionDef != null)
            {
                asset = expansionDef;
            }
            else
            {
                await LoadFromAddress();
            }
        }

        /// <summary>
        /// Parameterless Constructor for <see cref="AddressableExpansionDef"/>
        /// </summary>
        public AddressableExpansionDef() { }
        /// <summary>
        /// Constructor for <see cref="AddressableExpansionDef"/> that sets the <see cref="ExpansionDef"/> asset.
        /// </summary>
        /// <param name="ed">The <see cref="ExpansionDef"/> for this <see cref="AddressableExpansionDef"/></param>
        public AddressableExpansionDef(ExpansionDef ed)
        {
            asset = ed;
            useDirectReference = true;
        }
        /// <summary>
        /// Constructor for <see cref="AddressableExpansionDef"/> that sets the address that'll load the asset
        /// </summary>
        /// <param name="addressOrExpansionDefName">The Address for the <see cref="ExpansionDef"/>, this can also be the asset's name so it can load via the <see cref="ExpansionCatalog"/></param>
        public AddressableExpansionDef(string addressOrExpansionDefName)
        {
            address = addressOrExpansionDefName;
            useDirectReference = false;
        }
    }
}
