using R2API;
using RoR2;

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
        /// <param name="sender">The body that's being modified</param>
        /// <param name="args">The Stat Modifiers</param>
        void ModifyStatArguments(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args);
    }
}