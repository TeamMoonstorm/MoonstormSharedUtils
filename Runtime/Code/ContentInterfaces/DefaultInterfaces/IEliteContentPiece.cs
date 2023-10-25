using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm
{
    public interface IEliteContentPiece : IContentPiece
    {
        List<EliteDef> EliteDefs { get; }
    }
}