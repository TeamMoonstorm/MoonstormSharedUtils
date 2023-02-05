using RoR2;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    /// <summary>
    /// <inheritdoc cref="AddressableAsset{T}"/>
    /// The type of asset this refereces is an <see cref="EliteDef"/>, and can be loaded either via Addressables or the <see cref="EliteCatalog"/>
    /// </summary>
    [Serializable]
    public class AddressableEliteDef : AddressableAsset<EliteDef>
    {
        protected override async Task LoadAsset()
        {
            EliteDef def = EliteCatalog.eliteDefs.FirstOrDefault(x => x.name.Equals(address, StringComparison.OrdinalIgnoreCase));
            if (def != null)
            {
                asset = def;
            }
            else
            {
                await LoadFromAddress();
            }
        }

        /// <summary>
        /// Parameterless Constructor for <see cref="AddressableEliteDef"/>
        /// </summary>
        public AddressableEliteDef() { }
        /// <summary>
        /// Constructor for <see cref="AddressableEliteDef"/> that sets the <see cref="EliteDef"/> asset.
        /// </summary>
        /// <param name="ed">The <see cref="EliteDef"/> for this <see cref="AddressableEliteDef"/></param>
        public AddressableEliteDef(EliteDef ed)
        {
            asset = ed;
            useDirectReference = true;
        }
        /// <summary>
        /// Constructor for <see cref="AddressableEliteDef"/> that sets the address that'll load the asset
        /// </summary>        
        /// <param name="addressOrEliteDefName">The Address for the <see cref="EliteDef"/>, this can also be the asset's name so it can load via the <see cref="EliteCatalog"/></param>
        public AddressableEliteDef(string addressOrEliteDefName)
        {
            address = addressOrEliteDefName;
            useDirectReference = false;
        }
    }
}