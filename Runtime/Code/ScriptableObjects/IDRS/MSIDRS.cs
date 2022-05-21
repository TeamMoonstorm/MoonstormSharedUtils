using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    [Obsolete("MSIDRS is obsolete, upgrade to NamedIDRS")]
    public class MSIDRS : ScriptableObject
    {
        [Serializable]
        public struct KeyAssetRuleGroup
        {
            public string keyAssetName;
            public List<ItemDisplayRule> rules;
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
        }

        public static List<NamedIDRS> instancesList = new List<NamedIDRS>();
        internal ItemDisplayRuleSet vanillaIDRS;

        [Space]
        public List<KeyAssetRuleGroup> MSUKeyAssetRuleGroup = new List<KeyAssetRuleGroup>();
        public string VanillaIDRSKey;
    }
}