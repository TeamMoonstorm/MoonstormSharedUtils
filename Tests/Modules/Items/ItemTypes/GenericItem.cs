using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Moonstorm.Items
{
    public class GenericItem : ItemBase
    {
        public override ItemDef ItemDef => MSUTAssets.LoadAsset<ItemDef>("GenericItem");
        public override GameObject ItemDisplayPrefab => MSUTAssets.LoadAsset<GameObject>("GenericItemDisplay");

        [ConfigurableField(MSUTConfig.items)]
        [TokenModifier("MSU_ITEM_GENERICITEM_DESC", StatTypes.Percentage, 0)]
        public static float statMultiplier = 0.1f;
        public override void Initialize()
        {
            base.Initialize();
            R2API.RecalculateStatsAPI.GetStatCoefficients += AllStatsUp;
        }

        private void AllStatsUp(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(sender.GetItemCount(ItemDef) > 0)
            {
                var amount = statMultiplier + (statMultiplier * (sender.GetItemCount(ItemDef) - 1));
                MSUTLog.Info($"Adding {amount} to {sender}'s level mult add");
                args.levelMultAdd += amount;
            }
        }
    }
}
