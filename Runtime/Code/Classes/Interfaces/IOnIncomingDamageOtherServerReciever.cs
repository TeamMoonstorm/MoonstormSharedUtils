using RoR2;

namespace Moonstorm
{
    public interface IOnIncomingDamageOtherServerReciever
    {
        void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo);
    }
}
