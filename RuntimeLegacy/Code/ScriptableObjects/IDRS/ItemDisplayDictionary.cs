using BepInEx;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm
{
    public class ItemDisplayDictionary : ScriptableObject
    {
        [Serializable]
        public struct NamedDisplayDictionary
        {
            public string idrsName;
            public List<DisplayRule> displayRules;

            public bool IsEmpty { get => displayRules != null ? displayRules.Count == 0 : true; }

            public void AddDisplayRule(DisplayRule rule)
            {
                if (displayRules == null)
                    displayRules = new List<DisplayRule>();

                displayRules.Add(rule);
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

            public Vector3 localScales;

            public LimbFlags limbMask;

            [HideInInspector, NonSerialized]
            public ItemDisplayRule finishedRule;

            public const string NoValue = nameof(NoValue);

            internal void CreateRule(GameObject[] displayPrefabs)
            {
                if (string.IsNullOrEmpty(childName))
                {
                    finishedRule = new ItemDisplayRule
                    {
                        childName = NoValue,
                        followerPrefab = displayPrefabs[displayPrefabIndex],
                        localAngles = Vector3.zero,
                        localPos = Vector3.zero,
                        localScale = Vector3.zero,
                        limbMask = limbMask,
                        ruleType = ruleType
                    };
                    return;
                }

                finishedRule = new ItemDisplayRule
                {
                    childName = childName,
                    followerPrefab = displayPrefabs[displayPrefabIndex],
                    localAngles = localAngles,
                    localPos = localPos,
                    localScale = localScales,
                    limbMask = limbMask,
                    ruleType = ruleType
                };
            }
        }

        public static readonly List<ItemDisplayDictionary> instances = new List<ItemDisplayDictionary>();

        public UnityEngine.Object keyAsset;

        public GameObject[] displayPrefabs = Array.Empty<GameObject>();

        [Space]
        public List<NamedDisplayDictionary> namedDisplayDictionary = new List<NamedDisplayDictionary>();

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
                MSULog.Info($"Initializing ItemDisplayDictionary");
                List<ItemDisplayRuleSet> idrsToRegenerateRuntimeValues = new List<ItemDisplayRuleSet>();
                foreach (ItemDisplayDictionary itemDisplayDictionary in instances)
                {
                    var keyAsset = itemDisplayDictionary.keyAsset;
                    if (keyAsset is EquipmentDef ed)
                    {
                        EquipmentIndex index = EquipmentCatalog.FindEquipmentIndex(keyAsset.name);
                        if (index == EquipmentIndex.None)
                        {
#if DEBUG
                            MSULog.Debug($"Not appending values from {itemDisplayDictionary}, as its KeyAsset's index is none.");
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
                            MSULog.Debug($"Not appending values from {itemDisplayDictionary}, as its KeyAsset's index is none.");
#endif
                            continue;
                        }
                    }

                    for (int i = 0; i < itemDisplayDictionary.namedDisplayDictionary.Count; i++)
                    {
                        try
                        {
                            var current = itemDisplayDictionary.namedDisplayDictionary[i];
                            ItemDisplayRuleSet target = ItemDisplayCatalog.GetItemDisplayRuleSet(current.idrsName);
                            if (!target)
                            {
#if DEBUG
                                MSULog.Warning($"Not appending values of {itemDisplayDictionary}'s {i} index, as the target idrs is null.");
                                continue;
#endif
                            }
                            var keyAssetRuleGroup = itemDisplayDictionary.GetKeyAssetRuleGroup(current.idrsName);
                            HG.ArrayUtils.ArrayAppend(ref target.keyAssetRuleGroups, keyAssetRuleGroup);
                            idrsToRegenerateRuntimeValues.AddIfNotInCollection(target);
#if DEBUG
                            MSULog.Debug($"Finished appending values from {itemDisplayDictionary}'s {i} entry into {target}");
#endif
                        }
                        catch (Exception e)
                        {
                            MSULog.Error($"{e}\n({itemDisplayDictionary} index {i}");
                        }
                    }
#if DEBUG
                    MSULog.Debug($"Finished appending values of {itemDisplayDictionary}");
#endif
                }

                foreach (var idrs in idrsToRegenerateRuntimeValues)
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
            var keyAssetRuleGroup = new ItemDisplayRuleSet.KeyAssetRuleGroup();
            keyAssetRuleGroup.keyAsset = keyAsset;
            keyAssetRuleGroup.displayRuleGroup = new DisplayRuleGroup();

            var index = namedDisplayDictionary.FindIndex(x => x.idrsName == key);
            if (index >= 0)
            {
                var namedDisplay = namedDisplayDictionary[index];
                for (int i = 0; i < namedDisplay.displayRules.Count; i++)
                {
                    DisplayRule rule = namedDisplay.displayRules[i];
                    rule.CreateRule(displayPrefabs);
                    var finishedRule = rule.finishedRule;
                    keyAssetRuleGroup.displayRuleGroup.AddDisplayRule(finishedRule);
                }
            }
            return keyAssetRuleGroup;
        }
    }
}