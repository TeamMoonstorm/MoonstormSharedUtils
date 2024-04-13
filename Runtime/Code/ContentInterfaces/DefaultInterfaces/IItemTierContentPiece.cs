using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// See <see cref="IContentPiece"/> and <see cref="IContentPiece{T}"/> for more information regarding Content Pieces
    /// <br></br>
    /// <br>A version of <see cref="IContentPiece{T}"/> used to represent an ItemTier for the game.</br>
    /// <br>It's module is the <see cref="ItemTierModule"/></br>
    /// <para>Item Tiers added via this interface get different utilities to easily obtain the ItemIndices that use this tier (<see cref="ItemsWithThisTier"/>), and a list ov Available items that can be obtained (<see cref="AvailableTierDropList"/>)</para>
    /// <br>Item Tiers added via this interface will also be able to display unique VFX for their PickupDisplays (<see cref="PickupDisplayVFX"/>)</br>
    /// </summary>
    public interface IItemTierContentPiece : IContentPiece<ItemTierDef>
    {
        /// <summary>
        /// The ColorIndex for this ItemTier, If provided, the <see cref="ItemTierModule"/> will set the ItemTier's <see cref="ItemTierDef.colorIndex"/> to the index assigned by this <see cref="SerializableColorCatalogEntry"/>. Can be Null
        /// </summary>
        NullableRef<SerializableColorCatalogEntry> ColorIndex { get; }

        /// <summary>
        /// The DarkColorIndex for this ItemTier, If provided, the <see cref="ItemTierModule"/> will set the ItemTier's <see cref="ItemTierDef.darkColorIndex"/> to the index assigned by this <see cref="SerializableColorCatalogEntry"/>. Can be Null
        /// </summary>
        NullableRef<SerializableColorCatalogEntry> DarkColorIndex { get; }

        /// <summary>
        /// The PickupDisplay visual effects used for an item with this tier.
        /// </summary>
        GameObject PickupDisplayVFX { get; }

        /// <summary>
        /// Contains all the ItemIndices of items that use this ItemTier
        /// </summary>
        List<ItemIndex> ItemsWithThisTier { get; set; }

        /// <summary>
        /// Contains all the available items for the current run.
        /// </summary>
        List<PickupIndex> AvailableTierDropList { get; set; }
    }
}
