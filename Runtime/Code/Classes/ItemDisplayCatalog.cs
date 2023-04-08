using BepInEx;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if DEBUG
using Path = System.IO.Path;
#endif

namespace Moonstorm
{
    internal static class ItemDisplayCatalog
    {
        private static Dictionary<string, ItemDisplayRuleSet> idrsDictionary = new Dictionary<string, ItemDisplayRuleSet>();
        private static Dictionary<string, GameObject> displayDictionary = new Dictionary<string, GameObject>();
#if DEBUG
        private static List<string> survivorRuleSets = new List<string>();
        private static List<string> enemyRuleSets = new List<string>();
        private static Dictionary<string, List<string>> itemToDisplayPrefabs = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> equipmentToDisplayPrefabs = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> eliteEquipmentToDisplayPrefabs = new Dictionary<string, List<string>>();
#endif
        public static ResourceAvailability catalogAvailability;

        public static GameObject GetItemDisplay(string key)
        {
            var lowerInvariant = key.ToLowerInvariant();
            if (!displayDictionary.ContainsKey(lowerInvariant))
            {
#if DEBUG
                MSULog.Warning($"The following key was not present in the displayDictionary: {key}");
#endif
                return null;
            }
            return displayDictionary[lowerInvariant];
        }
        public static ItemDisplayRuleSet GetItemDisplayRuleSet(string key)
        {
            var lowerInvariant = key.ToLowerInvariant();
            if (!idrsDictionary.ContainsKey(lowerInvariant))
            {
#if DEBUG
                MSULog.Warning($"The following key was not present in the displayDictionary: {key}");
#endif
                return null;
            }
            return idrsDictionary[lowerInvariant];
        }

        public static void AddDisplay(ItemDisplayDictionary idd)
        {
            if (!idd)
                return;

            for(int i = 0; i < idd.displayPrefabs.Length; i++)
            {
                var go = idd.displayPrefabs[i];
                string key = go.name;
                string lowerInvariant = key.ToLowerInvariant();

                if(displayDictionary.ContainsKey(lowerInvariant))
                {
#if DEBUG
                    MSULog.Warning($"The following DisplayPrefab from an ItemDisplayDictionary was not added to the ItemDisplayCatalog, since it's key ({key}) was already present. (idd: {idd}, index: {i})");
#endif
                    continue;
                }
                displayDictionary.Add(lowerInvariant, go);
            }

            //I could put this in the original for loop, but i cant be arsed.
#if DEBUG
            if (!idd.keyAsset)
                return;

            string serializedkey = idd.keyAsset.name;
            if(!itemToDisplayPrefabs.ContainsKey(serializedkey))
            {
                itemToDisplayPrefabs[serializedkey] = new List<string>();
            }

            for(int i = 0; i < idd.displayPrefabs.Length; i++)
            {
                var go = idd.displayPrefabs[i];
                string goName = go.name;

                itemToDisplayPrefabs[serializedkey].AddIfNotInCollection(goName);
            }
#endif
        }
        [SystemInitializer]
        private static void SystemInitializer()
        {
            RoR2Application.onLoad += FillCatalog;
        }

        private static void FillCatalog()
        {
            CreateIDRSDictionary();
            CreateDisplayDictionary();
            catalogAvailability.MakeAvailable();
#if DEBUG
            SerializeCatalog();
            itemToDisplayPrefabs.Clear();
            equipmentToDisplayPrefabs.Clear();
            eliteEquipmentToDisplayPrefabs.Clear();
            survivorRuleSets.Clear();
            enemyRuleSets.Clear();
#endif
            idrsDictionary.Clear();
            displayDictionary.Clear();
        }

        private static void CreateIDRSDictionary()
        {
            foreach (var body in BodyCatalog.bodyPrefabs)
            {
                var characterModel = body.GetComponentInChildren<CharacterModel>();
                if (!characterModel)
                    continue;

                var idrs = characterModel.itemDisplayRuleSet;
                if (!idrs)
                    continue;

                string key = idrs.name.IsNullOrWhiteSpace() ? $"idrs{body.name}" : idrs.name;
                string lowerInvariant = key.ToLowerInvariant();
                if (!idrsDictionary.ContainsKey(lowerInvariant))
                    idrsDictionary.Add(lowerInvariant, idrs);

#if DEBUG
                var def = SurvivorCatalog.FindSurvivorDefFromBody(body);
                if (def)
                {
                    if(!enemyRuleSets.Contains(key))
                        survivorRuleSets.AddIfNotInCollection(key);
                }
                else
                {
                    if(!survivorRuleSets.Contains(key))
                        enemyRuleSets.AddIfNotInCollection(key);
                }
#endif
            }
        }

