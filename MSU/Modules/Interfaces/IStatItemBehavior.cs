namespace Moonstorm
{
    /// <summary>
    /// Interface used for RecalculateStats
    /// <para>Should only be used in ItemBehaviors</para>
    /// <para>This interface is best for modifying stats not supported by IBodyStatArgModifier, or for getting the values of stats after recalculation is finished</para>
    /// </summary>
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