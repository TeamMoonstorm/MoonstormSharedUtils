﻿using Moonstorm.AddressableAssets;
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
    [CreateAssetMenu(fileName = "New ItemDisplayDictionary", menuName = "Moonstorm/IDRS/ItemDisplayDictionary")]
    public class ItemDisplayDictionary : ScriptableObject
    {
        [Serializable]
        public struct NamedDisplayDictionary
        {
            public AddressableIDRS idrs;
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
            public string childName;
            public Vector3 localPos;
            public Vector3 localAngles;
            public Vector3 localScales;
            public LimbFlags limbMask;

            [HideInInspector]
            public ItemDisplayRule finishedRule;

            public const string NoValue = nameof(NoValue);

            internal void Parse()
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

        public UnityEngine.Object keyAsset;
        public GameObject displayPrefab;

        [Space]
        public List<NamedDisplayDictionary> namedDisplayDictionary = new List<NamedDisplayDictionary>();

        public ItemDisplayRuleSet.KeyAssetRuleGroup GetKeyAssetRuleGroup(ItemDisplayRuleSet ruleSet)
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
                    rule.Parse();
                    var finishedRule = rule.finishedRule;
                    finishedRule.followerPrefab = displayPrefab;
                    keyAssetRuleGroup.displayRuleGroup.AddDisplayRule(finishedRule);
                }
            }
            return keyAssetRuleGroup;
        }
    }
}