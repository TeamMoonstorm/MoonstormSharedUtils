#if DEBUG
using RoR2;
using System.Linq;
using UnityEngine;

namespace MSU
{
    internal class ItemDisplayHelper : MonoBehaviour
    {
        private CharacterModel _model;
        private ItemDisplayRuleSet _idrs;

        private void Awake()
        {
            _model = GetComponent<CharacterModel>();
        }

        private void Start()
        {
            _idrs = _model.itemDisplayRuleSet;
            for (int i = 0; i < _idrs.runtimeEquipmentRuleGroups.Length; i++)
            {
                DisplayRuleGroup ruleGroup = _idrs.runtimeEquipmentRuleGroups[i];
                ref DisplayRuleGroup refRuleGroup = ref ruleGroup;
                if (ruleGroup.isEmpty)
                    continue;

                if (CheckForNoValueRules(ref refRuleGroup))
                {
                    _idrs.runtimeEquipmentRuleGroups[i] = refRuleGroup;
                }

            }

            for (int i = 0; i < _idrs.runtimeItemRuleGroups.Length; i++)
            {
                DisplayRuleGroup ruleGroup = _idrs.runtimeItemRuleGroups[i];
                ref DisplayRuleGroup refRuleGroup = ref ruleGroup;
                if (ruleGroup.isEmpty)
                    continue;

                if (CheckForNoValueRules(ref refRuleGroup))
                {
                    _idrs.runtimeItemRuleGroups[i] = refRuleGroup;
                }
            }
        }

        private bool CheckForNoValueRules(ref DisplayRuleGroup ruleGroup)
        {
            bool anyChanges = false;
            for (int i = 0; i < ruleGroup.rules.Length; i++)
            {
                ItemDisplayRule currentRule = ruleGroup.rules[i];
                if (currentRule.childName == NamedItemDisplayRuleSet.DisplayRule.NO_VALUE)
                {
                    ChildLocator childLocator = _model.childLocator;
                    var firstChild = childLocator.transformPairs.FirstOrDefault();
                    currentRule.childName = firstChild.name;
                    anyChanges = true;
                }
                ruleGroup.rules[i] = currentRule;
            }
            return anyChanges;
        }
    }
}
#endif