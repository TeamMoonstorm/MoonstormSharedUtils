using System.Collections.Generic;
using UnityEngine;
using System;
using static Moonstorm.KeyAssetDisplayPairHolder;
using Object = UnityEngine.Object;

namespace Moonstorm.Utilities
{
    /// <summary>
    /// Utilities for handling item display rulesets from mods that do not use Thunderkit or MSU.
    /// </summary>
    [Obsolete("The MSIDRS util class is no longer supported due to changes in the standard of item displays implementation on modded characters.")]
    public static class MSIDRSUtil
    {
        /// <summary>
        /// Creates a Key Asset Display Pair struct.
        /// </summary>
        /// <param name="keyAsset">The key asset, must be either an EquipmentDef or an ItemDef</param>
        /// <param name="displayPrefabs">A list of display prefabs</param>
        /// <returns>The KeyAssetDisplayPair populated with the given arguments.</returns>
        public static KeyAssetDisplayPair CreateKeyAssetDisplayPair(Object keyAsset, List<GameObject> displayPrefabs)
        {
            var toReturn = new KeyAssetDisplayPair();
            toReturn.keyAsset = keyAsset;
            toReturn.displayPrefabs = displayPrefabs;
            return toReturn;
        }

        /// <summary>
        /// Creates a Key Asset Display Pair struct
        /// </summary>
        /// <param name="keyAsset">The key asset, must be either an EquipmentDef or an ItemDef</param>
        /// <param name="displayPrefab">a display prefab.</param>
        /// <returns>The KeyAssetDisplayPair populated with the given arguments.</returns>
        public static KeyAssetDisplayPair CreateKeyAssetDisplayPair(Object keyAsset, GameObject displayPrefab)
        {
            var toReturn = new KeyAssetDisplayPair();
            toReturn.keyAsset = keyAsset;
            toReturn.displayPrefabs = new List<GameObject> { displayPrefab };
            return toReturn;
        }

        /// <summary>
        /// Adds the KeyAssetDisplayPairs to the Moonstorm Item Display System.
        /// </summary>
        /// <param name="keyAssetDisplayPairs">A list of KeyAssetDisplayPairs.</param>
        public static void AddIDRSStuff(List<KeyAssetDisplayPair> keyAssetDisplayPairs)
        {
            keyAssetDisplayPairs.ForEach(kadp => kadp.AddKeyAssetDisplayPrefabsToIDRS());
        }
    }
}
