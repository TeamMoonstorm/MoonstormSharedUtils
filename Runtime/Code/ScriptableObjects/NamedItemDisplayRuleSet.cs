using BepInEx;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static RoR2.ItemDisplayRuleSet;

namespace MSU
{
    [CreateAssetMenu(fileName = "New NamedItemDisplayRuleSet", menuName = "MSU/IDRS/NamedItemDisplayRuleSet")]
    public class NamedItemDisplayRuleSet : ScriptableObject
    {
        private static readonly HashSet<NamedItemDisplayRuleSet> _instances = new HashSet<NamedItemDisplayRuleSet>();
        public ItemDisplayRuleSet targetItemDisplayRuleSet;

        [Space]
        public List<RuleGroup> rules = new List<RuleGroup>();

        private void Awake() => _instances.Add(this);
        private void OnDestroy() => _instances.Remove(this);

        [SystemInitializer]
        private static void SystemInitializer()
        {
            ItemDisplayCatalog.catalogAvailability.CallWhenAvailable(() =>
            {
                MSULog.Info("Initializing NamedItemDisplayRuleSets");
                foreach (var nidrs in _instances)
                {
                    try
                    {
                        foreach (ItemDisplayRuleSet.KeyAssetRuleGroup keyAssetRuleGroup in nidrs.GetKeyAssetRuleGroups())
                        {
                            HG.ArrayUtils.ArrayAppend(ref nidrs.targetItemDisplayRuleSet.keyAssetRuleGroups, keyAssetRuleGroup);
                        }
                        nidrs.targetItemDisplayRuleSet.GenerateRuntimeValues();

#if DEBUG
                        MSULog.Debug($"Finished appending values from {nidrs} to {nidrs.targetItemDisplayRuleSet}");
#endif
                    }
                    catch (Exception ex)
                    {
                        MSULog.Error($"Exception during initialization of NamedItemDisplayRuleSet called {nidrs.name}.\n{ex}");
                    }
                }
            });
        }

        private ItemDisplayRuleSet.KeyAssetRuleGroup[] GetKeyAssetRuleGroups()
        {
            var keyAssetList = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();
            foreach(var ruleGroup in rules)
            {
                if (ruleGroup.IsEmpty)
                    continue;

                var keyAssetName = ruleGroup.keyAssetName;
                UnityEngine.Object keyAsset = null;

                var eqpIndex = EquipmentCatalog.FindEquipmentIndex(keyAssetName);
                if (eqpIndex != EquipmentIndex.None && !keyAsset)
                {
                    keyAsset = EquipmentCatalog.GetEquipmentDef(eqpIndex);
                }

                var itemIndex = ItemCatalog.FindItemIndex(keyAssetName);
                if(itemIndex != ItemIndex.None && !keyAsset)
                {
                    keyAsset = ItemCatalog.GetItemDef(itemIndex);
                }

                if(!keyAsset)
                {
#if DEBUG
                    MSULog.Warning($"Could not get key asset of name {keyAssetName} (Index: {rules.IndexOf(ruleGroup)}). {this}");
#endif
                    continue;
                }

                var keyAssetGroup = new ItemDisplayRuleSet.KeyAssetRuleGroup { keyAsset = keyAsset };
                for(int i = 0; i < ruleGroup.rules.Count; i++)
                {
                    DisplayRule rule = ruleGroup.rules[i];
                    keyAssetGroup.displayRuleGroup.AddDisplayRule(rule.FinishedRule);
                }
                keyAssetList.Add(keyAssetGroup);
            }
            rules.Clear();
            return keyAssetList.ToArray();
        }

        [Serializable]
        public struct RuleGroup
        {
            public string keyAssetName;
            public List<DisplayRule> rules;

            public bool IsEmpty => rules != null ? rules.Count == 0 : true;

            public void AddRule(DisplayRule rule)
            {
                rules = rules ?? new List<DisplayRule>();

                rules.Add(rule);
            }
        }

        [Serializable]
        public struct DisplayRule
        {
            public ItemDisplayRuleType ruleType;
            public string displayPrefab;
            public string childName;
            public Vector3 localPos;
            public Vector3 localAngles;
            public Vector3 localScale;
            public LimbFlags limbMask;

            public const string NO_VALUE = "NoValue";

            public ItemDisplayRule FinishedRule
            {
                get
                {
                    if(!_finishedRule.HasValue)
                    {
                        GameObject prefab = ItemDisplayCatalog.GetItemDisplay(displayPrefab);
                        if(childName.IsNullOrWhiteSpace())
                        {
                            _finishedRule = new ItemDisplayRule
                            {
                                childName = NO_VALUE,
                                localAngles = Vector3.zero,
                                localPos = Vector3.zero,
                                localScale = Vector3.one,
                                followerPrefab = prefab,
                                limbMask = limbMask,
                                ruleType = ruleType
                            };
                        }
                        else
                        {
                            _finishedRule = new ItemDisplayRule
                            {
                                childName = childName,
                                localAngles = localAngles,
                                localPos = localPos,
                                localScale = localScale,
                                followerPrefab = prefab,
                                limbMask = limbMask,
                                ruleType = ruleType
                            };
                        }
                    }

                    return _finishedRule.Value;
                }
            }
            private ItemDisplayRule? _finishedRule;
        }
    }
}