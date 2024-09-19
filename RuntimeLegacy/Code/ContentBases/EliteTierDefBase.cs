using RoR2;
using System;

namespace Moonstorm
{
    public abstract class EliteTierDefBase : ContentBase
    {
        public abstract SerializableEliteTierDef SerializableEliteTierDef { get; }

        public abstract Func<SpawnCard.EliteRules, bool> IsAvailable { get; }
    }
}