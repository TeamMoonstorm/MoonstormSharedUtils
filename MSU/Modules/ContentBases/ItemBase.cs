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
        /// Add an ItemBehavior to the body.
        /// <para>Use body.AddItemBehavior(stack)</para>
        /// <para>T must be a class that inherits from CharacterBody.ItemBehavior</para>
        /// <para>This class must implement all the functionality of the Item</para>
        /// </summary>
        /// <param name="body">The body which is getting affected by the Item</param>
        /// <param name="stack">The amount of stacks of the Item, this value automatically calculated</param>
        public virtual void AddBehavior(ref CharacterBody body, int stack) { }
    }
}
