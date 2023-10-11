
using R2API;
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
        /// <summary>
        /// Represents if the Monster is available for a DCCS
        /// </summary>
        /// <returns>true if the Monster should be added, false otherwise</returns>
        public delegate bool IsAvailableForDCCSDelegate(DirectorAPI.StageInfo stageInfo);

        /// <summary>
        /// A list of <see cref="MSMonsterDirectorCard"/> for this Monster.
        /// </summary>
        public virtual List<MSMonsterDirectorCard> MonsterDirectorCards { get; } = new List<MSMonsterDirectorCard>();
        /// <summary>
        /// Whenever the DCCS are being modified to have the custom monster, this delegate is invoked, return True if you want the monster to be added, or False if you dont want the monster to be added.
        /// </summary>
        public virtual IsAvailableForDCCSDelegate IsAvailableForDCCS { get; } = DefaultIsAvailable;

        private static bool DefaultIsAvailable(DirectorAPI.StageInfo stageInfo) => true;
    }
}