using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New MSIDRS", menuName = "Moonstorm/IDRS/MSIDRS", order = 0)]
    public class MSIDRS : ScriptableObject
    {
        [Serializable]
        public struct KeyAssetRuleGroup
        {
            public string keyAssetName;
            public List<ItemDisplayRule> rules;

            public bool isEmpty
            {
                get
                {
                    if (rules != null)
                    {
                        return rules.Count == 0;
                    }
                    return true;
                }
            }

            public void AddDisplayRule(ItemDisplayRule itemDisplayRule)
            {
                if (rules == null)
                {
                    rules = new List<ItemDisplayRule>();
                }
                rules.Add(itemDisplayRule);
            }
        }
        [Serializable]
        public struct ItemDisplayRule
        {
            public ItemDisplayRuleType ruleType;

            public string displayPrefabName;

            [Tooltip("Values taken from the ItemDisplayPlacementHelper\nMake sure to use the copy format \"For Parsing\"!.")]
            [TextArea(1, int.MaxValue)]
            public string IDPHValues;

            public LimbFlags limbMask;

            internal string childName;
            internal Vector3 localPos;
            internal Vector3 localAngles;
            internal Vector3 localScale;

            private List<string> V3Builder;

            public const string NoValue = "NoValue";

            public void Parse()
            {
                if (IDPHValues == string.Empty)
                {
                    childName = NoValue;
                    localPos = new Vector3(0, 0, 0);
                    localAngles = new Vector3(0, 0, 0);
                    localScale = new Vector3(1, 1, 1);
                    return;
                }
                List<string> splitValues = IDPHValues.Split(',').ToList();
                childName = splitValues[0];
                V3Builder = new List<string>();
                V3Builder.Clear();
                V3Builder.Add(splitValues[1]);
                V3Builder.Add(splitValues[2]);
                V3Builder.Add(splitValues[3]);
                localPos = CreateVector3FromList(V3Builder);

                V3Builder.Clear();
                V3Builder.Add(splitValues[4]);
                V3Builder.Add(splitValues[5]);
                V3Builder.Add(splitValues[6]);
                localAngles = CreateVector3FromList(V3Builder);

                V3Builder.Clear();
                V3Builder.Add(splitValues[7]);
                V3Builder.Add(splitValues[8]);
                V3Builder.Add(splitValues[9]);
                localScale = CreateVector3FromList(V3Builder);
            }
            private Vector3 CreateVector3FromList(List<string> list)
            {
                Vector3 toReturn = new Vector3(float.Parse(list[0], CultureInfo.InvariantCulture), float.Parse(list[1], CultureInfo.InvariantCulture), float.Parse(list[2], CultureInfo.InvariantCulture));

                return toReturn;
            }
        }

        public static List<MSIDRS> instancesList = new List<MSIDRS>();
        internal ItemDisplayRuleSet vanillaIDRS;

        [Space]
        public List<KeyAssetRuleGroup> MSUKeyAssetRuleGroup = new List<KeyAssetRuleGroup>();
        public string VanillaIDRSKey;

        public void Awake()
        {
            instancesList.Add(this);
        }
        public void FetchIDRS()
        {
            if (ItemDisplayModuleBase.vanillaIDRS.TryGetValue(VanillaIDRSKey.ToLowerInvariant(), out var value))
            {
                if (value != null)
                {
                    vanillaIDRS = value;
                }
            }
        }

        public ItemDisplayRuleSet.KeyAssetRuleGroup[] GetItemDisplayRules()
        {
            var keyAssetList = new List<RoR2.ItemDisplayRuleSet.KeyAssetRuleGroup>();
            foreach (var SS2KeyAssetRuleGroup in MSUKeyAssetRuleGroup)
            {
                var keyAssetGroup = new RoR2.ItemDisplayRuleSet.KeyAssetRuleGroup();
                if (ItemDisplayModuleBase.itemKeyAssets.TryGetValue(SS2KeyAssetRuleGroup.keyAssetName.ToLowerInvariant(), out keyAssetGroup.keyAsset))
                {
                    keyAssetGroup.displayRuleGroup = new RoR2.DisplayRuleGroup { rules = Array.Empty<RoR2.ItemDisplayRule>() };
                }
                else if (ItemDisplayModuleBase.equipKeyAssets.TryGetValue(SS2KeyAssetRuleGroup.keyAssetName.ToLowerInvariant(), out keyAssetGroup.keyAsset))
                {
                    keyAssetGroup.displayRuleGroup = new RoR2.DisplayRuleGroup { rules = Array.Empty<RoR2.ItemDisplayRule>() };
                }
                if (keyAssetGroup.keyAsset == null)
                {
                    continue;
                }
                for (int i = 0; i < SS2KeyAssetRuleGroup.rules.Count; i++)
                {
                    ItemDisplayRule rule = SS2KeyAssetRuleGroup.rules[i];
                    rule.Parse();
                    var prefab = ItemDisplayModuleBase.LoadDisplay(rule.displayPrefabName.ToLowerInvariant());
                    HG.ArrayUtils.ArrayAppend(ref keyAssetGroup.displayRuleGroup.rules, new RoR2.ItemDisplayRule
                    {
                        ruleType = rule.ruleType,
                        followerPrefab = prefab,
                        childName = rule.childName,
                        localPos = rule.localPos,
                        localAngles = rule.localAngles,
                        localScale = rule.localScale,
                        limbMask = rule.limbMask
                    });
                }
                keyAssetList.Add(keyAssetGroup);
            }
            return keyAssetList.ToArray();
        }
    }
}