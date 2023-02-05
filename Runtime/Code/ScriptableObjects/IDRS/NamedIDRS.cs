using Moonstorm.AddressableAssets;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="NamedIDRS"/> is a version of an <see cref="ItemDisplayRuleSet"/> that can be populated and serialized in the editor.
    /// <para>The values in <see cref="namedRuleGroups"/> will be appended to the IDRS set in <see cref="idrs"/></para>
    /// </summary>
    [CreateAssetMenu(fileName = "New NamedIDRS", menuName = "Moonstorm/IDRS/NamedIDRS")]
    public class NamedIDRS : ScriptableObject
    {
        /// <summary>
        /// Wrapper for <see cref="ItemDisplayRuleSet.KeyAssetRuleGroup"/>.
        /// <para>The key asset can be loaded either via the <see cref="ItemCatalog"/>, <see cref="EquipmentCatalog"/> or via Addressables</para>
        /// </summary>
        [Serializable]
        public struct AddressNamedRuleGroup
        {
            [Tooltip("The key asset that this rule group uses")]
            public AddressableKeyAsset keyAsset;
            [Tooltip("The rules this rule group has")]
            public List<AddressNamedDisplayRule> rules;

            /// <summary>
            /// Returns true if <see cref="rules"/>'s count is 0 or if its null
            /// </summary>
            public bool IsEmpty { get => rules != null ? rules.Count == 0 : true; }

            /// <summary>
            /// Adds a new rule
            /// </summary>
            /// <param name="rule">The rule to add</param>
            public void AddRule(AddressNamedDisplayRule rule)
            {
                if (rules == null)
                    rules = new List<AddressNamedDisplayRule>();

                rules.Add(rule);
            }
        }
        /// <summary>
        /// Wrapper for <see cref="ItemDisplayRule"/>
        /// <param>The display prefab can be loaded via addressables or direct reference</param>
        /// </summary>
        [Serializable]
        public struct AddressNamedDisplayRule
        {
            [Tooltip("The type of display rule")]
            public ItemDisplayRuleType ruleType;
            [Tooltip("The display prefab")]
            public AddressableGameObject displayPrefab;
            [Tooltip("The name of the child where this display prefab will appear")]
            public string childName;
            [Tooltip("The local position of this display")]
            public Vector3 localPos;
            [Tooltip("The local angle of this display")]
            public Vector3 localAngles;
            [Tooltip("The local scale for this display")]
            public Vector3 localScales;
            [Tooltip("If supplied, this display will replace a limb, ask in the ror2 modding discord if you dont know what this does")]
            public LimbFlags limbMask;

            /// <summary>
            /// The finished rule
            /// </summary>
            [HideInInspector]
            public ItemDisplayRule finishedRule;

            /// <summary>
            /// A constant for an <see cref="AddressNamedRuleGroup"/> that  has no value
            /// </summary>
            public const string NoValue = nameof(NoValue);

            internal void CreateRule()
            {
                if (string.IsNullOrEmpty(childName))
                {
                    finishedRule = new ItemDisplayRule
                    {
                        childName = "NoValue",
                        localAngles = Vector3.zero,
                        localPos = Vector3.zero,
                        localScale = Vector3.zero,
                        followerPrefab = displayPrefab.Asset,
                        limbMask = limbMask,
                        ruleType = ruleType
                    };
                    return;
                }

                finishedRule = new ItemDisplayRule
                {
                    childName = childName,
                    localAngles = localAngles,
                    localPos = localPos,
                    localScale = localScales,
                    followerPrefab = displayPrefab.Asset,
                    limbMask = limbMask,
                    ruleType = ruleType
                };
            }
        }

        /// <summary>
        /// Contains all the instances of <see cref="NamedIDRS"/>
        /// </summary>
        public static readonly List<NamedIDRS> instances = new List<NamedIDRS>();
        [Tooltip("The namedRuleGroups serialized in this NamedIDRS will be copied over to this IDRS")]
        public ItemDisplayRuleSet idrs;

        [Tooltip("Implement the rule groups for this IDRS")]
        [Space]
        public List<AddressNamedRuleGroup> namedRuleGroups = new List<AddressNamedRuleGroup>();

        private void Awake()
        {
            instances.AddIfNotInCollection(this);
        }
        private void OnDestroy()
        {
            instances.RemoveIfNotInCollection(this);
        }

        [SystemInitializer]
        private static void SystemInitializer()
        {
            AddressableAssets.AddressableAsset.OnAddressableAssetsLoaded += () =>
            {
                MSULog.Info($"Initializing NamedIDRS");
                foreach (NamedIDRS namedIdrs in instances)
                {
                    try
                    {
                        foreach (ItemDisplayRuleSet.KeyAssetRuleGroup keyAssetRuleGroup in namedIdrs.GetKeyAssetRuleGroups())
                        {
                            HG.ArrayUtils.ArrayAppend(ref namedIdrs.idrs.keyAssetRuleGroups, keyAssetRuleGroup);
                        }
                        namedIdrs.idrs.GenerateRuntimeValues();
#if DEBUG
                        MSULog.Debug($"Finished appending values from {namedIdrs} to {namedIdrs.idrs}");
#endif
                    }
                    catch (Exception e)
                    {
                        MSULog.Error($"{e}\n({namedIdrs}");
                    }
                }
            };
        }

        internal ItemDisplayRuleSet.KeyAssetRuleGroup[] GetKeyAssetRuleGroups()
        {
            var keyAssetList = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();
            foreach (var namedRuleGroup in namedRuleGroups)
            {
                var keyAsset = namedRuleGroup.keyAsset.Asset;
                if (keyAsset is EquipmentDef ed)
                {
                    EquipmentIndex index = EquipmentCatalog.FindEquipmentIndex(keyAsset.name);
                    if (index == EquipmentIndex.None)
                    {
#if DEBUG
                        MSULog.Debug($"Not generating key asset rule group for {keyAsset.name} as its index is none.");
#endif
                        continue;
                    }
                }
                else if (keyAsset is ItemDef id)
                {
                    ItemIndex index = ItemCatalog.FindItemIndex(id.name);
                    if (index == ItemIndex.None)
                    {
#if DEBUG
                        MSULog.Debug($"Not generating key asset rule group for {keyAsset.name} as its index is none.");
#endif
                        continue;
                    }
                }

                var keyAssetGroup = new ItemDisplayRuleSet.KeyAssetRuleGroup { keyAsset = keyAsset };

                for (int i = 0; i < namedRuleGroup.rules.Count; i++)
                {
                    AddressNamedDisplayRule rule = namedRuleGroup.rules[i];
                    rule.CreateRule();
                    keyAssetGroup.displayRuleGroup.AddDisplayRule(rule.finishedRule);
                }
                keyAssetList.Add(keyAssetGroup);
            }
            namedRuleGroups.Clear();
            return keyAssetList.ToArray();
        }
    }
}
