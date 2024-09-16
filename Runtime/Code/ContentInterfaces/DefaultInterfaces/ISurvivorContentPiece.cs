using MSU;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    /// <summary>
    /// See <see cref="IGameObjectContentPiece{T}"/>, <see cref="IContentPiece"/> and <see cref="IContentPiece{T}"/> for more information regarding Content Pieces
    /// <br></br>
    /// <br>A version of <see cref="ICharacterContentPiece"/> used to represent a Survivor for the Game</br>
    /// <br>It's module is the <see cref="CharacterModule"/></br>
    public interface ISurvivorContentPiece : ICharacterContentPiece
    {
        /// <summary>
        /// The survivor's SurvivorDef
        /// </summary>
        SurvivorDef survivorDef { get; }
    }
}
