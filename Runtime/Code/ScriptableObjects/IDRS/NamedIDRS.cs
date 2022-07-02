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
    [CreateAssetMenu(fileName = "New NamedIDRS", menuName = "Moonstorm/IDRS/NamedIDRS")]
    public class NamedIDRS : ScriptableObject
    {
        [Serializable]
        public struct AddressNamedRuleGroup
        {
            public AddressableKeyAsset keyAsset;
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
            public AddressableGameObject displayPrefab;
            public string childName;
            public Vector3 localPos;
            public Vector3 localAngles;
            public Vector3 localScales;
            public LimbFlags limbMask;

            [HideInInspector]
            public ItemDisplayRule finishedRule;

            public const string NoValue = nameof(NoValue);

            internal void CreateRule()
            {
                if(string.IsNullOrEmpty(childName))
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
                    foreach (ItemDisplayRuleSet.KeyAssetRuleGroup keyAssetRuleGroup in namedIdrs.GetKeyAssetRuleGroups())
                    {
                        HG.ArrayUtils.ArrayAppend(ref namedIdrs.idrs.keyAssetRuleGroups, keyAssetRuleGroup);
                    }
                    namedIdrs.idrs.GenerateRuntimeValues();
                    MSULog.Debug($"Finished appending values from {namedIdrs} to {namedIdrs.idrs}");
                }
            };
        }

        internal ItemDisplayRuleSet.KeyAssetRuleGroup[] GetKeyAssetRuleGroups()
        {
            var keyAssetList = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();
            foreach(var namedRuleGroup in namedRuleGroups)
            {
                var keyAssetGroup = new ItemDisplayRuleSet.KeyAssetRuleGroup { keyAsset = namedRuleGroup.keyAsset.Asset };

                for(int i = 0; i < namedRuleGroup.rules.Count; i++)
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