        private static void CreateDisplayDictionary()
        {
            foreach(ItemDisplayRuleSet item in idrsDictionary.Values)
            {
                PopulateFrom(item);
            }

            void PopulateFrom(ItemDisplayRuleSet idrs)
            {
                foreach (var ruleGroup in idrs.keyAssetRuleGroups)
                {
                    var rulesArray = ruleGroup.displayRuleGroup.rules;
                    for (int i = 0; i < rulesArray.Length; i++)
                    {
                        var rule = rulesArray[i];
                        var displayPrefab = rule.followerPrefab;
                        if (!displayPrefab)
                            continue;

                        string key = displayPrefab.name.IsNullOrWhiteSpace() ? $"{ruleGroup.keyAsset.name}Display_{i}" : displayPrefab.name;
                        string lowerInvariant = key.ToLowerInvariant();

                        if (!displayDictionary.ContainsKey(lowerInvariant))
                        {
                            displayDictionary.Add(lowerInvariant, displayPrefab);
                        }
                        else
                        {
                            var existingPrefab = displayDictionary[lowerInvariant];
                            if (existingPrefab == displayPrefab)
                                continue;

                            int startingIndex = i - 1;
                            while (displayDictionary.ContainsKey(lowerInvariant))
                            {
                                startingIndex++;
                                key = $"{ruleGroup.keyAsset.name}Display_{startingIndex}";
                                lowerInvariant = key.ToLowerInvariant();
                            }
                            displayDictionary.Add(lowerInvariant, displayPrefab);
                        }
                    }
#if DEBUG
                    AddKeyAssetAndDisplaysForDictionary(ruleGroup);
#endif
                }
            }
        }

#if DEBUG
        private static void AddKeyAssetAndDisplaysForDictionary(ItemDisplayRuleSet.KeyAssetRuleGroup ruleGroup)
        {
            if (!(ruleGroup.keyAsset is ScriptableObject))
                return;

            ScriptableObject keyAsset = (ScriptableObject)ruleGroup.keyAsset;
            Dictionary<string, List<string>> target = null;
            switch (keyAsset)
            {
                case ItemDef id:
                    target = itemToDisplayPrefabs;
                    break;
                case EquipmentDef ed:
                    target = (ed && ed.passiveBuffDef && ed.passiveBuffDef.eliteDef) ? eliteEquipmentToDisplayPrefabs : equipmentToDisplayPrefabs;
                    break;
            }
            string keyName = keyAsset.name;
            if (!target.ContainsKey(keyName))
            {
                target[keyName] = new List<string>();
            }

            var rulesArray = ruleGroup.displayRuleGroup.rules;
            for (int i = 0; i < rulesArray.Length; i++)
            {
                var rule = rulesArray[i];
                var displayPrefab = rule.followerPrefab;
                if (!displayPrefab)
                    continue;

                string displayPrefabName = displayPrefab.name;
                string value = displayPrefabName.IsNullOrWhiteSpace() ? $"{keyName}Display_{i}" : displayPrefabName;

                target[keyName].AddIfNotInCollection(value);
            }
        }
        [Serializable]
        private class KeyAssetDisplayPrefabs
        {
            public string keyAsset;
            public string[] displays;
        }
        [Serializable]
        private class SerializedCatalog
        {
            [SerializeField] public string[] survivorItemDisplayRuleSets = Array.Empty<string>();
            [SerializeField] public string[] enemyItemDisplayRuleSets = Array.Empty<string>();
            [SerializeField] public KeyAssetDisplayPrefabs[] items = Array.Empty<KeyAssetDisplayPrefabs>();
            [SerializeField] public KeyAssetDisplayPrefabs[] equips = Array.Empty<KeyAssetDisplayPrefabs>();
            [SerializeField] public KeyAssetDisplayPrefabs[] eliteEquips = Array.Empty<KeyAssetDisplayPrefabs>();
        }
        private static void SerializeCatalog()
        {
            var cat = new SerializedCatalog();
            cat.survivorItemDisplayRuleSets = survivorRuleSets.ToArray();
            cat.enemyItemDisplayRuleSets = enemyRuleSets.ToArray();
            cat.items = itemToDisplayPrefabs.Select(kvp => new KeyAssetDisplayPrefabs { keyAsset = kvp.Key, displays = kvp.Value.ToArray() }).ToArray();
            cat.equips = equipmentToDisplayPrefabs.Select(kvp => new KeyAssetDisplayPrefabs { keyAsset = kvp.Key, displays = kvp.Value.ToArray() }).ToArray();
            cat.eliteEquips = eliteEquipmentToDisplayPrefabs.Select(kvp => new KeyAssetDisplayPrefabs { keyAsset = kvp.Key, displays = kvp.Value.ToArray() }).ToArray();

            string json = JsonUtility.ToJson(cat);

            using (var writer = System.IO.File.CreateText(Path.Combine(Path.GetDirectoryName(MoonstormSharedUtils.PluginInfo.Location), "IDRSCatalog.json")))
            {
                writer.Write(json);
            }
        }
#endif
    }
}