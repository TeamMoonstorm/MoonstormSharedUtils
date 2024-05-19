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
    /// <br>A version of <see cref="IGameObjectContentPiece{T}"/> used to represent an Interactable for the game.</br>
    /// <br>It's module is the <see cref="InteractableModule"/></br>
    /// <br>Contains a property that's used to provide the stages that this interactable can spawn on using <see cref="InteractableCardProvider"/></br>
    /// </summary>
    public interface IInteractableContentPiece : IGameObjectContentPiece<IInteractable>
    {
        /// <summary>
        /// The <see cref="InteractableCardProvider"/> for this Interactable.
        /// </summary>
        NullableRef<InteractableCardProvider> CardProvider { get; }
    }
}