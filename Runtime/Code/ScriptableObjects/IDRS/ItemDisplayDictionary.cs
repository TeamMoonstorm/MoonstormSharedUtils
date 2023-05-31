using BepInEx;
using Moonstorm.AddressableAssets;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// An <see cref="ItemDisplayDictionary"/> is used for appending a single <see cref="ItemDisplayRuleSet.KeyAssetRuleGroup"/> to multiple <see cref="ItemDisplayRuleSet"/>
    /// <para>It works in a similar fashion to R2API's ItemDisplayDictionary</para>
    /// </summary>
    [CreateAssetMenu(fileName = "New ItemDisplayDictionary", menuName = "Moonstorm/IDRS/ItemDisplayDictionary")]
    public class ItemDisplayDictionary : ScriptableObject
    {
        /// <summary>
        /// Represents a dictionary of an IDRS to the rules that will be appended.
        /// </summary>
        [Serializable]
        public struct NamedDisplayDictionary
        {
            [Tooltip("The IDRS to add the rules below to")]
            public string idrsName;
            [Obsolete("Use idrsName instead")] public AddressableIDRS idrs;
            [Tooltip("The rules for the IDRS above")]
            public List<DisplayRule> displayRules;

            /// <summary>
            /// Returns true if <see cref="displayRules"/>'s count is 0 or if its null
            /// </summary>
            public bool IsEmpty { get => displayRules != null ? displayRules.Count == 0 : true; }

            /// <summary>
            /// Adds a new rule
            /// </summary>
            /// <param name="rule">The rule to add</param>
            public void AddDisplayRule(DisplayRule rule)
            {
                if (displayRules == null)
                    displayRules = new List<DisplayRule>();

                displayRules.Add(rule);
            }
        }

        /// <summary>
        /// Wrapper for <see cref="ItemDisplayRule"/>
        /// <para>The <see cref="ItemDisplayRule"/>'s display prefab will be taken from <see cref="displayPrefab"/></para>
        /// </summary>
        [Serializable]
        public struct DisplayRule
        {
            [Tooltip("The type of display rule")]
            public ItemDisplayRuleType ruleType;
            [Tooltip("The index of the display prefab, taken from the Display Prefabs arrays")]
            public int displayPrefabIndex;
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
            [HideInInspector, NonSerialized]
            public ItemDisplayRule finishedRule;

            /// <summary>
            /// A constant for a <see cref="DisplayRule"/> that has no value
            /// </summary>
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

        /// <summary>
        /// Contains all instances of <see cref="ItemDisplayDictionary"/>
        /// </summary>
        public static readonly List<ItemDisplayDictionary> instances = new List<ItemDisplayDictionary>();

        [Tooltip("The key asset provided will be appended to all the ItemDisplayRuleSets defined in namedDisplayDictionary")]
        public UnityEngine.Object keyAsset;
        [Tooltip("An array of valid display prefabs for this Item")]
        public GameObject[] displayPrefabs = Array.Empty<GameObject>();
        [Obsolete("Use DisplayPrefabs and specify the displayPrefabIndex on the DisplayRule")]
        public GameObject displayPrefab;

        [Tooltip("Implement the rules for this Dictionary")]
        [Space]
        public List<NamedDisplayDictionary> namedDisplayDictionary = new List<NamedDisplayDictionary>();

        private void Awake()
        {
            instances.AddIfNotInCollection(this);
#if !UNITY_EDITOR
            ItemDisplayCatalog.AddDisplay(this);
            Upgrade();
#endif
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

        [ContextMenu("Upgrade for SerializedItemDisplayCatalog")]
        private void Upgrade()
        {
            int indexOfDisplayPrefab = displayPrefabs.Length;
            HG.ArrayUtils.ArrayAppend(ref displayPrefabs, displayPrefab);

            for (int i = 0; i < namedDisplayDictionary.Count; i++)
            {
                namedDisplayDictionary[i] = UpgradeNamedDisplayDictionary(namedDisplayDictionary[i], i);
                for (int j = 0; j < namedDisplayDictionary[i].displayRules.Count; j++)
                {
                    namedDisplayDictionary[i].displayRules[j] = UpgradeDisplayRule(namedDisplayDictionary[i].displayRules[j], i, j);
                }
            }

            NamedDisplayDictionary UpgradeNamedDisplayDictionary(NamedDisplayDictionary input, int index)
            {
                var copy = input;
                try
                {
                    if (!input.idrsName.IsNullOrWhiteSpace())
                        return input;

                    string idrsName = null;

                    if (!input.idrs.address.IsNullOrWhiteSpace())
                    {
                        string[] split = input.idrs.address.Split('/');
                        split = split[split.Length - 1].Split('.');
                        idrsName = split[0];
                    }
                    else

                    {
                        idrsName = input.idrs.Asset.name;
                    }
                    input.idrsName = idrsName;
                    return input;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to upgrade AddressNamedRuleGroup at index {index} for {this}.\n{e}");
                    return copy;
                }
            }

            DisplayRule UpgradeDisplayRule(DisplayRule input, int dictionaryIndex, int ruleIndex)
            {
                var copy = input;
                try
                {
                    if (input.displayPrefabIndex == indexOfDisplayPrefab)
                    {
                        return input;
                    }

                    input.displayPrefabIndex = indexOfDisplayPrefab;
                    return input;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to upgrade Displayrule from dictionary index {dictionaryIndex} at {ruleIndex} for {this}.\n{e}");
                    return copy;
                }
            }
        }
    }
}