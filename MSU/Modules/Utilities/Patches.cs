using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Moonstorm.Utilities
{
    public static class Patches
    {
        public static void Init()
        {
            IL.RoR2.GlobalEventManager.OnCharacterDeath += IL_ServerKilledOtherPatch;
        }

        ///<summary>Patches the IOnKilledOtherServerReciever to not call twice, once in the HealthComponent and once in the GlobalEventManager. Ghor fixed this for us, it can be removed next ror2 update.</summary>
        private static void IL_ServerKilledOtherPatch(ILContext il)
        {
            var c = new ILCursor(il);

            //This gets the code on line 729 in dnspy. 
            ILLabel endIfLabel = null;
            bool flag = c.TryGotoNext(
                x => x.MatchLdloc(12),
                x => x.MatchCallOrCallvirt<UnityEngine.Object>("op_Implicit"),
                x => x.MatchBrfalse(out endIfLabel));
            if (flag)
            {
                c.Emit(OpCodes.Ldc_I4_0);
                c.Emit(OpCodes.Brfalse, endIfLabel);
            }
            else
                MSULog.LogE("Errors: IL Instruction Not found. Skipping.");

            // This changes that line of code from
            // if (attacker)
            // to be
            // if (false == true && attacker)
        }

    }
}
