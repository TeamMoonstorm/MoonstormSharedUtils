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
        /*[Serializable]
        public struct NamedRuleGroup
        {
            public string keyAssetName;
            public List<NamedDisplayRule> rules;

            public bool IsEmpty { get => rules != null ? rules.Count == 0 : true; }

            public void AddRule(NamedDisplayRule rule)
            {
                if (rules == null)
                    rules = new List<NamedDisplayRule>();

                rules.Add(rule);
            }
        }
        [Serializable]
        public struct NamedDisplayRule
        {
            public ItemDisplayRuleType ruleType;
            public string displayPrefabName;
            [Tooltip("Values taken from the ItemDisplayPlacementHelper\nMake sure to use the copy format \"For Parsing\"!.")]
            [TextArea(1, int.MaxValue)]
            public string IDPHValues;
            public LimbFlags limbMask;

            [HideInInspector]
            public ItemDisplayRule finishedRule;

            public const string NoValue = nameof(NoValue);

            internal void Parse()
            {
                if(IDPHValues == string.Empty)
                {
                    finishedRule = new ItemDisplayRule
                    {
                        childName = "NoValue",
                        localAngles = Vector3.zero,
                        localPos = Vector3.zero,
                        localScale = Vector3.zero,
                        followerPrefab = ItemDisplayModuleBase.GetFollowerPrefab(displayPrefabName),
                        limbMask = limbMask,
                        ruleType = ruleType
                    };
                    return;
                }

                finishedRule = new ItemDisplayRule
                {
                    followerPrefab = ItemDisplayModuleBase.GetFollowerPrefab(displayPrefabName),
                    limbMask = limbMask,
                    ruleType = ruleType
                };

                List<string> splitValues = IDPHValues.Split(',').ToList();
                finishedRule.childName = splitValues[0];
                List<string> V3Builder = new List<string>();

                V3Builder.Add(splitValues[1]);
                V3Builder.Add(splitValues[2]);
                V3Builder.Add(splitValues[3]);
                finishedRule.localPos = CreateVector3(V3Builder);

                V3Builder.Clear();
                V3Builder.Add(splitValues[4]);
                V3Builder.Add(splitValues[5]);
                V3Builder.Add(splitValues[6]);
                finishedRule.localAngles = CreateVector3(V3Builder);

                V3Builder.Clear();
                V3Builder.Add(splitValues[7]);
                V3Builder.Add(splitValues[8]);
                V3Builder.Add(splitValues[9]);
                finishedRule.localScale = CreateVector3(V3Builder);
            }

            private Vector3 CreateVector3(List<string> list)
            {
                return new Vector3(GetFloat(list[0]), GetFloat(list[1]), GetFloat(list[2]));

                float GetFloat(string floatAsText) => float.Parse(floatAsText, CultureInfo.InvariantCulture);
            }
        }

        internal ItemDisplayRuleSet IDRS
        {
            get
            {
                if (!_idrs)
                    _idrs = ItemDisplayModuleBase.GetIDRS(idrsName);
                return _idrs;
            }
        }
        private ItemDisplayRuleSet _idrs;

        [Space(2)]
        public List<NamedRuleGroup> namedRuleGroups = new List<NamedRuleGroup>();
        public string idrsName;

        internal ItemDisplayRuleSet.KeyAssetRuleGroup[] GetKeyAssetRuleGroups()
        {
            var keyAssetList = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();
            foreach(var namedRuleGroup in namedRuleGroups)
            {
                var keyAssetGroup = new ItemDisplayRuleSet.KeyAssetRuleGroup();
                if(ItemDisplayModuleBase.GetKeyAsset(namedRuleGroup.keyAssetName, out keyAssetGroup.keyAsset))
                {
                    keyAssetGroup.displayRuleGroup = new DisplayRuleGroup();
                }
                if(!keyAssetGroup.keyAsset)
                {
                    MSULog.Warning($"Could not find KeyAsset of name {namedRuleGroup.keyAssetName} in {this}");
                    continue;
                }

                for(int i = 0; i < namedRuleGroup.rules.Count; i++)
                {
                    NamedDisplayRule rule = namedRuleGroup.rules[i];
                    rule.Parse();
                    keyAssetGroup.displayRuleGroup.AddDisplayRule(rule.finishedRule);
                }
                keyAssetList.Add(keyAssetGroup);
            }
            namedRuleGroups.Clear();
            return keyAssetList.ToArray();
        }*/
    }
}
