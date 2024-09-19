using RoR2;
using System.Collections.Generic;

namespace MSU
{
    /// <summary>
    /// See <see cref="IContentPiece"/> and <see cref="IContentPiece{T}"/> for more information regarding Content Pieces
    /// <br></br>
    /// <br>A version of <see cref="IEquipmentContentPiece"/> used to represent an Elite Equipment for the game.</br>
    /// <br>It's module is the <see cref="EquipmentModule"/></br>
    /// <br>Elites added via IEliteContentPiece and using <see cref="ExtendedEliteDef"/> will have their <see cref="ExtendedEliteDef.overlayMaterial"/>, <see cref="ExtendedEliteDef.eliteRamp"/>, and <see cref="ExtendedEliteDef.effect"/> working automatically.</br>
    /// </summary>
    public interface IEliteContentPiece : IEquipmentContentPiece
    {
        /// <summary>
        /// The Elites that represent this Elite Content Piece, this is a list to allow Tier1 elites, which have both a normal and honor def.
        /// </summary>
        List<EliteDef> eliteDefs { get; }
    }
}