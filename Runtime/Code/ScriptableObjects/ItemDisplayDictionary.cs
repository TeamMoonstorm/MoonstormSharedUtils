using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    [CreateAssetMenu(fileName = "New ItemDisplayDictionary", menuName = "MSU/IDRS/ItemDisplayDictionary")]
    public class ItemDisplayDictionary : ScriptableObject
    {
        private static readonly HashSet<ItemDisplayDictionary> _instances = new HashSet<ItemDisplayDictionary>();
        public ScriptableObject keyAsset;
        public GameObject[] displayPrefabs = Array.Empty<GameObject>();

        [Space]
        public List<DisplayDictionaryEntry> displayDictionaryEntries = new List<DisplayDictionaryEntry>();

        private void Awake() => _instances.Add(this);
        private void OnDestroy() => _instances.Add(this);

        [SystemInitializer]
        private static void SystemInitializer()
        {
            ItemDisplayCatalog.catalogAvailability.CallWhenAvailable(() =>
            {
                MSULog.Info("Initializing ItemDdisplayDictionaries");
                HashSet<ItemDisplayRuleSet> modifiedIDRS = new HashSet<ItemDisplayRuleSet>();
                foreach(ItemDisplayDictionary dictionary in _instances)
                {
                    var keyAsset = dictionary.keyAsset;
                    if(!(keyAsset is ItemDef id) || !(keyAsset is EquipmentDef ed))
                    {
                        MSULog.Warning($"Item display dictionary {dictionary} has an invalid key asset, a key asset must be either an ItemDef or EquipmentDef.");
                        continue;
                    }

                    if(id && id.itemIndex == ItemIndex.None)
                    {
#if DEBUG
                        MSULog.Debug($"Not appending valuees from {dictionary}, as its ItemDef's index is none.");
#endif
                        continue;
                    }
                    if (ed && ed.equipmentIndex == EquipmentIndex.None)
                    {
#if DEBUG
                        MSULog.Debug($"Not appending valuees from {dictionary}, as its ItemDef's index is none.");
#endif
                        continue;
                    }

                    for(int i = 0; i < dictionary.displayDictionaryEntries.Count; i++)
                    {
                        try
                        {
                            var entry = dictionary.displayDictionaryEntries[i];
                            ItemDisplayRuleSet target = ItemDisplayCatalog.GetItemDisplayRuleSet(entry.idrsName);
                            if(!target)
                            {
#if DEBUG
                                MSULog.Warning($"Not appending values of {dictionary}'s {i} index, as the target idrs is null.");
#endif
                                continue;
                            }
                            var keyAssetRuleGroup = dictionary.GetKeyAssetRuleGroup(entry.idrsName);
                            HG.ArrayUtils.ArrayAppend(ref target.keyAssetRuleGroups, keyAssetRuleGroup);
                            modifiedIDRS.Add(target);
#if DEBUG
                            MSULog.Debug($"Finished appending values from {dictionary}'s {i} entry into {target}");
#endif
                        }
                        catch (Exception e)
                        {
                            MSULog.Error($"{e}\n({dictionary} index {i}");
                        }
                    }
#if DEBUG
                    MSULog.Debug($"Finished appending values of {dictionary}");
#endif
                }

                foreach (var idrs in modifiedIDRS)
                {
                    idrs.GenerateRuntimeValues();
#if DEBUG
                    MSULog.Debug($"Regenerated runtime values for {idrs}");
#endif
                }
            });
        }

        private ItemDisplayRuleSet.KeyAssetRuleGroup GetKeyAssetRuleGroup(string key)
        {
            var keyAssetRuleGroup = new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = keyAsset,
                displayRuleGroup = new DisplayRuleGroup()
            };

            var index = displayDictionaryEntries.FindIndex(x => x.idrsName == key);
            if(index >= 0)
            {
                var entry = displayDictionaryEntries[index];
                if (entry.IsEmpty)
                    return keyAssetRuleGroup;

                for(int i = 0; i < entry.rules.Count; i++)
                {
                    DisplayRule rule = entry.rules[i];
                    rule.CreateRule(displayPrefabs);
                    var finishedRule = rule.FinishedRule;
                    keyAssetRuleGroup.displayRuleGroup.AddDisplayRule(finishedRule);
                }
            }
            return keyAssetRuleGroup;
        }

        [Serializable]
        public struct DisplayDictionaryEntry
        {
            public string idrsName;
            public List<DisplayRule> rules;

            public bool IsEmpty => rules == null ? rules.Count == 0 : true;

            public void AddDisplayRule(DisplayRule rule)
            {
                rules = rules ?? new List<DisplayRule>();

                rules.Add(rule);
            }
        }

        [Serializable]
        public struct DisplayRule
        {
            public ItemDisplayRuleType ruleType;
            public int displayPrefabIndex;
            public string childName;
            public Vector3 localPos;
            public Vector3 localAngles;
            public Vector3 localScale;
            public LimbFlags limbMask;

            public const string NO_VALUE = "NoValue";

            public ItemDisplayRule FinishedRule { get; private set; }

            internal void CreateRule(GameObject[] displayPrefabs)
            {
                if(string.IsNullOrWhiteSpace(childName))
                {
                    FinishedRule = new ItemDisplayRule
                    {
                        childName = NO_VALUE,
                        followerPrefab = displayPrefabs[displayPrefabIndex],
                        localAngles = localAngles,
                        localPos = localPos,
                        localScale = localScale,
                        ruleType = ruleType,
                        limbMask = limbMask
                    };
                    return;
                }

                FinishedRule = new ItemDisplayRule
                {
                    childName = childName,
                    followerPrefab = displayPrefabs[displayPrefabIndex],
                    localAngles = localAngles,
                    localPos = localPos,
                    localScale = localScale,
                    ruleType = ruleType,
                    limbMask = limbMask
                };
            }
        }
    }
}