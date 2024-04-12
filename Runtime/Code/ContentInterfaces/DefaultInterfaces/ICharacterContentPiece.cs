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
    /// See <see cref="IGameObjectContentPiece{T}"/>, <see cref="IContentPiece"/> and <see cref="IContentPiece{T}"/> for more information regarding Content Pieces
    /// <br></br>
    /// <br>A version of <see cref="IGameObjectContentPiece{T}"/> used to represent a generic CharacterBody for the game.</br>
    /// <br>Its module is the <see cref="CharacterModule"/></br>
    /// </summary>
    public interface ICharacterContentPiece : IGameObjectContentPiece<CharacterBody>
    {
        /// <summary>
        /// The Character's Master Prefab, this is added to your ContentPack's CharacterMasters array. Can be null.
        /// </summary>
        NullableRef<GameObject> MasterPrefab { get; }
    }
}