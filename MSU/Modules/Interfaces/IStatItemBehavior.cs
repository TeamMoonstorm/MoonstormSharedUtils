using System;

namespace Moonstorm
{
    /// <summary>
    /// Interface used for RecalculateStats
    /// </summary>
    [Obsolete("The IStatItemBehavior interface is deprecated, use IBodyStatArgModifier instead.")]
    public interface IStatItemBehavior
    {
        /// <summary>
        /// Code in here is the same as writing your code after orig(self)
        /// </summary>
        void RecalculateStatsEnd();

        /// <summary>
        /// Code in here is the same as writing your code before orig(self)
        /// </summary>
        void RecalculateStatsStart();
    }
}