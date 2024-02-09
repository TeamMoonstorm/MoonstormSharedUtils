
using R2API;
using System;
using System.Collections.Generic;

namespace Moonstorm
{
    public abstract class MonsterBase : CharacterBase
    {
        public delegate bool IsAvailableForDCCSDelegate(DirectorAPI.StageInfo stageInfo);

        public virtual List<MSMonsterDirectorCard> MonsterDirectorCards { get; } = new List<MSMonsterDirectorCard>();

        public virtual IsAvailableForDCCSDelegate IsAvailableForDCCS { get; } = DefaultIsAvailable;

        private static bool DefaultIsAvailable(DirectorAPI.StageInfo stageInfo) => true;
    }
}