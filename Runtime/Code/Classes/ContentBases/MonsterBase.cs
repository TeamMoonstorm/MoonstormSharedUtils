
namespace Moonstorm
{
    /// <summary>
    /// <inheritdoc cref="CharacterBase"/>
    /// <para>A MonsterBase also contains a <see cref="MonsterDirectorCard"/>, used for spawning the monster ingame with the Combat Director</para>
    /// </summary>
    public abstract class MonsterBase : CharacterBase
    {
        /// <summary>
        /// The <see cref="MSMonsterDirectorCard"/> for this Monster
        /// </summary>
        public abstract MSMonsterDirectorCard MonsterDirectorCard { get; }
    }
}