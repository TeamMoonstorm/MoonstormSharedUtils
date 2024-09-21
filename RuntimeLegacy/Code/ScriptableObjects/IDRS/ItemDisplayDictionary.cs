using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm
{
    public class ItemDisplayDictionary : ScriptableObject
    {
        [Serializable]
        public struct NamedDisplayDictionary
        {
            public string idrsName;
            public List<DisplayRule> displayRules;

            public bool IsEmpty { get => displayRules != null ? displayRules.Count == 0 : true; }

            public void AddDisplayRule(DisplayRule rule)
            {
                throw new System.NotImplementedException();
            }
        }

        [Serializable]
        public struct DisplayRule
        {
            public ItemDisplayRuleType ruleType;

            public int displayPrefabIndex;

            public string childName;

            public Vector3 localPos;

            public Vector3 localAngles;

            public Vector3 localScales;

            public LimbFlags limbMask;

            [HideInInspector, NonSerialized]
            public ItemDisplayRule finishedRule;

            public const string NoValue = nameof(NoValue);

            internal void CreateRule(GameObject[] displayPrefabs)
            {
                throw new System.NotImplementedException();
            }
        }

        public static readonly List<ItemDisplayDictionary> instances = new List<ItemDisplayDictionary>();

        public UnityEngine.Object keyAsset;

        public GameObject[] displayPrefabs = Array.Empty<GameObject>();

        [Space]
        public List<NamedDisplayDictionary> namedDisplayDictionary = new List<NamedDisplayDictionary>();

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
        private ItemDisplayRuleSet.KeyAssetRuleGroup GetKeyAssetRuleGroup(string key)
        {
            throw new System.NotImplementedException();
        }
    }
}