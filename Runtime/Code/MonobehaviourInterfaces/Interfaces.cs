using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSU
{
    internal static class Interfaces
    {
        [SystemInitializer]
        private static IEnumerator Init()
        {
            MSULog.Info("Setting up Interfaces");
            yield return null;
            IL.RoR2.HealthComponent.TakeDamageProcess += SetupOnIncomingDamageOtherServerReciever;
        }

        #region IOnIncomingDamageOtherserverReciever
        /*
         * This interface is meant to allow an "attacker" to modify the incoming damage of a "soon to be victim".
         * We're doing this by putting our cursor right after the IOnIncomingDamageServerReceiver[] load happens, but before that gets ran, there we run our own setup.
         * 
         * IOnIncomingDamageserverReceiver[] array = onIncomingDamageReceivers;
         * <----- ILHook goes here
         * for(int i = 0; i < array.Length; i++)
         */
        private static void SetupOnIncomingDamageOtherServerReciever(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.AfterLabel,
                x => x.MatchLdarg(out _),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.onIncomingDamageReceivers))
                ))
            {
                MSULog.Fatal($"Failed to set up {nameof(IOnIncomingDamageOtherServerReciever)}!");
                return;
            }


            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<DamageInfo, HealthComponent>>(RunOnIncomingDamageOther);

#if DEBUG
            MSULog.Debug($"{nameof(IOnIncomingDamageOtherServerReciever)} succesfully set up.");
#endif
        }

        private static void RunOnIncomingDamageOther(DamageInfo damageInfo, HealthComponent victimHealthComponent)
        {
            if ((bool)damageInfo.attacker)
            {
                List<IOnIncomingDamageOtherServerReciever> incomingDamageOtherComponents = GetComponentsCache<IOnIncomingDamageOtherServerReciever>.GetGameObjectComponents(damageInfo.attacker);
                foreach (IOnIncomingDamageOtherServerReciever item in incomingDamageOtherComponents)
                {
                    item.OnIncomingDamageOther(victimHealthComponent, damageInfo);
                }
                GetComponentsCache<IOnIncomingDamageOtherServerReciever>.ReturnBuffer(incomingDamageOtherComponents);
            }
        }
        #endregion;
    }
}
