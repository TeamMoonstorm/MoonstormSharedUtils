using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections;
using UnityEngine;

namespace MSU
{
    internal static class Interfaces
    {
        internal static event Action<HealthComponent, DamageInfo> @event;
        [SystemInitializer]
        private static IEnumerator Init()
        {
            MSULog.Info("Setting up Interfaces");
            yield return null;
            IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
        }

        #region IOnIncomingDamageOtherserverReciever
        private static void HealthComponent_TakeDamageProcess(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            bool tryGotoNext = c.TryGotoNext(MoveType.After,
                x => x.MatchStfld<DamageInfo>("rejected"),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>("onIncomingDamageReceivers"));

            if (!tryGotoNext)
                MSULog.Fatal($"Failed to set up {nameof(IOnIncomingDamageOtherServerReciever)}!");

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

            @event?.Invoke(healthComponent, damageInfo);
        }
        #endregion;
    }
}
