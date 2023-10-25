using R2API;

namespace Moonstorm
{
    /// <summary>
    /// An IBodyStatArgModifier can be used to modify the Stats of a characterbody using <see cref="RecalculateStatsAPI"/>
    /// </summary>
    public interface IBodyStatArgModifier
    {
        /// <summary>
        /// Modify the stat arguments for this body.
        /// NEVER override, only add, substract, divide or multiply.
        /// </summary>
        /// <param name="args">The arguments to modify</param>
        void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args);
    }
}