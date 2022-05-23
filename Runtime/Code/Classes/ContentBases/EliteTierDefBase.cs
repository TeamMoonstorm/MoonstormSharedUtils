using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm
{
    public abstract class EliteTierDefBase : ContentBase
    {
        public abstract SerializableEliteTierDef SerializableEliteTierDef { get; }

        public abstract Func<SpawnCard.EliteRules, bool> IsAvailable { get; }
    }
}