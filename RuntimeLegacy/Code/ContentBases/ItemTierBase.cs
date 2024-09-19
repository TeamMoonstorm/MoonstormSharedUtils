using R2API.ScriptableObjects;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm
{
    public abstract class ItemTierBase : ContentBase
    {
        public virtual SerializableColorCatalogEntry ColorIndex { get; }

        public virtual SerializableColorCatalogEntry DarkColorIndex { get; }

        public abstract ItemTierDef ItemTierDef { get; }

        public abstract GameObject PickupDisplayVFX { get; }

        public List<ItemIndex> ItemsWithThisTier { get; internal set; } = new List<ItemIndex>();

        public List<PickupIndex> AvailableTierDropList { get; internal set; } = new List<PickupIndex>();
    }
}