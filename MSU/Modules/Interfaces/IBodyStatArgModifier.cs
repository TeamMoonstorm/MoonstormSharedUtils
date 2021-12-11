using R2API;

namespace Moonstorm
{
    /// <summary>
    /// Interface for modifying a body's stats. Uses R2API's RecalculateStatsAPI
    /// </summary>
    public interface IBodyStatArgModifier
    {
        /// <summary>
        /// Modify the Recalculate Stats arguments
        /// </summary>
        /// <param name="args">The Stat Modifiers</param>
        void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args);
    }
}