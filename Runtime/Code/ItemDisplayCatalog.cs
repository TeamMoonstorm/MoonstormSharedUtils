using BepInEx;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSU
{
    internal static class ItemDisplayCatalog
    {
        private static StringComparer _comparer = StringComparer.OrdinalIgnoreCase;
        private static Dictionary<string, ItemDisplayRuleSet> _idrsDictionary = new Dictionary<string, ItemDisplayRuleSet>(_comparer);
        private static Dictionary<string, GameObject> _displayDictionary = new Dictionary<string, GameObject>(_comparer);

        public static ResourceAvailability catalogAvailability;
        public static GameObject GetItemDisplay(string key)
        {
            if (!_displayDictionary.ContainsKey(key))
            {
#if DEBUG
                MSULog.Warning($"The following key was not present in the displayDictionary: {key}");
#endif
                return null;
            }
            return _displayDictionary[key];
        }

        public static ItemDisplayRuleSet GetItemDisplayRuleSet(string key)
        {
            if (!_idrsDictionary.ContainsKey(key))
            {
#if DEBUG
                MSULog.Warning($"The following key was not present in the idrsDictionary: {key}");
#endif
                return null;
            }
            return _idrsDictionary[key];
        }

        [SystemInitializer]
        private static IEnumerator SystemInitializer()
        {
            yield return null;

            RoR2Application.onLoad += () =>
            {
                CreateIDRSDictionary();
                CreateDisplayDictionary();
                catalogAvailability.MakeAvailable();
#if DEBUG
                SerializeCatalog();
                _itemToDisplayPrefabs.Clear();
                _equipmentToDisplayPrefabs.Clear();
                _eliteEquipmentToDisplayPrefabs.Clear();
                _survivorRuleSets.Clear();
                _enemyRuleSets.Clear();
#endif
                _idrsDictionary.Clear();
                _displayDictionary.Clear();
            };
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
                if (!_idrsDictionary.ContainsKey(key))
                    _idrsDictionary.Add(key, idrs);

#if DEBUG
                var def = SurvivorCatalog.FindSurvivorDefFromBody(body);
                if (def)
                {
                    if (!_enemyRuleSets.Contains(key))
                        _survivorRuleSets.Add(key);
                }
                else
                {
                    if (!_survivorRuleSets.Contains(key))
                        _enemyRuleSets.Add(key);
                }
#endif
            }
        }

        private static void CreateDisplayDictionary()
        {
            foreach (ItemDisplayRuleSet idrs in _idrsDictionary.Values)
            {
                PopulateDisplaysFromIDRS(idrs);
            }

            foreach (var (_, item) in ItemModule.moonstormItems)
            {
                PopulateDisplaysFromItems(item);
            }

            foreach (var (_, equipment) in EquipmentModule.allMoonstormEquipments)
            {
                PopulateDisplaysFromEquips(equipment);
            }
        }

        private static void PopulateDisplaysFromIDRS(ItemDisplayRuleSet idrs)
        {
            foreach (var ruleGroup in idrs.keyAssetRuleGroups)
            {
                if (ruleGroup.displayRuleGroup.isEmpty)
                    continue;

                var rulesArray = ruleGroup.displayRuleGroup.rules;
                for (int i = 0; i < rulesArray.Length; i++)
                {
                    var rule = rulesArray[i];
                    var displayPrefab = rule.followerPrefab;
                    if (!displayPrefab)
                        continue;

                    string key = displayPrefab.name.IsNullOrWhiteSpace() ? $"{ruleGroup.keyAsset.name}Display_{i}" : displayPrefab.name;

                    if (!_displayDictionary.ContainsKey(key))
                    {
                        _displayDictionary.Add(key, displayPrefab);
                    }
                    else
                    {
                        var existingPrefab = _displayDictionary[key];
                        if (existingPrefab == displayPrefab)
                            continue;

                        int startingIndex = i - 1;
                        while (_displayDictionary.ContainsKey(key))
                        {
                            startingIndex++;
                            key = $"{ruleGroup.keyAsset.name}Display_{startingIndex}";
                        }
                        _displayDictionary.Add(key, displayPrefab);
                    }
                }

#if DEBUG
                AddKeyAssetAndDisplaysForSerializedDictionary(ruleGroup);
#endif

            }
        }

        private static void PopulateDisplaysFromItems(IItemContentPiece item)
        {
            if (!item.itemDisplayPrefabs)
            {
                return;
            }

            List<GameObject> displayPrefabs = item.itemDisplayPrefabs;
            ItemDef itemDef = item.asset;

            for (int i = 0; i < displayPrefabs.Count; i++)
            {
                var displayPrefab = displayPrefabs[i];

                if (!displayPrefab)
                    continue;

                string key = displayPrefab.name.IsNullOrWhiteSpace() ? $"{itemDef.name}Display_{i}" : displayPrefab.name;

                if (!_displayDictionary.ContainsKey(key))
                {
                    _displayDictionary.Add(key, displayPrefab);
                }
                else
                {
                    var existingPrefab = _displayDictionary[key];
                    if (existingPrefab == displayPrefab)
                        continue;

                    int startingIndex = i - 1;
                    while (_displayDictionary.ContainsKey(key))
                    {
                        startingIndex++;
                        key = $"{itemDef.name}Display_{startingIndex}";
                    }
                    _displayDictionary.Add(key, displayPrefab);
                }
            }

#if DEBUG
            AddKeyAssetAndDisplaysForSerializedDictionary(itemDef, displayPrefabs);
#endif
        }

        private static void PopulateDisplaysFromEquips(IEquipmentContentPiece equipment)
        {
            if (!equipment.itemDisplayPrefabs)
            {
                return;
            }

            List<GameObject> displayPrefabs = equipment.itemDisplayPrefabs;
            EquipmentDef itemDef = equipment.asset;

            for (int i = 0; i < displayPrefabs.Count; i++)
            {
                var displayPrefab = displayPrefabs[i];

                if (!displayPrefab)
                    continue;

                string key = displayPrefab.name.IsNullOrWhiteSpace() ? $"{itemDef.name}Display_{i}" : displayPrefab.name;

                if (!_displayDictionary.ContainsKey(key))
                {
                    _displayDictionary.Add(key, displayPrefab);
                }
                else
                {
                    var existingPrefab = _displayDictionary[key];
                    if (existingPrefab == displayPrefab)
                        continue;

                    int startingIndex = i - 1;
                    while (_displayDictionary.ContainsKey(key))
                    {
                        startingIndex++;
                        key = $"{itemDef.name}Display_{startingIndex}";
                    }
                    _displayDictionary.Add(key, displayPrefab);
                }
            }

#if DEBUG
            AddKeyAssetAndDisplaysForSerializedDictionary(itemDef, displayPrefabs);
#endif
        }

#if DEBUG
        private static HashSet<string> _survivorRuleSets = new HashSet<string>(_comparer);
        private static HashSet<string> _enemyRuleSets = new HashSet<string>(_comparer);
        private static Dictionary<string, HashSet<string>> _itemToDisplayPrefabs = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, HashSet<string>> _equipmentToDisplayPrefabs = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, HashSet<string>> _eliteEquipmentToDisplayPrefabs = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        private static void AddKeyAssetAndDisplaysForSerializedDictionary(ItemDisplayRuleSet.KeyAssetRuleGroup ruleGroup)
        {
            if (ruleGroup.displayRuleGroup.isEmpty)
                return;

            if (!(ruleGroup.keyAsset is ScriptableObject keyAsset))
                return;

            Dictionary<string, HashSet<string>> target = null;
            switch (keyAsset)
            {
                case ItemDef id:
                    target = _itemToDisplayPrefabs;
                    break;
                case EquipmentDef ed:
                    target = (ed && ed.passiveBuffDef && ed.passiveBuffDef.eliteDef) ? _eliteEquipmentToDisplayPrefabs : _equipmentToDisplayPrefabs;
                    break;
                default:
                    return;
            }

            string keyName = keyAsset.name;
            if (!target.ContainsKey(keyName))
            {
                target[keyName] = new HashSet<string>(_comparer);
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

                target[keyName].Add(value);
            }
        }

        private static void AddKeyAssetAndDisplaysForSerializedDictionary(ScriptableObject keyAsset, List<GameObject> displayPrefabs)
        {
            Dictionary<string, HashSet<string>> target = null;

            switch (keyAsset)
            {
                case ItemDef id:
                    target = _itemToDisplayPrefabs;
                    break;
                case EquipmentDef ed:
                    target = (ed && ed.passiveBuffDef && ed.passiveBuffDef.eliteDef) ? _eliteEquipmentToDisplayPrefabs : _equipmentToDisplayPrefabs;
                    break;
                default:
                    return;
            }

            string keyName = keyAsset.name;
            if (!target.ContainsKey(keyName))
            {
                target[keyName] = new HashSet<string>(_comparer);
            }

            for (int i = 0; i < displayPrefabs.Count; i++)
            {
                var displayPrefab = displayPrefabs[i];
                if (!displayPrefab)
                    continue;

                string displayPrefabName = displayPrefab.name;
                string value = displayPrefabName.IsNullOrWhiteSpace() ? $"{keyName}Display_{i}" : displayPrefabName;

                target[keyName].Add(value);
            }
        }

        [Serializable]
        private class SerializedKeyAssetDisplayPrefabs
        {
            public string keyAsset;
            public string[] displays;
        }

        [Serializable]
        private class SerializedIDRSCatalog
        {
            [SerializeField] public string[] survivorItemDisplayRuleSets = Array.Empty<string>();
            [SerializeField] public string[] enemyItemDisplayRuleSets = Array.Empty<string>();
            [SerializeField] public SerializedKeyAssetDisplayPrefabs[] items = Array.Empty<SerializedKeyAssetDisplayPrefabs>();
            [SerializeField] public SerializedKeyAssetDisplayPrefabs[] equipments = Array.Empty<SerializedKeyAssetDisplayPrefabs>();
            [SerializeField] public SerializedKeyAssetDisplayPrefabs[] eliteEquipments = Array.Empty<SerializedKeyAssetDisplayPrefabs>();
        }

        private static void SerializeCatalog()
        {
            var cat = new SerializedIDRSCatalog();
            cat.survivorItemDisplayRuleSets = _survivorRuleSets.ToArray();
            cat.enemyItemDisplayRuleSets = _enemyRuleSets.ToArray();
            cat.items = _itemToDisplayPrefabs.Select(kvp => new SerializedKeyAssetDisplayPrefabs
            {
                keyAsset = kvp.Key,
                displays = kvp.Value.ToArray()
            }).ToArray();
            cat.equipments = _equipmentToDisplayPrefabs.Select(kvp => new SerializedKeyAssetDisplayPrefabs
            {
                keyAsset = kvp.Key,
                displays = kvp.Value.ToArray()
            }).ToArray();
            cat.eliteEquipments = _eliteEquipmentToDisplayPrefabs.Select(kvp => new SerializedKeyAssetDisplayPrefabs
            {
                keyAsset = kvp.Key,
                displays = kvp.Value.ToArray()
            }).ToArray();

            string json = JsonUtility.ToJson(cat, true);

            var directory = System.IO.Path.GetDirectoryName(MSUMain.pluginInfo.Location);
            var fileName = "IDRSCatalog.json";
            var filePath = System.IO.Path.Combine(directory, fileName);

            using (var writer = System.IO.File.CreateText(filePath))
            {
                writer.Write(json);
                MSULog.Message("ItemDisplayCatalog serialized.");
            }
        }
#endif
    }
}
