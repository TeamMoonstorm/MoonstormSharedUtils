using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;

namespace MSU
{
    static class Interfaces
    {
        [SystemInitializer]
        private static void Init()
        {
            MSULog.Info("Setting up Interfaces");
            IL.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        #region IOnIncomingDamageOtherserverReciever
        private static void HealthComponent_TakeDamage(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                x => x.MatchStfld<DamageInfo>("rejected"),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>("onIncomingDamageReceivers"));
            c.Index -= 1;

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<HealthComponent, DamageInfo>>(RunOnIncomingDamageOther);

#if DEBUG
            MSULog.Debug($"{nameof(IOnIncomingDamageOtherServerReciever)} succesfully set up.");
#endif
        }

        private static void RunOnIncomingDamageOther(HealthComponent healthComponent, DamageInfo damageInfo)
        {
            if (!damageInfo.attacker)
                return;
            IOnIncomingDamageOtherServerReciever[] interfaces = damageInfo.attacker.GetComponents<IOnIncomingDamageOtherServerReciever>();
            for (int i = 0; i < interfaces.Length; i++)
                interfaces[i].OnIncomingDamageOther(healthComponent, damageInfo);
        }
        #endregion;
    }
}
