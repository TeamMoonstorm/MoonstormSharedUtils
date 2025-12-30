using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MSU
{
    /// <summary>
    /// See <see cref="IContentPiece"/> and <see cref="IContentPiece{T}"/> for more information regarding Content Pieces
    /// <br></br>
    /// <br>A version of <see cref="IContentPiece{T}"/> used to represent an Item for the game.</br>
    /// <br>It's module is the <see cref="ItemModule"/></br>
    /// <br>Items with it's <see cref="ItemDef.deprecatedTier"/> set to <see cref="ItemTier.Boss"/> will be automatically added to the <see cref="DLC1Content.Items.VoidMegaCrabItem"/> transmutation pool</br>
    /// <br>If you're looking to create a Void Item, use <see cref="IVoidItemContentPiece"/> instead.</br>
    /// </summary>
    public interface IItemContentPiece : IContentPiece<ItemDef>
    {
        /// <summary>
        /// The ItemDisplayPrefabs for this Item, can be null.
        /// </summary>
        NullableRef<List<GameObject>> itemDisplayPrefabs { get; }

        /// <summary>
        /// <b>Optional Interface Implementation</b>
        /// <br></br>
        /// This method allows you to associate a <see cref="BaseItemBodyBehavior"/> with the <see cref="ItemDef"/> that's obtained via this instance of the interface.
        /// <br></br>
        /// Assuming that the returned Type inherits from <see cref="BaseItemBodyBehavior"/>, it gets added with the rest of the game's body behaviours, without logging errors associated with item availability.
        /// </summary>
        /// <returns>A Type, specifically the BaseItemBodyBehaviour associated with this item.</returns>
        public Type GetBehaviourType() => null;
    }
}
