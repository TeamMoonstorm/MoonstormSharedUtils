using RoR2;

namespace Moonstorm
{
    /// <summary>
    /// <inheritdoc cref="CharacterBase"/>
    /// <para>A SurvivorBase also contains a <see cref="SurvivorDef"/>, used for making the body a survivor.</para>
    /// </summary>
    public abstract class SurvivorBase : CharacterBase
    {
        /// <summary>
        /// The <see cref="RoR2.SurvivorDef"/> for this SurvivorBase
        /// </summary>
        public abstract SurvivorDef SurvivorDef { get; }
    }
}