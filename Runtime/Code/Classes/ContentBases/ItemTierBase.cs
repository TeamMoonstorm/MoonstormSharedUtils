using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using R2API.ScriptableObjects;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="ContentBase"/> that represents an <see cref="RoR2.ItemTierDef"/> for the game, the ItemTierDef is represented via the <see cref="ItemTierDef"/>
    /// <para>Its tied module base is the <see cref="ItemTierModuleBase"/></para>
    /// </summary>
    public abstract class ItemTierBase : ContentBase
    {
        /// <summary>
        /// Optional, if supplied, it'll override the <see cref="ItemTierDef.colorIndex"/> for this custom color.
        /// </summary>
        public virtual SerializableColorCatalogEntry ColorIndex { get; }
        /// <summary>
        /// Optional, if supplied, it'll override the <see cref="ItemTierDef.darkColorIndex"/> for this custom color.
        /// </summary>
        public virtual SerializableColorCatalogEntry DarkColorIndex { get; }

        /// <summary>
        /// The ItemTierDef that represents this ItemTierBase
        /// </summary>
        public abstract ItemTierDef ItemTierDef { get; }

        /// <summary>
        /// The pickup display for this ItemTierDef, note that this is not the droplet VFX, but the VFX that appears when the item's pickup is in the world.
        /// </summary>
        public abstract GameObject PickupDisplayVFX { get; }

        /// <summary>
        /// A list of all the items that have this Tier
        /// </summary>
        public List<ItemIndex> ItemsWithThisTier { get; internal set; } = new List<ItemIndex>();

        /// <summary>
        /// A list of the available items that can drop in the current run, returns null or an outdated list if a run is not active.
        /// </summary>
        public List<PickupIndex> AvailableTierDropList { get; internal set; } = new List<PickupIndex>();
    }
}