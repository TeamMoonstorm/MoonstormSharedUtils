using RoR2;
using UnityEngine;

namespace Moonstorm
{
    public abstract class BuffBase : ContentBase
    {
        public abstract BuffDef BuffDef { get; }

        public virtual Material OverlayMaterial { get; }

        public virtual void AddBehavior(ref CharacterBody body, int stack) { }
    }
}
