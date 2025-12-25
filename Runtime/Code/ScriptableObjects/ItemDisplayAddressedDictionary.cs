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

    /// <summary>
    /// An <see cref="ItemDisplayAddressedDictionary"/> is a custom <see cref="ScriptableObject"/> which is used for appending a single <see cref="IDRS.KeyAssetRuleGroup"/> to multiple different vanilla <see cref="ItemDisplayRuleSet"/>. It works in a similar fashion to R2API's ItemDisplayDictionary.
    /// <br></br>
    /// For example, you can utilize the <see cref="ItemDisplayAddressedDictionary"/> to add Elite displays to all vanilla survivors and monsters inside the editor.
    /// </summary>
    [CreateAssetMenu(fileName = "New ItemDisplayAddressedDictionary", menuName = "MSU/ItemDisplayAddressedDictionary")]
    public class ItemDisplayAddressedDictionary : ScriptableObject
    {
        #region SubTypes
        /// <summary>
        /// Represents a custom version of <see cref="ItemDisplayRule"/>.
        /// <br></br>
        /// The main difference between this and <see cref="ItemAddressedDisplayRule"/> is the utilization of R2API's <see cref="AddressReferencedPrefab"/> to reference the Display Prefab, an AddressReference is utilized mainly for adding Spikes to Mithrix, and for using <see cref="AddressableComponentRequirementAttribute"/> to reduce the addressable options
        /// </summary>
        [Serializable]
        public struct ItemAddressedDisplayRule
        {
            [Tooltip("The rule type for this rule.")]
            public ItemDisplayRuleType ruleType;
            [Tooltip("The display prefab to utilize for this ItemDisplay")]
            [AddressableComponentRequirement(typeof(ItemDisplay))]
            public AddressReferencedPrefab displayPrefab;
            [Tooltip("The child locator entry at which this prefab will be instantiated")]
            public string childName;
            [Tooltip("The local position of the prefab, relative to the child locator entry specified in childName")]
            public Vector3 localPos;
            [Tooltip("The local angles of the prefab, relative to the child locator entry specified in childName")]
            public Vector3 localAngles;
            [Tooltip("The local scale of the prefab, relative to the child locator entry specified in childName")]
            public Vector3 localScale;
            [Tooltip("A limb flag for this ItemDisplay to remove the limb. Paul's goat hoof uses this to hide the character's leg")]
            public LimbFlags limbMask;

            /// <summary>
            /// Returns the <see cref="ItemDisplayRule"/> stored within this <see cref="ItemAddressedDisplayRule"/>
            /// </summary>
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

        /// <summary>
        /// Represents an entry within the dictionary, where the key is <see cref="targetIDRS"/>, and the value is <see cref="rules"/>.
        /// <br></br>
        /// Basically, all the rules specified in <see cref="rules"/> will be added to the <see cref="IDRS"/> referenced in <see cref="targetIDRS"/>. The KeyAsset will be the one you specify in <see cref="keyAsset"/>
        /// </summary>
        [Serializable]
        public struct DisplayDictionaryEntry
        {
            [Tooltip("The vanilla IDRS we want to modify")]
            public AssetReferenceT<IDRS> targetIDRS;
            [Tooltip("The new ItemDisplayRules to add to targetIDRS")]
            public List<ItemAddressedDisplayRule> rules;

            /// <summary>
            /// Returns wether the <see cref="DisplayDictionaryEntry"/> is empty
            /// </summary>
            public bool isEmpty => rules != null ? rules.Count == 0 : true;

            /// <summary>
            /// Adds a new <see cref="ItemAddressedDisplayRule"/> to the <see cref="rules"/>
            /// </summary>
            /// <param name="rule">The new rule to add</param>
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

        [Tooltip("The KeyAsset that is going to be used for the new entries in each IDRS")]
        [TypeRestrictedReference(typeof(ItemDef), typeof(EquipmentDef))]
        public ScriptableObject keyAsset;

        [Space(10)]
        [Tooltip("The entries of this dictionary")]
        public DisplayDictionaryEntry[] displayEntries = Array.Empty<DisplayDictionaryEntry>();

        /// <summary>
        /// Adds all the entries stored within this <see cref="ItemDisplayAddressedDictionary"/> into the target IDRS
        /// </summary>
        /// <returns>A coroutine which needs to be processed to completion.</returns>
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
