using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;

namespace Moonstorm
{
    static class Interfaces
    {
        [SystemInitializer]
        private static void Init()
        {
            MSULog.LogI("Setting up Interfaces");
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

            MSULog.LogD($"{nameof(IOnIncomingDamageOtherServerReciever)} succesfully set up.");
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
