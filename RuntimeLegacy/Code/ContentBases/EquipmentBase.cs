using RoR2;
using UnityEngine;

namespace Moonstorm
{
    public abstract class EquipmentBase : ContentBase
    {
        public abstract EquipmentDef EquipmentDef { get; }

        public virtual GameObject ItemDisplayPrefab { get; }

        public virtual void AddBehavior(ref CharacterBody body, int stack) { }

        public virtual bool FireAction(EquipmentSlot slot) => false;
    }
}
