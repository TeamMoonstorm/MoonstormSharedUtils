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
        /*[Serializable]
        public struct NamedDisplayDictionary
        {
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
            [Tooltip("Values taken from the ItemDisplayPlacementHelper\nMake sure to use the copy format \"For Parsing\"!.")]
            [TextArea(1, int.MaxValue)]
            public string IDPHValues;
            public LimbFlags limbMask;

            [HideInInspector]
            public ItemDisplayRule finishedRule;

            public const string NoValue = nameof(NoValue);

            internal void Parse()
            {
                if (IDPHValues == string.Empty)
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

        public UnityEngine.Object keyAsset;
        public GameObject displayPrefab;

        [Space]
        public List<NamedDisplayDictionary> namedDisplayDictionary = new List<NamedDisplayDictionary>();

        public ItemDisplayRuleSet.KeyAssetRuleGroup GetKeyAssetRuleGroup(ItemDisplayRuleSet ruleSet)
        {
            var keyAssetRuleGroup = new ItemDisplayRuleSet.KeyAssetRuleGroup();
            keyAssetRuleGroup.keyAsset = keyAsset;
            keyAssetRuleGroup.displayRuleGroup = new DisplayRuleGroup();

            var index = namedDisplayDictionary.FindIndex(x => x.IDRS == ruleSet);
            if(index >= 0)
            {
                var namedDisplay = namedDisplayDictionary[index];
                for(int i = 0; i < namedDisplay.displayRules.Count; i++)
                {
                    DisplayRule rule = namedDisplay.displayRules[i];
                    rule.Parse();
                    rule.finishedRule.followerPrefab = displayPrefab;
                    keyAssetRuleGroup.displayRuleGroup.AddDisplayRule(rule.finishedRule);
                }
            }
            return keyAssetRuleGroup;
        }*/
    }
}