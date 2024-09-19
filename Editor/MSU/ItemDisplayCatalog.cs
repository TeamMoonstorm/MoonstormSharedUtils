using RoR2.Editor;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MSU.Editor
{
    public class ItemDisplayCatalog
    {
        public static bool catalogExists => jsonAsset;
        public static TextAsset jsonAsset => AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/IDRSCatalog.json");

        public ReadOnlyCollection<string> survivorItemDisplayRuleSets { get; private set; }
        public ReadOnlyCollection<string> enemyItemDisplayRuleSets { get; private set; }
        public ReadOnlyCollection<string> allKeyAssets { get; private set; }
        public ReadOnlyDictionary<string, ReadOnlyCollection<string>> itemToDisplayPrefabs { get; private set; }
        public ReadOnlyDictionary<string, ReadOnlyCollection<string>> equipmentToDisplayPrefabs { get; private set; }
        public ReadOnlyDictionary<string, ReadOnlyCollection<string>> eliteEquipmentToDisplayPrefabs { get; private set; }
        public bool DoesIDRSExist(string idrsKey)
        {
            if (survivorItemDisplayRuleSets.Contains(idrsKey))
                return true;
            else
                return enemyItemDisplayRuleSets.Contains(idrsKey);
        }
        public ReadOnlyCollection<string> GetKeyAssetDisplays(string key)
        {
            if (key.IsNullOrEmptyOrWhiteSpace())
                return null;

            if (itemToDisplayPrefabs.ContainsKey(key))
            {
                return itemToDisplayPrefabs[key];
            }
            if (equipmentToDisplayPrefabs.ContainsKey(key))
            {
                return equipmentToDisplayPrefabs[key];
            }
            if (eliteEquipmentToDisplayPrefabs.ContainsKey(key))
            {
                return eliteEquipmentToDisplayPrefabs[key];
            }
            return null;
        }
        public static ItemDisplayCatalog LoadCatalog()
        {
            if (!catalogExists)
            {
                EditorUtility.DisplayDialog("No ItemDisplayCatalog Found", "The ItemDisplayCatalog could not be found in the Project.\nThe ItemDisplayCatalog at runtime creates a serialized .json version that's used in the Editor context.\nTo Create the serialized version of the ItemDisplayCatalog, build MSU on Debug mode and run your profile, the json file will be created on the same folder where MSU's dll is located.\nAfter creating it, place the file inside your Assets folder.", "Ok");
                return null;
            }

            return CreateCatalog();
        }

        private static ItemDisplayCatalog CreateCatalog()
        {
            var intermediary = JsonUtility.FromJson<JSONIntermediary>(jsonAsset.text);
            var catalog = new ItemDisplayCatalog();

            catalog.survivorItemDisplayRuleSets = new ReadOnlyCollection<string>(intermediary.survivorItemDisplayRuleSets);
            catalog.enemyItemDisplayRuleSets = new ReadOnlyCollection<string>(intermediary.enemyItemDisplayRuleSets);
            catalog.itemToDisplayPrefabs = new ReadOnlyDictionary<string, ReadOnlyCollection<string>>(intermediary.items.ToDictionary(key => key.keyAsset, value => new ReadOnlyCollection<string>(value.displays)));
            catalog.equipmentToDisplayPrefabs = new ReadOnlyDictionary<string, ReadOnlyCollection<string>>(intermediary.equipments.ToDictionary(key => key.keyAsset, value => new ReadOnlyCollection<string>(value.displays)));
            catalog.eliteEquipmentToDisplayPrefabs = new ReadOnlyDictionary<string, ReadOnlyCollection<string>>(intermediary.eliteEquipments.ToDictionary(key => key.keyAsset, value => new ReadOnlyCollection<string>(value.displays)));
            catalog.allKeyAssets = new ReadOnlyCollection<string>(intermediary.items.Select(k => k.keyAsset).Concat(intermediary.equipments.Select(k => k.keyAsset).Concat(intermediary.eliteEquipments.Select(k => k.keyAsset))).ToList());

            return catalog;
        }
        private ItemDisplayCatalog() { }

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
            public KeyAssetDisplayPrefabs[] equipments = Array.Empty<KeyAssetDisplayPrefabs>();
            public KeyAssetDisplayPrefabs[] eliteEquipments = Array.Empty<KeyAssetDisplayPrefabs>();
        }
    }

    public interface IItemDisplayCatalogReceiver
    {
        ItemDisplayCatalog catalog { get; set; }
    }
}