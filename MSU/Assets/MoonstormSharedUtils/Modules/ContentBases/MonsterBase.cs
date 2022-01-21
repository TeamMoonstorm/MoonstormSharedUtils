
namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for inittializing a Monster
    /// <para>Inherits from CharacterBase</para>
    /// <para>Not fully implemented</para>
    /// </summary>
    public abstract class MonsterBase : CharacterBase
    {
        public abstract MSMonsterDirectorCard MonsterDirectorCard { get; set; }
    }
}