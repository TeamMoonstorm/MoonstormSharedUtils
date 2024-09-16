using JetBrains.Annotations;
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
    }
}
