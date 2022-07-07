using Moonstorm.AddressableAssets;
using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// An <see cref="ItemDisplayDictionary"/> is used for appending a single <see cref="ItemDisplayRuleSet.KeyAssetRuleGroup"/> to multiple <see cref="ItemDisplayRuleSet"/>
    /// <para>It works ina  similar fashion to R2API's ItemDisplayDictionary</para>
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
            public AddressableIDRS idrs;
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
            /// A constant for a <see cref="DisplayRule"/> that has no value
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
        [Tooltip("The game object that will be used as a display prefab")]
        public GameObject displayPrefab;

        [Tooltip("Implement the rules for this Dictionary")]
        [Space]
        public List<NamedDisplayDictionary> namedDisplayDictionary = new List<NamedDisplayDictionary>();

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
                MSULog.Info($"Initializing ItemDisplayDictionary");
                foreach (ItemDisplayDictionary itemDisplayDictionary in instances)
                {
                    var keyAsset = itemDisplayDictionary.keyAsset;
                    if (keyAsset is EquipmentDef ed)
                    {
                        EquipmentIndex index = EquipmentCatalog.FindEquipmentIndex(keyAsset.name);
                        if (index == EquipmentIndex.None)
                        {
                            MSULog.Debug($"Not appending values from {itemDisplayDictionary}, as its KeyAsset's index is none.");
                            continue;
                        }
                    }
                    else if (keyAsset is ItemDef id)
                    {
                        ItemIndex index = ItemCatalog.FindItemIndex(id.name);
                        if (index == ItemIndex.None)
                        {
                            MSULog.Debug($"Not appending values from {itemDisplayDictionary}, as its KeyAsset's index is none.");
                            continue;
                        }
                    }

                    for (int i = 0; i < itemDisplayDictionary.namedDisplayDictionary.Count; i++)
                    {
                        try
                        {
                            var current = itemDisplayDictionary.namedDisplayDictionary[i];
                            var keyAssetRuleGroup = itemDisplayDictionary.GetKeyAssetRuleGroup(current.idrs.Asset);
                            HG.ArrayUtils.ArrayAppend(ref current.idrs.Asset.keyAssetRuleGroups, keyAssetRuleGroup);

                            current.idrs.Asset.GenerateRuntimeValues();
                            MSULog.Debug($"Finished appending values from {itemDisplayDictionary}'s {i} entry into {current.idrs.Asset}");
                        }
                        catch(Exception e)
                        {
                            MSULog.Error($"{e}\n({itemDisplayDictionary} index {i}");
                        }
                    }
                    MSULog.Debug($"Finished appending values of {itemDisplayDictionary}");
                }
            };
        }
        private ItemDisplayRuleSet.KeyAssetRuleGroup GetKeyAssetRuleGroup(ItemDisplayRuleSet ruleSet)
        {
            var keyAssetRuleGroup = new ItemDisplayRuleSet.KeyAssetRuleGroup();
            keyAssetRuleGroup.keyAsset = keyAsset;
            keyAssetRuleGroup.displayRuleGroup = new DisplayRuleGroup();

            var index = namedDisplayDictionary.FindIndex(x => x.idrs.Asset == ruleSet);
            if(index >= 0)
            {
                var namedDisplay = namedDisplayDictionary[index];
                for(int i = 0; i < namedDisplay.displayRules.Count; i++)
                {
                    DisplayRule rule = namedDisplay.displayRules[i];
                    rule.CreateRule();
                    var finishedRule = rule.finishedRule;
                    finishedRule.followerPrefab = displayPrefab;
                    keyAssetRuleGroup.displayRuleGroup.AddDisplayRule(finishedRule);
                }
            }
            return keyAssetRuleGroup;
        }
    }
}