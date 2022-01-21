using R2API;

namespace Moonstorm
{
    /// <summary>
    /// Interface for modifying a body's stats. Uses R2API's RecalculateStatsAPI
    /// <para>Should be used only in ItemBehaviors</para>
    /// </summary>
    public interface IBodyStatArgModifier
    {
        /// <summary>
        /// Modify the StatArguments that are going to be added to the body
        /// </summary>
        /// <param name="args">The arguments themselves, NEVER overwrite the fields, only Add, Divide, Multiply or Substract them.</param>
        void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args);
    }
}