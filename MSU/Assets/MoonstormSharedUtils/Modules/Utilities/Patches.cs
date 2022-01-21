using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Moonstorm.Utilities
{
    internal static class Patches
    {
        internal static void Init()
        {
            IL.RoR2.GlobalEventManager.OnCharacterDeath += IL_ServerKilledOtherPatch;
        }

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
                MSULog.Error("Errors: IL Instruction Not found. Skipping.");

            // This changes that line of code from
            // if (attacker)
            // to be
            // if (false == true && attacker)
        }

    }
}
