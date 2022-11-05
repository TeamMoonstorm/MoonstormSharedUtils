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
    /// The type of asset this references is an <see cref="ItemDef"/> and can be loaded either via Addressables or the <see cref="ItemCatalog"/>
    /// </summary>
    [Serializable]
    public class AddressableItemDef : AddressableAsset<ItemDef>
    {
        protected override async Task LoadAsset()
        {
            ItemIndex index = ItemCatalog.FindItemIndex(address);
            if(index != ItemIndex.None)
            {
                asset = ItemCatalog.GetItemDef(index);
            }
            else
            {
                await LoadFromAddress();
            }
        }


        /// <summary>
        /// Parameterless Constructor for <see cref="AddressableItemDef"/>
        /// </summary>
        public AddressableItemDef() { }
        /// <summary>
        /// Constructor for <see cref="AddressableItemDef"/> that sets the <see cref="ItemDef"/> asset.
        /// </summary>
        /// <param name="id">The <see cref="ItemDef"/> for this <see cref="AddressableItemDef"/></param>
        public AddressableItemDef(ItemDef id)
        {
            asset = id;
            useDirectReference = true;
        }
        /// <summary>
        /// Constructor for <see cref="AddressableItemDef"/> that sets the address that'll load the asset
        /// </summary>
        /// <param name="addressOrItemDefName">The Address for the <see cref="ItemDef"/>, this can also be the asset's name so it can load via the <see cref="ItemCatalog"/></param>
        public AddressableItemDef(string addressOrItemDefName)
        {
            address = addressOrItemDefName;
            useDirectReference = false;
        }
    }
}
