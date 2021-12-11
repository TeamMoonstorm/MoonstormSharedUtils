namespace Moonstorm
{
    /// <summary>
    /// Interface used for RecalculateStats
    /// <para>You should probably use IBodyStatArgModifier instead.</para>
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