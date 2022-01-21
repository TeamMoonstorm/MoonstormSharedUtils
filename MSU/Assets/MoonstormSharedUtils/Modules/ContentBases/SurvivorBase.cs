using RoR2;

namespace Moonstorm
{
    /// <summary>
    /// A Content Class for initializing a Survivor
    /// <para>Inherits from CharacterBase</para>
    /// </summary>
    public abstract class SurvivorBase : CharacterBase
    {
        /// <summary>
        /// Your Survivor's survivorDef
        /// </summary>
        public abstract SurvivorDef SurvivorDef { get; set; }
    }
}