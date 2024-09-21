using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm
{
    public class NamedIDRS : ScriptableObject
    {
        [Serializable]
        public struct AddressNamedRuleGroup
        {
            public string keyAssetName;

            public List<AddressNamedDisplayRule> rules;

            public bool IsEmpty { get => rules != null ? rules.Count == 0 : true; }

            public void AddRule(AddressNamedDisplayRule rule)
            {
                throw new System.NotImplementedException();
            }
        }

        [Serializable]
        public struct AddressNamedDisplayRule
        {
            public ItemDisplayRuleType ruleType;

            public string displayPrefabName;

            public string childName;

            public Vector3 localPos;

            public Vector3 localAngles;

            public Vector3 localScales;

            public LimbFlags limbMask;

            [HideInInspector, NonSerialized]
            public ItemDisplayRule finishedRule;

            public const string NoValue = nameof(NoValue);

            internal void CreateRule()
            {
                throw new System.NotImplementedException();
            }
        }

        public static readonly List<NamedIDRS> instances = new List<NamedIDRS>();

        public ItemDisplayRuleSet idrs;

        [Space]
        public List<AddressNamedRuleGroup> namedRuleGroups = new List<AddressNamedRuleGroup>();

        private void Awake()
        {
            throw new System.NotImplementedException();
        }
        private void OnDestroy()
        {
            throw new System.NotImplementedException();
        }

        private static void SystemInitializer()
        {
            throw new System.NotImplementedException();
        }

        internal ItemDisplayRuleSet.KeyAssetRuleGroup[] GetKeyAssetRuleGroups()
        {
            throw new System.NotImplementedException();
        }
    }
}
