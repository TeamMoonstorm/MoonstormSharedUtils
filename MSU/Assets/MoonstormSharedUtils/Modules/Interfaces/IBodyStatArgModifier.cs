using R2API;

namespace Moonstorm
{
    public interface IBodyStatArgModifier
    {
        void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args);
    }
}