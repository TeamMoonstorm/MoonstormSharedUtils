using RoR2;
using UnityEngine;
using System.Collections.Generic;
namespace ExampleMod
{
    [CreateAssetMenu(fileName = "ItemAssetCollection", menuName = "ExampleMod/AssetCollections/ItemAssetCollection")]
    public class ItemAssetCollection : ExtendedAssetCollection
    {
        public List<GameObject> itemDisplayPrefabs;
        public ItemDef itemDef;
    }
}