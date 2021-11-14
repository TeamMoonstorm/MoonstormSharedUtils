using R2API;
using RoR2;

namespace Moonstorm
{
    public interface IRecalculateStats
    {
        void StatModifiers(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args);
    }
}