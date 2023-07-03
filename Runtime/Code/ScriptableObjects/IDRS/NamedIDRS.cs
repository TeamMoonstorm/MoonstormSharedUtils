using BepInEx;
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
            [Tooltip("The Key Asset, usually an ItemDef or EquipmentDef name")]
            public string keyAssetName;
            [Obsolete("Use keyAssetName instead")] public AddressableKeyAsset keyAsset;
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
            [Tooltip("The Display Prefab")]
            public string displayPrefabName;
            [Obsolete("Use displayPrefabName instead")] public AddressableGameObject displayPrefab;
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
            /// A constant for an <see cref="AddressNamedRuleGroup"/> that  has no value
            /// </summary>
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
#if !UNITY_EDITOR
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

        [ContextMenu("Upgrade for SerializedItemDisplayCatalog")]
        private void Upgrade()
        {
            for (int i = 0; i < namedRuleGroups.Count; i++)
            {
                namedRuleGroups[i] = UpgradeRuleGroup(namedRuleGroups[i], i);
                for (int j = 0; j < namedRuleGroups[i].rules.Count; j++)
                {
                    namedRuleGroups[i].rules[j] = UpgradeRule(namedRuleGroups[i].rules[j], i, j);
                }
            }

            AddressNamedRuleGroup UpgradeRuleGroup(AddressNamedRuleGroup input, int index)
            {
                var copy = input;
                try
                {
                    if (!input.keyAssetName.IsNullOrWhiteSpace())
                        return input;

                    string keyAssetName = null;
#pragma warning disable CS0618 // Type or member is obsolete
                    switch (input.keyAsset.loadAssetFrom)
                    {
                        case AddressableKeyAsset.KeyAssetAddressType.UsingDirectReference:
                            keyAssetName = input.keyAsset.Asset.name;
                            break;
                        case AddressableKeyAsset.KeyAssetAddressType.Addressables:
                            string[] split = input.keyAsset.address.Split('/');
                            split = split[split.Length - 1].Split('.');
                            keyAssetName = split[0];
                            break;
                        default:
                            keyAssetName = input.keyAsset.address;
                            break;
                    }
#pragma warning restore CS0618 // Type or member is obsolete
                    input.keyAssetName = keyAssetName;
                    return input;
                }
                catch (Exception e)
                {
                    MSULog.Error($"Failed to upgrade AddressNamedRuleGroup at index {index} for {this}.\n{e}");
                    return copy;
                }
            }

            AddressNamedDisplayRule UpgradeRule(AddressNamedDisplayRule input, int groupIndex, int index)
            {
                var copy = input;
                try
                {
                    if (!input.displayPrefabName.IsNullOrWhiteSpace())
                    {
                        return input;
                    }

                    string displayPrefabName = null;
#pragma warning disable CS0618 // Type or member is obsolete
                    if (input.displayPrefab.address.IsNullOrWhiteSpace())
                    {
                        displayPrefabName = input.displayPrefab.Asset.name;
                    }
                    else
                    {
                        string[] split = input.displayPrefab.address.Split('/');
                        split = split[split.Length - 1].Split('.');
                        displayPrefabName = split[0];
                    }
#pragma warning restore CS0618 // Type or member is obsolete

                    input.displayPrefabName = displayPrefabName;
                    return input;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to upgrade AddressNamedDisplayRule from group index {groupIndex} at {index} for {this}.\n{e}");
                    return copy;
                }
            }
        }
    }
}
