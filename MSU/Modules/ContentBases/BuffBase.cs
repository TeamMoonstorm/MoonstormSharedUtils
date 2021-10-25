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
        /// Add an item behavior to your buff.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="stack"></param>
        public virtual void AddBehavior(ref CharacterBody body, int stack) { }
    }
}
