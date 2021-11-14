using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New SingleItemDisplayRule", menuName = "Moonstorm/IDRS/SingleItemDisplayRule")]
    public class MSSingleItemDisplayRule : ScriptableObject
    {
        [Serializable]
        public struct SingleKeyAssetRuleGroup
        {
            public string vanillaIDRSKey;

            public List<SingleItemDisplayRule> itemDisplayRules;

            internal ItemDisplayRuleSet vanillaIDRS;
            public bool isEmpty
            {
                get
                {
                    if (itemDisplayRules != null)
                    {
                        return itemDisplayRules.Count == 0;
                    }
                    return true;
                }
            }

            public void AddDisplayRule(SingleItemDisplayRule itemDisplayRule)
            {
                if (itemDisplayRules == null)
                {
                    itemDisplayRules = new List<SingleItemDisplayRule>();
                }
                itemDisplayRules.Add(itemDisplayRule);
            }
            public void FetchIDRS()
            {
                if (ItemDisplayModuleBase.vanillaIDRS.TryGetValue(vanillaIDRSKey.ToLowerInvariant(), out var value))
                {
                    if (value != null)
                    {
                        vanillaIDRS = value;
                    }
                }
            }
        }
        [Serializable]
        public struct SingleItemDisplayRule
        {
            public ItemDisplayRuleType ruleType;

            [Tooltip("Values taken from the ItemDisplayPlacementHelper\nMake sure to use the copy format \"For Parsing\"!.")]
            [TextArea(1, int.MaxValue)]
            public string IDPHValues;

            public LimbFlags limbMask;

            internal string childName;
            internal Vector3 localPos;
            internal Vector3 localAngles;
            internal Vector3 localScale;

            private List<string> V3Builder;

            internal const string constant = "NoValue";

            public void Parse()
            {
                if (IDPHValues == string.Empty)
                {
                    childName = constant;
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

        public string keyAssetName;
        public string displayPrefabName;

        [Space]
        public List<SingleKeyAssetRuleGroup> singleItemDisplayRules = new List<SingleKeyAssetRuleGroup>();

        public ItemDisplayRuleSet.KeyAssetRuleGroup Parse(int index)
        {
            var KeyAssetToReturn = new ItemDisplayRuleSet.KeyAssetRuleGroup();
            if (ItemDisplayModuleBase.itemKeyAssets.TryGetValue(keyAssetName.ToLowerInvariant(), out KeyAssetToReturn.keyAsset))
            {
                KeyAssetToReturn.displayRuleGroup = new DisplayRuleGroup { rules = Array.Empty<RoR2.ItemDisplayRule>() };
            }
            else if (ItemDisplayModuleBase.equipKeyAssets.TryGetValue(keyAssetName.ToLowerInvariant(), out KeyAssetToReturn.keyAsset))
            {
                KeyAssetToReturn.displayRuleGroup = new DisplayRuleGroup { rules = Array.Empty<RoR2.ItemDisplayRule>() };
            }
            if (KeyAssetToReturn.keyAsset == null)
            {
                return new ItemDisplayRuleSet.KeyAssetRuleGroup { keyAsset = null };
            }
            for (int i = 0; i < singleItemDisplayRules[index].itemDisplayRules.Count; i++)
            {
                SingleItemDisplayRule rule = singleItemDisplayRules[index].itemDisplayRules[i];
                rule.Parse();
                var prefab = ItemDisplayModuleBase.LoadDisplay(displayPrefabName.ToLowerInvariant());
                HG.ArrayUtils.ArrayAppend(ref KeyAssetToReturn.displayRuleGroup.rules, new RoR2.ItemDisplayRule
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
            return KeyAssetToReturn;
        }
    }
}
