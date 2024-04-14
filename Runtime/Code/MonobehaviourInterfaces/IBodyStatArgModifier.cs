using R2API;

namespace MSU
{
    /// <summary>
    /// An IBodyStatArgModifier can be used to modify the Stats of a characterbody using <see cref="RecalculateStatsAPI"/>
    /// <para>Intended to be used in MonoBehaviours that are added to the CharacterBody.</para>
    /// </summary>
    public interface IBodyStatArgModifier
    {
        /// <summary>
        /// Modify the stat arguments for this body.
        /// NEVER override, only add, substract, divide, multiply or modulo.
        /// </summary>
        /// <param name="args">The arguments to modify</param>
        void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args);
    }
}