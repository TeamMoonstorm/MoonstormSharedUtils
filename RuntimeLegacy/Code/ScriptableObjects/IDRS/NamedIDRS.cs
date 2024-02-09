using BepInEx;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm
{
    public class NamedIDRS : ScriptableObject
    {
        [Serializable]
        public struct AddressNamedRuleGroup
        {
            public string keyAssetName;

            public List<AddressNamedDisplayRule> rules;

            public bool IsEmpty { get => rules != null ? rules.Count == 0 : true; }

            public void AddRule(AddressNamedDisplayRule rule)
            {
                if (rules == null)
                    rules = new List<AddressNamedDisplayRule>();

                rules.Add(rule);
            }
        }

        [Serializable]
        public struct AddressNamedDisplayRule
        {
            public ItemDisplayRuleType ruleType;

            public string displayPrefabName;

            public string childName;

            public Vector3 localPos;

            public Vector3 localAngles;

            public Vector3 localScales;

            public LimbFlags limbMask;

            [HideInInspector, NonSerialized]
            public ItemDisplayRule finishedRule;

            public const string NoValue = nameof(NoValue);

            internal void CreateRule()
            {
                GameObject prefab = ItemDisplayCatalog.GetItemDisplay(displayPrefabName);
                if (string.IsNullOrEmpty(childName))
                {
                    finishedRule = new ItemDisplayRule
                    {
                        childName = NoValue,
                        localAngles = Vector3.zero,
                        localPos = Vector3.zero,
                        localScale = Vector3.zero,
                        followerPrefab = prefab,
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
                    followerPrefab = prefab,
                    limbMask = limbMask,
                    ruleType = ruleType
                };
            }
        }

        public static readonly List<NamedIDRS> instances = new List<NamedIDRS>();

        public ItemDisplayRuleSet idrs;

        [Space]
        public List<AddressNamedRuleGroup> namedRuleGroups = new List<AddressNamedRuleGroup>();

        private void Awake()
        {
            instances.AddIfNotInCollection(this);
        }
        private void OnDestroy()
        {
            instances.RemoveIfInCollection(this);
        }

        [SystemInitializer]
        private static void SystemInitializer()
        {
            ItemDisplayCatalog.catalogAvailability.CallWhenAvailable(() =>
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
            });
        }

        internal ItemDisplayRuleSet.KeyAssetRuleGroup[] GetKeyAssetRuleGroups()
        {
            var keyAssetList = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();
            foreach (var namedRuleGroup in namedRuleGroups)
            {
                var keyAssetName = namedRuleGroup.keyAssetName;
                UnityEngine.Object keyAsset = null;
                var equipmentIndex = EquipmentCatalog.FindEquipmentIndex(keyAssetName);
                if (equipmentIndex != EquipmentIndex.None && !keyAsset)
                {
                    keyAsset = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
                }
                var itemIndex = ItemCatalog.FindItemIndex(keyAssetName);
                if (itemIndex != ItemIndex.None && !keyAsset)
                {
                    keyAsset = ItemCatalog.GetItemDef(itemIndex);
                }

                if (!keyAsset)
                {
#if DEBUG
                    MSULog.Warning($"Could not get key asset of name {keyAssetName} (Index: {namedRuleGroups.IndexOf(namedRuleGroup)}). {this}");
#endif
                    continue;
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
