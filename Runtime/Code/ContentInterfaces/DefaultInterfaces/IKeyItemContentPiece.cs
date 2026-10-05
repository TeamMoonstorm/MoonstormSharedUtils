using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// See <see cref="IContentPiece"/> and <see cref="IContentPiece{T}"/> for more information regarding Content Pieces
    /// <br></br>
    /// <br>A version of <see cref="IContentPiece{T}"/> used to represent a KeyItemDef for the game.</br>
    /// <br>It's module is the <see cref="KeyItemModule"/></br>
    /// </summary>
    public interface IKeyItemContentPiece : IContentPiece<KeyItemDef>
    {
        NullableRef<List<GameObject>> itemDisplayPrefabs { get; }
    }
}