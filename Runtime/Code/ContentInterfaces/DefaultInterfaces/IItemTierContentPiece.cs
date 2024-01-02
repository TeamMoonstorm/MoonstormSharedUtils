using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    public interface IItemTierContentPiece : IContentPiece<ItemTierDef>
    {
        NullableRef<SerializableColorCatalogEntry> ColorIndex { get; }
        NullableRef<SerializableColorCatalogEntry> DarkColorIndex { get; }
        GameObject PickupDisplayVFX { get; }

        List<ItemIndex> ItemsWithThisTier { get; set; }
        List<PickupIndex> AvailableTierDropList { get; set; }
    }
}
