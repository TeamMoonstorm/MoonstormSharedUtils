using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace Moonstorm.Components
{
    [Obsolete]
    [RequireComponent(typeof(CharacterModel))]
    public class MoonstormIDH : MonoBehaviour
    {
        private CharacterModel model;
        private ItemDisplayRuleSet modelIDRS;

        void Start()
        {
            model = GetComponent<CharacterModel>();
            modelIDRS = model.itemDisplayRuleSet;
            for (int i = 0; i < modelIDRS.runtimeEquipmentRuleGroups.Length; i++)
            {
                DisplayRuleGroup ruleGroup = modelIDRS.runtimeEquipmentRuleGroups[i];
                if (!ruleGroup.isEmpty)
                {
                    for (int j = 0; j < ruleGroup.rules.Length; j++)
                    {
                        ItemDisplayRule currentRule = ruleGroup.rules[j];
                        if (currentRule.childName == NamedIDRS.AddressNamedDisplayRule.NoValue)
                        {
                            ChildLocator childLocator = model.childLocator;
                            var firstChild = childLocator.transformPairs.FirstOrDefault().name;
                            currentRule.childName = firstChild;
                        }
                        ruleGroup.rules[j] = currentRule;
                    }
                }
                modelIDRS.runtimeEquipmentRuleGroups[i] = ruleGroup;
            }

            for (int i = 0; i < modelIDRS.runtimeItemRuleGroups.Length; i++)
            {
                DisplayRuleGroup ruleGroup = modelIDRS.runtimeItemRuleGroups[i];
                if (!ruleGroup.isEmpty)
                {
                    for (int j = 0; j < ruleGroup.rules.Length; j++)
                    {
                        ItemDisplayRule currentRule = ruleGroup.rules[j];
                        if (currentRule.childName == NamedIDRS.AddressNamedDisplayRule.NoValue)
                        {
                            ChildLocator childLocator = model.childLocator;
                            var firstChild = childLocator.transformPairs.FirstOrDefault().name;
                            currentRule.childName = firstChild;
                        }
                        ruleGroup.rules[j] = currentRule;
                    }
                }
                modelIDRS.runtimeItemRuleGroups[i] = ruleGroup;
            }
        }
    }
}