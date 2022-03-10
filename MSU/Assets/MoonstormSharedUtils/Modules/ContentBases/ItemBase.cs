using RoR2;
using UnityEngine;

namespace Moonstorm
{
    public abstract class ItemBase : ContentBase
    {
        public abstract ItemDef ItemDef { get; set; }

        public virtual GameObject ItemDisplayPrefab { get; }

        public virtual void AddBehavior(ref CharacterBody body, int stack) { }
    }
}
