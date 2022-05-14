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
            public List<AdressNamedDisplayRule> rules;

            public bool IsEmpty { get => rules != null ? rules.Count == 0 : true; }

            public void AddRule(AdressNamedDisplayRule rule)
            {
                if (rules == null)
                    rules = new List<AdressNamedDisplayRule>();

                rules.Add(rule);
            }
        }
        [Serializable]
        public struct AdressNamedDisplayRule
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

        public ItemDisplayRuleSet idrs;

        [Space(2)]
        public List<AddressNamedRuleGroup> namedRuleGroups = new List<AddressNamedRuleGroup>();

        internal ItemDisplayRuleSet.KeyAssetRuleGroup[] GetKeyAssetRuleGroups()
        {
            var keyAssetList = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();
            foreach(var namedRuleGroup in namedRuleGroups)
            {
                var keyAssetGroup = new ItemDisplayRuleSet.KeyAssetRuleGroup { keyAsset = namedRuleGroup.keyAsset.Asset };

                for(int i = 0; i < namedRuleGroup.rules.Count; i++)
                {
                    AdressNamedDisplayRule rule = namedRuleGroup.rules[i];
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
