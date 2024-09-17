using RoR2;

namespace Moonstorm
{
    static class Interfaces
    {
        [SystemInitializer]
        private static void Init()
        {
            MSULog.Info("Setting up Interfaces");
            MSU.Interfaces.@event += RunOnIncomingDamageOther;
        }

        private static void RunOnIncomingDamageOther(HealthComponent healthComponent, DamageInfo damageInfo)
        {
            if (!damageInfo.attacker)
                return;
            IOnIncomingDamageOtherServerReciever[] interfaces = damageInfo.attacker.GetComponents<IOnIncomingDamageOtherServerReciever>();
            for (int i = 0; i < interfaces.Length; i++)
                interfaces[i].OnIncomingDamageOther(healthComponent, damageInfo);
        }
    }
}
