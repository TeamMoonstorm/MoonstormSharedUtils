using RoR2;

namespace Moonstorm
{
    public abstract class EquipmentBase : ContentBase
    {
        public abstract EquipmentDef EquipmentDef { get; set; }

        public virtual void AddBehavior(ref CharacterBody body, int stack)
        {
        }

        public virtual bool FireAction(EquipmentSlot slot)
        {
            return false;
        }
    }
}
