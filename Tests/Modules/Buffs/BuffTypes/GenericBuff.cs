using Moonstorm.Components;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.Buffs
{
    public class GenericBuff : BuffBase
    {
        public override BuffDef BuffDef => MSUTAssets.LoadAsset<BuffDef>("bdGenericBuff");

        public class GenericBuffBehaviour : BaseBuffBodyBehavior, IOnTakeDamageServerReceiver, IBodyStatArgModifier
        {
            [BaseBuffBodyBehavior.BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => MSUTContent.Buffs.bdGenericBuff;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                var itemCount = body.GetItemCount(MSUTContent.Items.GenericVoidItem);
                if(itemCount > 0)
                {
                    var amount = Items.GenericVoidItem.statMultiplier + (Items.GenericVoidItem.statMultiplier * (itemCount - 1));
                    MSUTLog.Info($"Adding {amount} to {body}'s level mult add");
                    args.levelMultAdd += amount;
                }
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                var attacker = damageReport.attackerBody;
                if(attacker)
                {
                    MSUTLog.Info($"Inflicting cripple on {attacker} for {Items.GenericVoidItem.crippleDuration} seconds");
                    attacker.AddTimedBuff(RoR2Content.Buffs.Cripple, Items.GenericVoidItem.crippleDuration);
                }
            }
        }
    }
}
