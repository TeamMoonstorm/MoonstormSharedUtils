using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New Key Asset Display Pair Holder", menuName = "Moonstorm/IDRS/Key Asset Display Pair Holder", order = 0)]
    public class KeyAssetDisplayPairHolder : ScriptableObject
    {
        [Serializable]
        public struct KeyAssetDisplayPair
        {
            public Object keyAsset;
            public List<GameObject> displayPrefabs;

            internal void AddKeyAssetDisplayPrefabsToIDRS()
            {
                /*var keyAssetName = keyAsset.name.ToLowerInvariant();

                if (keyAsset is ItemDef itemDef)
                    ItemDisplayModuleBase.itemKeyAssets.Add(keyAssetName, itemDef);
                else if (keyAsset is EquipmentDef eqpDef)
                    ItemDisplayModuleBase.equipKeyAssets.Add(keyAssetName, eqpDef);
                else
                    return;

                for (int i = 0; i < displayPrefabs.Count; i++)
                {
                    var constructedName = $"{keyAssetName}DisplayPrefab_{i}".ToLowerInvariant();
                    ItemDisplayModuleBase.moonstormItemDisplayPrefabs.Add(constructedName, displayPrefabs[i]);
                }*/
            }
        }

        public KeyAssetDisplayPair[] KeyAssetDisplayPairs;
    }
}