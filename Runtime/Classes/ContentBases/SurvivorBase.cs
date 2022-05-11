using RoR2;

namespace Moonstorm
{
    public abstract class SurvivorBase : CharacterBase
    {
        public abstract SurvivorDef SurvivorDef { get; }
    }
}