using RoR2;

namespace Moonstorm
{
    public abstract class ItemBase : ContentBase
    {
        public abstract ItemDef ItemDef { get; set; }
        public virtual void AddBehavior(ref CharacterBody body, int stack) { }
    }
}
