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
        public static Dictionary<string, ItemDisplayRuleSet> idrsDictionary = new Dictionary<string, ItemDisplayRuleSet>();
        public static Dictionary<string, GameObject> displayDictionary = new Dictionary<string, GameObject>();
#if DEBUG
        public static List<string> survivorRuleSets = new List<string>();
        public static List<string> enemyRuleSets = new List<string>();
        public static Dictionary<string, List<string>> itemToDisplayPrefabs = new Dictionary<string, List<string>>();
        public static Dictionary<string, List<string>> equipmentToDisplayPrefabs = new Dictionary<string, List<string>>();
        public static Dictionary<string, List<string>> eliteEquipmentToDisplayPrefabs = new Dictionary<string, List<string>>();
#endif
        public static ResourceAvailability catalogAvailability;

        public static GameObject GetItemDisplay(string key)
        {
            if (!displayDictionary.ContainsKey(key))
            {
#if DEBUG
                MSULog.Warning($"The following key was not present in the displayDictionary: {key}");
#endif
                return null;
            }
            return displayDictionary[key];
        }
        public static ItemDisplayRuleSet GetItemDisplayRuleSet(string key)
        {
            if (!idrsDictionary.ContainsKey(key))
            {
#if DEBUG
                MSULog.Warning($"The following key was not present in the displayDictionary: {key}");
#endif
                return null;
            }
            return idrsDictionary[key];
        }

        public static void AddDisplay(ItemDisplayDictionary idd)
        {
            if (!idd)
                return;

            if (!idd.keyAsset || !idd.displayPrefab)
                return;

            if(displayDictionary.ContainsKey(idd.keyAsset.name))
            {
#if DEBUG
                MSULog.Warning($"The following idd was not added to the ItemDisplayCatalog, since it's key ({idd.keyAsset})'s name is already pressent: {idd}");
                return;
#endif
            }
            displayDictionary.Add(idd.keyAsset.name, idd.displayPrefab);
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
                if (!idrsDictionary.ContainsKey(key))
                    idrsDictionary.Add(key, idrs);

#if DEBUG
                var def = SurvivorCatalog.FindSurvivorDefFromBody(body);
                if (def)
                {
                    survivorRuleSets.AddIfNotInCollection(key);
                }
                else
                {
                    enemyRuleSets.AddIfNotInCollection(key);
                }
#endif
            }
        }

        private static void CreateDisplayDictionary()
        {
            PopulateFrom("Commando");
            PopulateFrom("Croco");
            PopulateFrom("Mage");
            PopulateFrom("LunarExploder");

            void PopulateFrom(string address)
            {
                var idrs = Addressables.LoadAssetAsync<ItemDisplayRuleSet>($"RoR2/Base/{address}/idrs{address}.asset").WaitForCompletion();

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

                        if (!displayDictionary.ContainsKey(key))
                        {
                            displayDictionary.Add(key, displayPrefab);
                        }
                        else
                        {
                            var existingPrefab = displayDictionary[key];
                            if (existingPrefab == displayPrefab)
                                continue;

                            int startingIndex = i - 1;
                            while (displayDictionary.ContainsKey(key))
                            {
                                startingIndex++;
                                key = $"{ruleGroup.keyAsset.name}Display_{startingIndex}";
                            }
                            displayDictionary.Add(key, displayPrefab);
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