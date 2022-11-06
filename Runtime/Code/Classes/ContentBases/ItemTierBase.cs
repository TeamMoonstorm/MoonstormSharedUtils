using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm.Experimental
{
    public abstract class ItemTierBase : ContentBase
    {
        public abstract ItemTierDef ItemTierDef { get; }

        public abstract GameObject PickupDisplayVFX { get; }

        public List<ItemIndex> ItemsWithThisTier { get; internal set; } = new List<ItemIndex>();

        public List<PickupIndex> AvailableTierDropList { get; internal set; } = new List<PickupIndex>();
    }
}