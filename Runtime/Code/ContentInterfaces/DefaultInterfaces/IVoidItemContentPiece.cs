using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    public interface IVoidItemContentPiece : IItemContentPiece
    {
        List<ItemDef> GetInfectableItems();
    }
}