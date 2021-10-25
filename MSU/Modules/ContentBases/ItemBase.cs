using RoR2;

namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing an Item
    /// </summary>
    public abstract class ItemBase : ContentBase
    {
        /// <summary>
        /// Your Item's ItemDef
        /// </summary>
        public abstract ItemDef ItemDef { get; set; }

        /// <summary>
        /// Add your item behavior here
        /// </summary>
        /// <param name="body"></param>
        /// <param name="stack"></param>
        public virtual void AddBehavior(ref CharacterBody body, int stack) { }
    }
}
