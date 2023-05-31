using RoR2EditorKit;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils
{
    public class ItemDisplayCatalog
    {
        [Serializable]
        private class JSONIntermediary
        {
            [Serializable]
            public class KeyAssetDisplayPrefabs
            {
                public string keyAsset = string.Empty;
                public string[] displays = Array.Empty<string>();
            }

            public string[] survivorItemDisplayRuleSets = Array.Empty<string>();
            public string[] enemyItemDisplayRuleSets = Array.Empty<string>();
            public KeyAssetDisplayPrefabs[] items = Array.Empty<KeyAssetDisplayPrefabs>();
            public KeyAssetDisplayPrefabs[] equips = Array.Empty<KeyAssetDisplayPrefabs>();
            public KeyAssetDisplayPrefabs[] eliteEquips = Array.Empty<KeyAssetDisplayPrefabs>();
        }
        public static bool CatalogExists => JSONAsset;
        private static TextAsset JSONAsset => AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/ThunderKitSettings/IDRSCatalog.json");

        public ReadOnlyCollection<string> SurvivorItemDisplayRuleSets { get; private set; }
        public ReadOnlyCollection<string> EnemyItemDisplayRuleSets { get; private set; }
        public ReadOnlyDictionary<string, ReadOnlyCollection<string>> ItemToDisplayPrefabs { get; private set; }
        public ReadOnlyDictionary<string, ReadOnlyCollection<string>> EquipmentToDisplayPrefabs { get; private set; }
        public ReadOnlyDictionary<string, ReadOnlyCollection<string>> EliteEquipmentToDisplayPrefabs { get; private set; }
        public bool DoesIDRSExist(string idrsKey)
        {
            if (SurvivorItemDisplayRuleSets.Contains(idrsKey))
                return true;
            else
                return EnemyItemDisplayRuleSets.Contains(idrsKey);
        }
        public ReadOnlyCollection<string> GetKeyAssetDisplays(string key)
        {
            if (key.IsNullOrEmptyOrWhitespace())
                return null;

            if (ItemToDisplayPrefabs.ContainsKey(key))
            {
                return ItemToDisplayPrefabs[key];
            }
            if (EquipmentToDisplayPrefabs.ContainsKey(key))
            {
                return EquipmentToDisplayPrefabs[key];
            }
            if (EliteEquipmentToDisplayPrefabs.ContainsKey(key))
            {
                return EliteEquipmentToDisplayPrefabs[key];
            }
            return null;
        }
        public static ItemDisplayCatalog LoadCatalog()
        {
            if (!CatalogExists)
            {
                EditorUtility.DisplayDialog("No ItemDisplayCatalog Found", "The ItemDisplayCatalog could not be found in the Project.\nThe ItemDisplayCatalog at runtime creates a serialized .json version that's used in the Editor context.\nTo Create the serialized version of the ItemDisplayCatalog, build MSU on Debug mode and run your profile, the json file will be created on the same folder where MSU's dll is located.\nAfter creating it, place the file inside your ThunderKitSettings folder.", "Ok");
                return null;
            }

            return CreateCatalog();
        }

        private static ItemDisplayCatalog CreateCatalog()
        {
            var intermediary = JsonUtility.FromJson<JSONIntermediary>(JSONAsset.text);
            var catalog = new ItemDisplayCatalog();

            catalog.SurvivorItemDisplayRuleSets = new ReadOnlyCollection<string>(intermediary.survivorItemDisplayRuleSets);
            catalog.EnemyItemDisplayRuleSets = new ReadOnlyCollection<string>(intermediary.enemyItemDisplayRuleSets);
            catalog.ItemToDisplayPrefabs = new ReadOnlyDictionary<string, ReadOnlyCollection<string>>(intermediary.items.ToDictionary(key => key.keyAsset, value => new ReadOnlyCollection<string>(value.displays)));
            catalog.EquipmentToDisplayPrefabs = new ReadOnlyDictionary<string, ReadOnlyCollection<string>>(intermediary.equips.ToDictionary(key => key.keyAsset, value => new ReadOnlyCollection<string>(value.displays)));
            catalog.EliteEquipmentToDisplayPrefabs = new ReadOnlyDictionary<string, ReadOnlyCollection<string>>(intermediary.eliteEquips.ToDictionary(key => key.keyAsset, value => new ReadOnlyCollection<string>(value.displays)));

            return catalog;
        }
        private ItemDisplayCatalog() { }
    }
}