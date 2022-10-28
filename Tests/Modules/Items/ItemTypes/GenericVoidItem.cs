using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Moonstorm.Items
{
    public class GenericVoidItem : VoidItemBase
    {
        public override ItemDef ItemDef => MSUTAssets.LoadAsset<ItemDef>("GenericVoidItem");
        public override GameObject ItemDisplayPrefab => MSUTAssets.LoadAsset<GameObject>("GenericVoidItemDisplay");

        [ConfigurableField(MSUTConfig.items)]
        [TokenModifier("MSU_ITEM_GENERICVOIDITEM_DESC", 0)]
        public static float statMultiplier = 0.05f;

        [ConfigurableField(MSUTConfig.items)]
        [TokenModifier("MSU_ITEM_GENERICVOIDITEM_DESC", StatTypes.Percentage, 1)]
        public static float crippleDuration = 3f;

        public override IEnumerable<ItemDef> LoadItemsToInfect()
        {
            return new ItemDef[1] { MSUTAssets.LoadAsset<ItemDef>("GenericItem") };
        }

        public class GenericVoidItemBehaviour : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            private static ItemDef GetItemDef() => MSUTContent.Items.GenericVoidItem;

            private void FixedUpdate()
            {
                if (!body.healthComponent.alive)
                    return;

                if(!body.healthComponent.isHealthLow && body.HasBuff(MSUTContent.Buffs.bdGenericBuff))
                {
                    body.RemoveBuff(MSUTContent.Buffs.bdGenericBuff);
                    return;
                }

                if(body.healthComponent.isHealthLow && !body.HasBuff(MSUTContent.Buffs.bdGenericBuff))
                    body.AddBuff(MSUTContent.Buffs.bdGenericBuff);
            }
        }
    }
}
