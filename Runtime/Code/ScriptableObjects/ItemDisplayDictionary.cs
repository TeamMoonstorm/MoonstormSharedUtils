using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// An <see cref="ItemDisplayDictionary"/> is used for appending a single <see cref="ItemDisplayRuleSet.KeyAssetRuleGroup"/> to multiple <see cref="ItemDisplayRuleSet"/>. It works in a similar fashion to R2API's ItemDisplayDictionary.
    /// <para>The IDRS that are modified are loaded via using their names and MSU's internal ItemDisplayCatalog.</para>
    /// </summary>
    [CreateAssetMenu(fileName = "New ItemDisplayDictionary", menuName = "MSU/IDRS/ItemDisplayDictionary")]
    public class ItemDisplayDictionary : ScriptableObject
    {
        private static readonly HashSet<ItemDisplayDictionary> _instances = new HashSet<ItemDisplayDictionary>();

        [Tooltip("The key asset that's used for the key asset rule groups, must be of type ItemDef or EquipmentDef")]
        public ScriptableObject keyAsset;
        [Tooltip("An array of valid item display prefabs for the Key Asset")]
        public GameObject[] displayPrefabs = Array.Empty<GameObject>();

        [Space]
        [Tooltip("The dictionary's entries.")]
        public List<DisplayDictionaryEntry> displayDictionaryEntries = new List<DisplayDictionaryEntry>();

        private void Awake() => _instances.Add(this);
        private void OnDestroy() => _instances.Add(this);

        [SystemInitializer]
        private static IEnumerator SystemInitializer()
        {
            yield return new WaitForEndOfFrame();
            ItemDisplayCatalog.catalogAvailability.CallWhenAvailable(() =>
            {
                MSULog.Info("Initializing ItemDdisplayDictionaries");
                HashSet<ItemDisplayRuleSet> modifiedIDRS = new HashSet<ItemDisplayRuleSet>();
                foreach (ItemDisplayDictionary dictionary in _instances)
                {
                    var keyAsset = dictionary.keyAsset;

                    EquipmentDef ed = null;
                    ItemDef id = null;

                    bool isItem = keyAsset is ItemDef;
                    bool isEquipment = keyAsset is EquipmentDef;

                    if (!isItem && !isEquipment)
                    {
                        MSULog.Warning($"Item display dictionary {dictionary} has an invalid key asset, a key asset must be either an ItemDef or EquipmentDef.");
                        continue;
                    }

                    id = isItem ? (ItemDef)keyAsset : null;
                    ed = isEquipment ? (EquipmentDef)keyAsset : null;

                    if (id && id.itemIndex == ItemIndex.None)
                    {
#if DEBUG
                        MSULog.Debug($"Not appending valuees from {dictionary}, as its ItemDef's index is none.");
#endif
                        continue;
                    }
                    if (ed && ed.equipmentIndex == EquipmentIndex.None)
                    {
#if DEBUG
                        MSULog.Debug($"Not appending valuees from {dictionary}, as its ItemDef's index is none.");
#endif
                        continue;
                    }

                    for (int i = 0; i < dictionary.displayDictionaryEntries.Count; i++)
                    {
                        try
                        {
                            var entry = dictionary.displayDictionaryEntries[i];
                            ItemDisplayRuleSet target = ItemDisplayCatalog.GetItemDisplayRuleSet(entry.idrsName);
                            if (!target)
                            {
#if DEBUG
                                MSULog.Warning($"Not appending values of {dictionary}'s {i} index, as the target idrs is null.");
#endif
                                continue;
                            }
                            var keyAssetRuleGroup = dictionary.GetKeyAssetRuleGroup(entry.idrsName);
                            HG.ArrayUtils.ArrayAppend(ref target.keyAssetRuleGroups, keyAssetRuleGroup);
                            modifiedIDRS.Add(target);
#if DEBUG
                            MSULog.Debug($"Finished appending values from {dictionary}'s {i} entry into {target}");
#endif
                        }
                        catch (Exception e)
                        {
                            MSULog.Error($"{e}\n({dictionary} index {i}");
                        }
                    }
#if DEBUG
                    MSULog.Debug($"Finished appending values of {dictionary}");
#endif
                }

                foreach (var idrs in modifiedIDRS)
                {
                    idrs.GenerateRuntimeValues();
#if DEBUG
                    MSULog.Debug($"Regenerated runtime values for {idrs}");
#endif
                }
            });
        }

        private ItemDisplayRuleSet.KeyAssetRuleGroup GetKeyAssetRuleGroup(string key)
        {
            var keyAssetRuleGroup = new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = keyAsset,
                displayRuleGroup = new DisplayRuleGroup()
            };

            var index = displayDictionaryEntries.FindIndex(x => x.idrsName == key);
            if (index >= 0)
            {
                var entry = displayDictionaryEntries[index];
                if (entry.isEmpty)
                    return keyAssetRuleGroup;

                for (int i = 0; i < entry.rules.Count; i++)
                {
                    DisplayRule rule = entry.rules[i];
                    rule.CreateRule(displayPrefabs);
                    var finishedRule = rule.finishedRule;
                    keyAssetRuleGroup.displayRuleGroup.AddDisplayRule(finishedRule);
                }
            }
            return keyAssetRuleGroup;
        }

        /// <summary>
        /// Represents an entry for a <see cref="ItemDisplayDictionary"/>
        /// <para>Contains a string that'll be used to load a specific IDRS, and a list of display rules.</para>
        /// </summary>
        [Serializable]
        public struct DisplayDictionaryEntry
        {
            [Tooltip("The name of the IDRS to load")]
            public string idrsName;
            [Tooltip("The rules for this dictionary entry")]
            public List<DisplayRule> rules;

            /// <summary>
            /// Checks if this dictionary entry is empty or not.
            /// </summary>
            public bool isEmpty => rules != null ? rules.Count == 0 : true;

            /// <summary>
            /// Adds a new DisplayRule to this DictionaryEntry
            /// </summary>
            /// <param name="rule"></param>
            public void AddDisplayRule(DisplayRule rule)
            {
                rules = rules ?? new List<DisplayRule>();

                rules.Add(rule);
            }
        }

        /// <summary>
        /// Represents a DisplayRule for an entry inside a <see cref="DisplayDictionaryEntry"/>.
        /// <para>Contains sufficient metadata for transforming into a <see cref="RoR2.ItemDisplayRule"/> at runtime.</para>
        /// </summary>
        [Serializable]
        public struct DisplayRule
        {
            [Tooltip("The type of rule this display uses")]
            public ItemDisplayRuleType ruleType;
            [Tooltip("The index to use to load the Display Prefab from the \"Display Prefabs\" array")]
            public int displayPrefabIndex;
            [Tooltip("The name of a ChildLocator entry to instantiate the display prefab")]
            public string childName;
            [Tooltip("The display prefab's position relative to it's parent.")]
            public Vector3 localPos;
            [Tooltip("The display prefab's rotation relative to it's parent.")]
            public Vector3 localAngles;
            [Tooltip("The display prefab's scale relative to it's parent.")]
            public Vector3 localScale;
            [Tooltip("A mask of limbs to occlude when this display rule becomes active.")]
            public LimbFlags limbMask;

            /// <summary>
            /// Represents a display rule which has it's <see cref="childName"/> set to "NoValue"
            /// <br>This in turn will setup the display rule to use the first entry of the model's child locator, which then can be modified to be placed correctly using the ItemDisplayPlacementHelper</br>
            /// </summary>
            public const string NO_VALUE = "NoValue";

            /// <summary>
            /// Contains the finished rule from this DisplayRule
            /// </summary>
            public ItemDisplayRule finishedRule { get; private set; }

            internal void CreateRule(GameObject[] displayPrefabs)
            {
                if (string.IsNullOrWhiteSpace(childName))
                {
                    finishedRule = new ItemDisplayRule
                    {
                        childName = NO_VALUE,
                        followerPrefab = displayPrefabs[displayPrefabIndex],
                        localAngles = localAngles,
                        localPos = localPos,
                        localScale = localScale,
                        ruleType = ruleType,
                        limbMask = limbMask
                    };
                    return;
                }

                finishedRule = new ItemDisplayRule
                {
                    childName = childName,
                    followerPrefab = displayPrefabs[displayPrefabIndex],
                    localAngles = localAngles,
                    localPos = localPos,
                    localScale = localScale,
                    ruleType = ruleType,
                    limbMask = limbMask
                };
            }
        }
    }
}