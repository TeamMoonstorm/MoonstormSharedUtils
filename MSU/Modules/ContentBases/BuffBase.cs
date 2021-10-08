using RoR2;

namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing a Buff
    /// </summary>
    public abstract class BuffBase
    {
        /// <summary>
        /// Your Buff's BuffDef
        /// </summary>
        public abstract BuffDef BuffDef { get; set; }

        public BuffBase() { }

        /// <summary>
        /// Initialize your Buff
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Add an item behavior to your buff.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="stack"></param>
        public virtual void AddBehavior(ref CharacterBody body, int stack) { }
    }
}
