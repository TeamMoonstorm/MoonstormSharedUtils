using HG.Coroutines;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using HGParallelCoroutine = HG.Coroutines.ParallelCoroutine;
using IDRS = RoR2.ItemDisplayRuleSet;

namespace MSU
{
    [CreateAssetMenu(fileName = "New ItemDisplayAddressedDictionary", menuName = "MSU/ItemDisplayAddressedDictionary")]
    public class ItemDisplayAddressedDictionary : ScriptableObject
    {
        #region SubTypes
        [Serializable]
        public struct ItemAddressedDisplayRule
        {
            public ItemDisplayRuleType ruleType;
            [AddressableComponentRequirement(typeof(ItemDisplay))]
            public AddressReferencedPrefab displayPrefab;
            public string childName;
            public Vector3 localPos;
            public Vector3 localAngles;
            public Vector3 localScale;
            public LimbFlags limbMask;

            public ItemDisplayRule ToItemDisplayRule()
            {
                GameObject displayPrefab = this.displayPrefab.LoadAssetNow();
                return new ItemDisplayRule
                {
                    ruleType = ruleType,
                    limbMask = limbMask,
                    followerPrefabAddress = new AssetReferenceGameObject(""),
                    followerPrefab = displayPrefab,
                    childName = childName,
                    localPos = localPos,
                    localAngles = localAngles,
                    localScale = localScale
                };
            }
        }
        [Serializable]
        public struct DisplayDictionaryEntry
        {
            public AssetReferenceT<IDRS> targetIDRS;
            public List<ItemAddressedDisplayRule> rules;

            public bool isEmpty => rules != null ? rules.Count == 0 : true;

            public void AddDisplayRule(ItemAddressedDisplayRule rule)
            {
                rules ??= new List<ItemAddressedDisplayRule>();
                rules.Add(rule);
            }

            internal IEnumerator AddToTargetIDRS(ScriptableObject keyAsset)
            {
                if (isEmpty)
                    yield break;

                if (!targetIDRS.RuntimeKeyIsValid())
                    yield break;

                var assetRequest = Addressables.LoadAssetAsync<ItemDisplayRuleSet>(targetIDRS.RuntimeKey);
                while(!assetRequest.IsDone)
                {
                    yield return null;
                }

                IDRS idrs = assetRequest.Result;

                try
                {
                    DisplayRuleGroup displayRuleGroup = new DisplayRuleGroup();
                    foreach (var rule in rules)
                    {
                        displayRuleGroup.AddDisplayRule(rule.ToItemDisplayRule());
                    }

                    idrs.SetDisplayRuleGroup(keyAsset, displayRuleGroup);
                }
                finally
                {
                    Addressables.Release(assetRequest);
                }
            }
        }

        #endregion
        [TypeRestrictedReference(typeof(ItemDef), typeof(EquipmentDef))]
        public ScriptableObject keyAsset;

        [Space(10)]
        public DisplayDictionaryEntry[] displayEntries = Array.Empty<DisplayDictionaryEntry>();

        public IEnumerator AddEntries()
        {
            yield return null;
            HGParallelCoroutine perEntryCoroutine = new HGParallelCoroutine();

            foreach(var entry in displayEntries)
            {
                perEntryCoroutine.Add(entry.AddToTargetIDRS(keyAsset));
            }

            while(perEntryCoroutine.MoveNext())
            {
                yield return null;
            }
        }
    }
}
