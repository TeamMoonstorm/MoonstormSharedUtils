using RoR2;

namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing an Item
    /// </summary>
    public abstract class ItemBase
    {
        /// <summary>
        /// Your Item's ItemDef
        /// </summary>
        public abstract ItemDef ItemDef { get; set; }

        public static ItemDef itemDef;
        /// <summary>
        /// Initialize your item here
        /// <para>Running base.Initialize() REQUIRED</para>
        /// </summary>
        public virtual void Initialize()
        {
            ItemDef = ItemDef;
        }

        /// <summary>
        /// Add your item behavior here
        /// </summary>
        /// <param name="body"></param>
        /// <param name="stack"></param>
        public virtual void AddBehavior(ref CharacterBody body, int stack) { }
    }
}
