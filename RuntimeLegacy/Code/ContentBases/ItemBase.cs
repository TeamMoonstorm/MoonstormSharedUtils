using RoR2;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="ContentBase"/> that represents an <see cref="RoR2.ItemDef"/> for the game, the Item is represented via the <see cref="ItemDef"/>
    /// <para>Its tied module base is the <see cref="ItemModuleBase"/></para>
    /// <para>You should also know about the <see cref="RoR2.Items.BaseItemBodyBehavior"/>"/></para>
    /// </summary>
    public abstract class ItemBase : ContentBase
    {
        /// <summary>
        /// The ItemDef that represents this ItemBase
        /// </summary>
        public abstract ItemDef ItemDef { get; }

        /// <summary>
        /// Optional, supply your item's ItemDisplayPrefab here
        /// </summary>
        public virtual GameObject ItemDisplayPrefab { get; }
    }
}
