using RoR2;
using UnityEngine;
using System.Collections.Generic;
namespace ExampleMod
{
    [CreateAssetMenu(fileName = "EquipmentAssetCollection", menuName = "ExampleMod/AssetCollections/EquipmentAssetCollection")]
    public class EquipmentAssetCollection : ExtendedAssetCollection
    {
        public List<GameObject> itemDisplayPrefabs;
        public EquipmentDef equipmentDef;
    }
}