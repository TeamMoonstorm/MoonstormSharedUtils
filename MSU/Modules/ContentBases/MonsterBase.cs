
namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for inittializing a Monster
    /// <para>Inherits from CharacterBase</para>
    /// </summary>
    public abstract class MonsterBase : CharacterBase
    {
        /// <summary>
        /// Al the Monster director cards
        /// </summary>
        public static MSMonsterDirectorCardHolder[] enabledMonsterCards;

        /// <summary>
        /// Your Monster's Director Cards.
        /// </summary>
        public abstract MSMonsterDirectorCardHolder directorCards { get; set; }
    }
}