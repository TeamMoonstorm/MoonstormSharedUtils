using RoR2;

namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing a Buff
    /// </summary>
    public abstract class BuffBase : ContentBase
    {
        /// <summary>
        /// Your Buff's BuffDef
        /// </summary>
        public abstract BuffDef BuffDef { get; set; }

        /// <summary>
        /// Add an ItemBehavior to the body.
        /// <para>Use body.AddItemBehavior(stack)</para>
        /// <para>T must be a class that inherits from CharacterBody.ItemBehavior</para>
        /// <para>This class must implement all the functionality of the buff</para>
        /// </summary>
        /// <param name="body">The body which is getting affected by the buff</param>
        /// <param name="stack">The amount of stacks of the buff calculated automatically by the mod</param>
        public virtual void AddBehavior(ref CharacterBody body, int stack) { }
    }
}
