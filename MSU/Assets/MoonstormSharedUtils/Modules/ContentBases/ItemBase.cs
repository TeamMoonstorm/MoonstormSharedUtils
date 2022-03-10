using RoR2;
using UnityEngine;

namespace Moonstorm
{
    public abstract class ItemBase : ContentBase
    {
        public abstract ItemDef ItemDef { get; }

        public virtual GameObject ItemDisplayPrefab { get; }
    }
}
