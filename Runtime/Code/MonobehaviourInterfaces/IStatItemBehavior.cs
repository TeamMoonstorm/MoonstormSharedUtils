namespace MSU
{
    /// <summary>
    /// An IStatItemBehavior is used by MSU's ContentManager to run the methods <see cref="RecalculateStatsEnd"/> and <see cref="RecalculateStatsStart"/>
    /// <para>Effectively allows you to run code before and after <see cref="RoR2.CharacterBody.RecalculateStats"/></para>
    /// <para>If your objective is to modify the stats directly, use <see cref="IBodyStatArgModifier"/> instead</para>
    /// <para>Intended to be used in a MonoBehavior that's added to the CharacterBody.</para>
    /// </summary>
    public interface IStatItemBehavior
    {
        /// <summary>
        /// Code ran here runs after RecalculateStats()
        /// </summary>
        void RecalculateStatsEnd();

        /// <summary>
        /// Code ran here runs before RecalculateStats()
        /// </summary>
        void RecalculateStatsStart();
    }
}