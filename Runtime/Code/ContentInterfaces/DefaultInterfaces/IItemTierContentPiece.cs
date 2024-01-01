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
        public NullableRef<SerializableColorCatalogEntry> ColorIndex { get; }
        public NullableRef<SerializableColorCatalogEntry> DarkColorIndex { get; }
        public GameObject PickupDisplayVFX { get; }

        public List<ItemIndex> ItemsWithThisTier { get; set; }
        public List<PickupIndex> AvailableTierDropList { get; set; }
    }
}
