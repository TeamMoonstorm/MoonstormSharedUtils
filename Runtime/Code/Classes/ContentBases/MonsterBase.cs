
using System;
using System.Collections.Generic;

namespace Moonstorm
{
    /// <summary>
    /// <inheritdoc cref="CharacterBase"/>
    /// <para>A MonsterBase also contains a <see cref="MonsterDirectorCards"/> List, used for spawning the monster ingame with the Combat Director</para>
    /// </summary>
    public abstract class MonsterBase : CharacterBase
    {
        [Obsolete("Use the MonsterDirectorCards list instead.")]
        public virtual MSMonsterDirectorCard MonsterDirectorCard { get; }

        /// <summary>
        /// A list of <see cref="MSMonsterDirectorCard"/> for this Monster.
        /// </summary>
        public virtual List<MSMonsterDirectorCard> MonsterDirectorCards { get; } = new List<MSMonsterDirectorCard>();
    }
}