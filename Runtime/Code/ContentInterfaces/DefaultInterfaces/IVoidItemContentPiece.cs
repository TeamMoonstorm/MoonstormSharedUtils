using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    /// <summary>
    /// See <see cref="IContentPiece"/> and <see cref="IContentPiece{T}"/> for more information regarding Content Pieces
    /// <br></br>
    /// <br>A version of <see cref="IItemContentPiece"/> used to represent a Void Item for the game.</br>
    /// <br>It's module is the <see cref="ItemModule"/></br>
    /// <br>Items added via this interface will have new Transmutation entries, causing all the ItemDefs listed by <see cref="GetInfectableItems"/> to transform into this Item.</br>
    public interface IVoidItemContentPiece : IItemContentPiece
    {
        /// <summary>
        /// Obtains a list of all the ItemDefs this Void Item should Infect
        /// </summary>
        /// <returns>A List of Items to infect</returns>
        List<ItemDef> GetInfectableItems();
    }
}