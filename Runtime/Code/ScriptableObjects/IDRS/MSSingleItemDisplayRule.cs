using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    [Obsolete("MSSingleItemDisplayRule is obsolete, update to ItemDisplayDictionary")]
    public class MSSingleItemDisplayRule : ScriptableObject
    {
        [Serializable]
        public struct SingleKeyAssetRuleGroup
        {
            public string vanillaIDRSKey;

            public List<SingleItemDisplayRule> itemDisplayRules;

            internal ItemDisplayRuleSet vanillaIDRS;
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
        }

        public string keyAssetName;
        public string displayPrefabName;

        [Space]
        public List<SingleKeyAssetRuleGroup> singleItemDisplayRules = new List<SingleKeyAssetRuleGroup>();
    }
}
