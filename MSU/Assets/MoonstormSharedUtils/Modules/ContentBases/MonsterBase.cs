
namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for inittializing a Monster
    /// <para>Inherits from CharacterBase</para>
    /// </summary>
    public abstract class MonsterBase : CharacterBase
    {
        /// <summary>
        /// Your Monster's Monster Director Card
        /// </summary>
        public abstract MSMonsterDirectorCard MonsterDirectorCard { get; set; }
    }
}