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
    /// <summary>
    /// A <see cref="NamedItemDisplayRuleSet"/> is a version of a <see cref="ItemDisplayRuleSet"/> which can be populated and serialized in the editor.
    /// <para>The values in <see cref="rules"/> will be appended to the IDRS set in <see cref="targetItemDisplayRuleSet"/></para>
    /// <para>The KeyAsset for the rule groups are loaded with their names</para>
    /// </summary>
    [CreateAssetMenu(fileName = "New NamedItemDisplayRuleSet", menuName = "MSU/IDRS/NamedItemDisplayRuleSet")]
    public class NamedItemDisplayRuleSet : ScriptableObject
    {
        private static readonly HashSet<NamedItemDisplayRuleSet> _instances = new HashSet<NamedItemDisplayRuleSet>();
        [Tooltip("The target ItemDisplayRuleSet to modify")]
        public ItemDisplayRuleSet targetItemDisplayRuleSet;

        [Tooltip("The new rules that'll be implemented in \"Target Item Display Rule Set\"")]
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
                if (ruleGroup.isEmpty)
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
                    keyAssetGroup.displayRuleGroup.AddDisplayRule(rule.finishedRule);
                }
                keyAssetList.Add(keyAssetGroup);
            }
            rules.Clear();
            return keyAssetList.ToArray();
        }

        /// <summary>
        /// Represents a group of rules which are accessed via a specific KeyAsset.
        /// </summary>
        [Serializable]
        public struct RuleGroup
        {
            [Tooltip("The name of the key asset, this must either be the name of an ItemDef or an EquipmentDef")]
            public string keyAssetName;
            [Tooltip("The rules contained in this RuleGroup")]
            public List<DisplayRule> rules;

            /// <summary>
            /// Checks if this rule group is empty or not
            /// </summary>
            public bool isEmpty => rules != null ? rules.Count == 0 : true;

            /// <summary>
            /// Adds a new DisplayRule to this RuleGroup
            /// </summary>
            /// <param name="rule"></param>
            public void AddRule(DisplayRule rule)
            {
                rules = rules ?? new List<DisplayRule>();

                rules.Add(rule);
            }
        }

        /// <summary>
        /// Represents a DisplayRule for an entry inside a <see cref="RuleGroup"/>.
        /// <para>Contains sufficient metadata for transforming into a <see cref="RoR2.ItemDisplayRule"/> at runtime</para>
        /// </summary>
        [Serializable]
        public struct DisplayRule
        {
            [Tooltip("The type of rule this display uses")]
            public ItemDisplayRuleType ruleType;
            [Tooltip("The name of the display prefab to use for this rule.")]
            public string displayPrefabName;
            [Tooltip("The name of a ChildLocator entry to instantiate the display prefab")]
            public string childName;
            [Tooltip("The display prefab's position relative to it's parent.")]
            public Vector3 localPos;
            [Tooltip("The display prefab's rotation relative to it's parent.")]
            public Vector3 localAngles;
            [Tooltip("The display prefab's scale relative to it's parent.")]
            public Vector3 localScale;
            [Tooltip("A mask of limbs to occlude when this display rule becomes active.")]
            public LimbFlags limbMask;

            /// <summary>
            /// Represents a display rule which has it's <see cref="childName"/> set to "NoValue"
            /// <br>This in turn will setup the display rule to use the first entry of the model's child locator, which then can be modified to be placed correctly using the ItemDisplayPlacementHelper</br>
            /// </summary>
            public const string NO_VALUE = "NoValue";

            /// <summary>
            /// Contains the finished rule from this DisplayRule
            /// </summary>
            public ItemDisplayRule finishedRule
            {
                get
                {
                    if(!_finishedRule.HasValue)
                    {
                        GameObject prefab = ItemDisplayCatalog.GetItemDisplay(displayPrefabName);
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