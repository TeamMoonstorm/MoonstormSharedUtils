using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExampleMod
{
    [CreateAssetMenu(fileName = "ItemTierAssetCollection", menuName = "ExampleMod/AssetCollections/ItemTierAssetCollection")]
    public class ItemTierAssetCollection : ExtendedAssetCollection
    {
        public SerializableColorCatalogEntry colorIndex;
        public SerializableColorCatalogEntry darkColorIndex;
        public GameObject pickupDisplayVFX;
        public ItemTierDef itemTierDef;
    }
}
