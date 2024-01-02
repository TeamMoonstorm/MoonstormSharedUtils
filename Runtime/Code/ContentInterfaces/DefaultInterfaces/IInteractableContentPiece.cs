using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    public interface IInteractableContentPiece : IGameObjectContentPiece<IInteractable>
    {
        InteractableCardProvider CardProvider { get; }
    }
}