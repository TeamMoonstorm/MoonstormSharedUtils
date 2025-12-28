using HG;
using RoR2;
using RoR2.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using static MSU.NamedItemDisplayRuleSet;
using IOPath = System.IO.Path;

namespace MSU.Editor.EditorWindows
{
    public class ItemDisplayMigrationWizard : EditorWizardWindow
    {
        public bool upgradeNamedItemDisplayRuleSet;
        public bool upgradeItemDisplayDictionary;

        public string objectFilter;
        public List<ScriptableObject> itemsToUpgrade = new();

        private WizardCoroutineHelper _wizardCoroutineHelper;
        private List<ScriptableObject> _validUpgradeCandidates = new List<ScriptableObject>();
        private ReadOnlyCollection<string> _availableIDRS;
        private ReadOnlyCollection<string> _availableKeyAssets;
        private ReadOnlyCollection<string> _availableDisplayPrefabs;

        Dictionary<string, AssetMatchContainer<ScriptableObject>> _keyAssetNameToAssetMatch = new Dictionary<string, AssetMatchContainer<ScriptableObject>>();
        Dictionary<string, AssetMatchContainer<GameObject>> _displayPrefabNameToAssetMatch = new Dictionary<string, AssetMatchContainer<GameObject>>();
        Dictionary<string, AssetMatchContainer<ItemDisplayRuleSet>> _idrsNameToAssetMatch = new Dictionary<string, AssetMatchContainer<ItemDisplayRuleSet>>();

        private StringBuilder _wizardLog = new StringBuilder();
        public static ItemDisplayMigrationWizard Open()
        {
            return Open<ItemDisplayMigrationWizard>();
        }

        protected override void SetupControls()
        {
            base.SetupControls();
            AddExtraFooterButtons();
        }

        private void AddExtraFooterButtons()
        {
            var runWizardButton = rootVisualElement.Q<Button>("RunWizard");
            runWizardButton.style.width = new StyleLength(StyleKeyword.Auto);

            var closeButton = rootVisualElement.Q<Button>("CloseWizardButton");
            closeButton.style.width = new StyleLength(StyleKeyword.Auto);

            var parent = runWizardButton.parent;

            var newButton = new Button(() => RefreshCandidates(additively: true))
            {
                text = "Refresh Candidates Additively"
            };
            newButton.style.flexGrow = 1;
            parent.Insert(parent.IndexOf(runWizardButton), newButton);

            newButton = new Button(() => RefreshCandidates(additively: false))
            {
                text = "Refresh Candidates"
            };
            newButton.style.flexGrow = 1;
            parent.Insert(parent.IndexOf(runWizardButton), newButton);
        }

        private void RefreshCandidates(bool additively)
        {
            using var _ = ListPool<ScriptableObject>.RentCollection(out var newCandidates);

            if (upgradeItemDisplayDictionary)
            {
                var itemDisplayDictionaryPaths = AssetDatabase.FindAssets("t:ItemDisplayDictionary")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .ToArray();

                foreach (var path in itemDisplayDictionaryPaths)
                {
                    if (path.Contains(objectFilter))
                    {
                        newCandidates.Add(AssetDatabase.LoadAssetAtPath<ItemDisplayDictionary>(path));
                    }
                }
            }

            if (upgradeNamedItemDisplayRuleSet)
            {
                var itemDisplayDictionaryPaths = AssetDatabase.FindAssets("t:NamedItemDisplayRuleSet")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .ToArray();

                foreach (var path in itemDisplayDictionaryPaths)
                {
                    if (path.Contains(objectFilter))
                    {
                        newCandidates.Add(AssetDatabase.LoadAssetAtPath<NamedItemDisplayRuleSet>(path));
                    }
                }
            }

            if (additively)
            {
                itemsToUpgrade.Union(newCandidates);
            }
            else
            {
                itemsToUpgrade.Clear();
                itemsToUpgrade.AddRange(newCandidates);
            }
        }

        protected override IEnumerator RunWizardCoroutine()
        {
            _wizardLog.Clear();
            _wizardCoroutineHelper = new WizardCoroutineHelper(this);

            if (upgradeItemDisplayDictionary)
            {
                _wizardCoroutineHelper.AddStep(FilterItemsToUpgrade<ItemDisplayDictionary>(), "Filtering ItemDisplayDictionaries");
                _wizardCoroutineHelper.AddStep(QueryItemDisplayRuleSets(), "Querying ItemDisplayRuleSets");
                _wizardCoroutineHelper.AddStep(UpgradeItemDisplayDictionaries(), "Upgrading ItemDisplayDictionaries");
            }

            if (upgradeNamedItemDisplayRuleSet)
            {
                _wizardCoroutineHelper.AddStep(FilterItemsToUpgrade<NamedItemDisplayRuleSet>(), "Filtering ItemDisplayDictionaries");
                _wizardCoroutineHelper.AddStep(QueryKeyAssets(), "Querying KeyAssets");
                _wizardCoroutineHelper.AddStep(QueryDisplayPrefabs(), "Querying Display Prefabs");
                _wizardCoroutineHelper.AddStep(UpgradeNamedItemDisplayRuleSets(), "Upgrading NamedItemDisplayRuleSet");
            }

            _wizardCoroutineHelper.AddStep(WriteLog(), "Writing Log");

            while (_wizardCoroutineHelper.MoveNext())
            {
                yield return null;
            }

            AssetDatabase.SaveAssets();
            yield break;
        }

        private IEnumerator FilterItemsToUpgrade<T>() where T : ScriptableObject
        {
            _validUpgradeCandidates.Clear();
            for (int i = 0; i < itemsToUpgrade.Count; i++)
            {
                yield return R2EKMath.Remap(i, 0, itemsToUpgrade.Count, 0, 1);

                ScriptableObject scriptableObject = itemsToUpgrade[i];
                if (scriptableObject is T t)
                {
                    _validUpgradeCandidates.Add(t);
                }
            }
        }
        #region NIDRS
        private IEnumerator QueryKeyAssets()
        {
            List<string> keyAssets = new List<string>();

            yield return 0;

            var lookup = new AddressablesPathDictionary.EntryLookup()
                .WithTypeRestriction(typeof(ItemDef), typeof(EquipmentDef))
                .WithLookupType(AddressablesPathDictionary.EntryType.Guid);
            var subroutine = lookup.PerformLookupAsync();
            while (subroutine.MoveNext())
            {
                yield return 0;
            }
            keyAssets.AddRange(lookup.results);

            yield return 0.33f;

            var equipmentGUIDS = AssetDatabase.FindAssets("t:EquipmentDef");
            for (int i = 0; i < equipmentGUIDS.Length; i++)
            {
                yield return R2EKMath.Remap(i, 0, equipmentGUIDS.Length, 0.33f, 0.66f);
                keyAssets.Add(AssetDatabase.GUIDToAssetPath(equipmentGUIDS[i]));
            }

            var itemGUIDS = AssetDatabase.FindAssets("t:ItemDef");
            for (int i = 0; i < itemGUIDS.Length; i++)
            {
                yield return R2EKMath.Remap(i, 0, itemGUIDS.Length, 0.33f, 0.66f);
                keyAssets.Add(AssetDatabase.GUIDToAssetPath(itemGUIDS[i]));
            }

            _availableKeyAssets = new ReadOnlyCollection<string>(keyAssets);
            _wizardLog.AppendLine($"Found {_availableKeyAssets.Count} Key Assets ({lookup.results.Count} Addressable, {_availableKeyAssets.Count - lookup.results.Count} as Assets)");
        }

        private IEnumerator QueryDisplayPrefabs()
        {
            List<string> result = new List<string>();

            yield return 0;

            var lookup = new AddressablesPathDictionary.EntryLookup()
                .WithTypeRestriction(typeof(GameObject))
                .WithComponentRequirement(typeof(ItemDisplay), false)
                .WithLookupType(AddressablesPathDictionary.EntryType.Guid);
            var subroutine = lookup.PerformLookupAsync();
            while (subroutine.MoveNext())
            {
                yield return 0;
            }
            result.AddRange(lookup.results);

            yield return 0.5f;

            var allGameObjects = AssetDatabaseUtil.FindAssetsByType<GameObject>().ToArray();
            for (int i = 0; i < allGameObjects.Length; i++)
            {
                yield return R2EKMath.Remap(i, 0, allGameObjects.Length, 0.5f, 1);

                GameObject gameObject = allGameObjects[i];
                if (gameObject.TryGetComponent<ItemDisplay>(out _))
                {
                    result.Add(AssetDatabase.GetAssetPath(gameObject));
                }
            }

            _availableDisplayPrefabs = new ReadOnlyCollection<string>(result);
            _wizardLog.AppendLine($"Found {_availableDisplayPrefabs.Count} Display Prefabs ({lookup.results.Count} Addressable, {_availableDisplayPrefabs.Count - lookup.results.Count} as Assets)");
        }

        private IEnumerator UpgradeNamedItemDisplayRuleSets()
        {
            for (int i = 0; i < _validUpgradeCandidates.Count; i++)
            {
                NamedItemDisplayRuleSet nidrs = (NamedItemDisplayRuleSet)_validUpgradeCandidates[i];
                _wizardLog.AppendLine();
                _wizardLog.AppendLine($"Upgrading {nidrs}");

                //First we'll get the target IDRS, very very likely this is empty.
                ItemDisplayRuleSet target = nidrs.targetItemDisplayRuleSet;
                var subroutine = FillDataFromNamedItemDisplayRuleSet(nidrs, target);
                while (subroutine.MoveNext())
                {
                    float subroutineProgress = (float)subroutine.Current;
                    var subroutineMaxProgress = Mathf.Min(i + 1, _validUpgradeCandidates.Count);
                    var val = R2EKMath.Remap(subroutineProgress, 0, 1, i, Mathf.Min(i + 1, subroutineMaxProgress));
                    yield return R2EKMath.Remap(val, 0, _validUpgradeCandidates.Count, 0, 1);
                }
                _wizardLog.AppendLine($"{nidrs} Upgraded succesfully.");
                if(nidrs.rules.Count <= 0)
                {
                    _wizardLog.AppendLine($"Destroying {nidrs} as it no longer contains any rules.");
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(nidrs));
                }
                else
                {
                    EditorUtil.SetDirty(nidrs);
                    AssetDatabase.SaveAssetIfDirty(nidrs);
                }

                EditorUtil.SetDirty(target);
                AssetDatabase.SaveAssetIfDirty(target);
                _wizardLog.AppendLine();
            }
        }

        private IEnumerator FillDataFromNamedItemDisplayRuleSet(NamedItemDisplayRuleSet from, ItemDisplayRuleSet to)
        {
            var displayPrefabNames = ListPool<string>.RentCollection();
            var upgradedRuleIndices = ListPool<int>.RentCollection();
            to.keyAssetRuleGroups = Array.Empty<ItemDisplayRuleSet.KeyAssetRuleGroup>();

            for (int i = 0; i < from.rules.Count; i++)
            {
                displayPrefabNames.Clear();

                float progress = R2EKMath.Remap(i, 0, from.rules.Count, 0, 1);
                yield return progress;

                var ruleGroup = from.rules[i];

                //We need to match the key asset by name AssetMatchContainer helps with that.
                var keyAssetSubroutine = new CoroutineWithResult<AssetMatchContainer<ScriptableObject>>(MatchAsset(_availableKeyAssets, ruleGroup.keyAssetName, _keyAssetNameToAssetMatch));
                while (keyAssetSubroutine.MoveNext())
                {
                    yield return progress;
                }

                //If there was no match, continue.
                AssetMatchContainer<ScriptableObject> scrobjMatchCointainer = keyAssetSubroutine.result;
                if (scrobjMatchCointainer.matchResult == MatchResult.NoMatch)
                {
                    _wizardLog.AppendLine($"WARN: No match for key asset with name {ruleGroup.keyAssetName} found. It was probably retrieved at runtime from the IDRS catalog. Not adding entry.");
                    continue;
                }

                //Found a match, time to set.
                ItemDisplayRuleSet.KeyAssetRuleGroup keyAssetRuleGroup = new ItemDisplayRuleSet.KeyAssetRuleGroup();

                //Set the key asset
                if (scrobjMatchCointainer.matchResult == MatchResult.AssetReference)
                {
                    keyAssetRuleGroup.keyAsset = scrobjMatchCointainer.assetMatch;
                }
                else if (scrobjMatchCointainer.matchResult == MatchResult.Guid)
                {
                    keyAssetRuleGroup.keyAssetAddress = new RoR2.AddressableAssets.IDRSKeyAssetReference(scrobjMatchCointainer.matchGUID);
                }

                //Now we need to match the display prefabs.
                foreach (var rule in ruleGroup.rules)
                {
                    displayPrefabNames.Add(rule.displayPrefabName);
                }
                var displayPrefabSubroutine = new CoroutineWithResult<AssetMatchContainer<GameObject>[]>(MatchAssets(_availableDisplayPrefabs, displayPrefabNames, _displayPrefabNameToAssetMatch));
                while (displayPrefabSubroutine.MoveNext())
                {
                    yield return progress;
                }

                var displayPrefabMatches = displayPrefabSubroutine.result;
                for (int matchIndex = 0; matchIndex < displayPrefabMatches.Length; matchIndex++)
                {
                    AssetMatchContainer<GameObject> prefabMatchContainer = displayPrefabMatches[matchIndex];
                    NamedItemDisplayRuleSet.DisplayRule associatedDisplayRule = ruleGroup.rules[matchIndex];

                    //Regardless of match, if the rule is a limb mask, then add it.
                    if (associatedDisplayRule.ruleType == ItemDisplayRuleType.LimbMask)
                    {
                        keyAssetRuleGroup.displayRuleGroup.AddDisplayRule(new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.LimbMask,
                            limbMask = associatedDisplayRule.limbMask
                        });
                        continue;
                    }

                    //No match? continue.
                    if (prefabMatchContainer.matchResult == MatchResult.NoMatch)
                    {
                        _wizardLog.AppendLine($"WARN: No match for DisplayPrefab with name {associatedDisplayRule.displayPrefabName} found. It was probably retrieved at runtime from the IDRS catalog. Not adding rule.");
                        continue;
                    }

                    ItemDisplayRule newRule = new ItemDisplayRule
                    {
                        localAngles = associatedDisplayRule.localAngles,
                        childName = associatedDisplayRule.childName,
                        localPos = associatedDisplayRule.localPos,
                        localScale = associatedDisplayRule.localScale,
                        ruleType = associatedDisplayRule.ruleType,
                    };

                    //set follower prefab depending on match result.
                    if (prefabMatchContainer.matchResult == MatchResult.AssetReference)
                    {
                        newRule.followerPrefab = prefabMatchContainer.assetMatch;
                    }
                    else
                    {
                        newRule.followerPrefabAddress = new(prefabMatchContainer.matchGUID);
                    }
                    keyAssetRuleGroup.displayRuleGroup.AddDisplayRule(newRule);
                }

                //Only add new rule group is display rule group is not empty.
                if (keyAssetRuleGroup.displayRuleGroup.isEmpty)
                {
                    _wizardLog.AppendLine($"WARN: Not adding KeyAssetRuleGroup created for key asset with name {ruleGroup.keyAssetName}, as it computed no ItemDisplayRules.");
                    continue;
                }

                upgradedRuleIndices.Add(i);
                HG.ArrayUtils.ArrayAppend(ref to.keyAssetRuleGroups, keyAssetRuleGroup);
            }

            //upgradedRuleIndices goes from smallest index to largest, iterate backwards.
            for(int i = upgradedRuleIndices.Count - 1; i >= 0; i--)
            {
                from.rules.RemoveAt(upgradedRuleIndices[i]);
            }
            ListPool<string>.ReturnCollection(displayPrefabNames);
            ListPool<int>.ReturnCollection(upgradedRuleIndices);
        }
        #endregion

        #region IDD
        private IEnumerator QueryItemDisplayRuleSets()
        {
            List<string> result = new List<string>();
            yield return 0;

            var lookup = new AddressablesPathDictionary.EntryLookup()
                .WithTypeRestriction(typeof(ItemDisplayRuleSet))
                .WithLookupType(AddressablesPathDictionary.EntryType.Guid);
            var subroutine = lookup.PerformLookupAsync();
            while (subroutine.MoveNext())
            {
                yield return 0;
            }
            result.AddRange(lookup.results);

            yield return 0.5f;

            var itemDisplayRuleSetGUIDS = AssetDatabase.FindAssets("t:ItemDisplayRuleSet");
            for (int i = 0; i < itemDisplayRuleSetGUIDS.Length; i++)
            {
                yield return R2EKMath.Remap(i, 0, itemDisplayRuleSetGUIDS.Length, 0.5f, 1f);
                result.Add(AssetDatabase.GUIDToAssetPath(itemDisplayRuleSetGUIDS[i]));
            }

            _availableIDRS = new ReadOnlyCollection<string>(result);
            _wizardLog.AppendLine($"Found {_availableIDRS.Count} Key Assets ({lookup.results.Count} Addressable, {_availableIDRS.Count - lookup.results.Count} as Assets)");
        }

        private IEnumerator UpgradeItemDisplayDictionaries()
        {
            for (int i = 0; i < _validUpgradeCandidates.Count; i++)
            {
                ItemDisplayDictionary idd = (ItemDisplayDictionary)_validUpgradeCandidates[i];
                _wizardLog.AppendLine();
                _wizardLog.AppendLine($"Upgrading {idd}");

                ItemDisplayAddressedDictionary result = CreateInstance<ItemDisplayAddressedDictionary>();
                result.keyAsset = idd.keyAsset;
                var subroutine = FillDataFromItemDisplayDictionary(idd, result);
                while (subroutine.MoveNext())
                {
                    float subroutineProgress = (float)subroutine.Current;
                    var subroutineMaxProgress = Mathf.Min(i + 1, _validUpgradeCandidates.Count);
                    var val = R2EKMath.Remap(subroutineProgress, 0, 1, i, Mathf.Min(i + 1, subroutineMaxProgress));
                    yield return R2EKMath.Remap(val, 0, _validUpgradeCandidates.Count, 0, 1);
                }

                //We've gotta seamlessly upgrade the asset, time to bust out that ScriptableObjectUpdater goodie.
                ScriptableObjectUpdater.UpgradeAsset(idd, result);
                _wizardLog.AppendLine($"{idd} Upgraded succesfully.");
                _wizardLog.AppendLine();
            }
        }

        public IEnumerator FillDataFromItemDisplayDictionary(ItemDisplayDictionary from, ItemDisplayAddressedDictionary to)
        {
            var upgradedRuleIndices = ListPool<int>.RentCollection();
            //Starstorm uses IDD for their own characters, fucking sucks but i'll have to fix that.
            var displayDictionaryEntries = from.displayDictionaryEntries;

            for (int i = 0; i < displayDictionaryEntries.Count; i++)
            {
                float progress = R2EKMath.Remap(i, 0, displayDictionaryEntries.Count, 0, 1);
                yield return progress;

                var displayDictionaryEntry = displayDictionaryEntries[i];

                //We need to find the main IDRS, so use AssetMatchContainer for that.
                var idrsSubroutine = new CoroutineWithResult<AssetMatchContainer<ItemDisplayRuleSet>>(MatchAsset(_availableIDRS, displayDictionaryEntry.idrsName, _idrsNameToAssetMatch));
                while (idrsSubroutine.MoveNext())
                {
                    yield return progress;
                }

                //No asset match? continue and log it.
                AssetMatchContainer<ItemDisplayRuleSet> idrsMatchContainer = idrsSubroutine.result;

                if (idrsMatchContainer.matchResult == MatchResult.NoMatch)
                {
                    _wizardLog.AppendLine($"WARN: No match for IDRS with name {displayDictionaryEntry.idrsName} found. It was probably retireved at runtime from the IDRS catalog. Not adding entry.");
                    continue;
                }

                //If for whatever reason this is an IDRS ASSET (not GUID), we need to add the display there.
                if (idrsMatchContainer.matchResult == MatchResult.AssetReference)
                {
                    _wizardLog.AppendLine($"INFO: Match for IDRS with name {displayDictionaryEntry.idrsName} found. It's an asset within the project, the entry will be added directly to the IDRS.");
                    AddItemDisplayDictionaryEntryToIDRS(from.keyAsset, from.displayPrefabs, displayDictionaryEntry.rules, idrsMatchContainer.assetMatch);
                    upgradedRuleIndices.Add(i);
                    continue;
                }

                //Found a match, time to begin the process.
                ItemDisplayAddressedDictionary.DisplayDictionaryEntry dictionaryEntry = new ItemDisplayAddressedDictionary.DisplayDictionaryEntry
                {
                    targetIDRS = new AssetReferenceT<ItemDisplayRuleSet>(idrsMatchContainer.matchGUID)
                };

                for(int j = 0; j < displayDictionaryEntry.rules.Count; j++)
                {
                    var displayRule = displayDictionaryEntry.rules[j];

                    if(displayRule.ruleType == ItemDisplayRuleType.LimbMask)
                    {
                        dictionaryEntry.AddDisplayRule(new ItemDisplayAddressedDictionary.ItemAddressedDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.LimbMask,
                            limbMask = displayRule.limbMask
                        });
                    }
                    var displayPrefab = HG.ArrayUtils.GetSafe(from.displayPrefabs, displayRule.displayPrefabIndex);
                    if(!displayPrefab)
                    {
                        _wizardLog.AppendLine($"WARN: Cannot append rule {j} in DisplayDictionaryEntry {i} in {from} because no GameObject exists at the index of {displayRule.displayPrefabIndex}");
                        continue;
                    }

                    dictionaryEntry.AddDisplayRule(new ItemDisplayAddressedDictionary.ItemAddressedDisplayRule
                    {
                        localAngles = displayRule.localAngles,
                        childName = displayRule.childName,
                        displayPrefab = new R2API.AddressReferencedAssets.AddressReferencedPrefab(displayPrefab),
                        limbMask = displayRule.limbMask,
                        localPos = displayRule.localPos,
                        localScale = displayRule.localScale,
                        ruleType = displayRule.ruleType,
                    });
                }

                upgradedRuleIndices.Add(i);
                HG.ArrayUtils.ArrayAppend(ref to.displayEntries, dictionaryEntry);
            }

            for (int i = upgradedRuleIndices.Count - 1; i >= 0; i--)
            {
                from.displayDictionaryEntries.RemoveAt(upgradedRuleIndices[i]);
            }
        }

        private void AddItemDisplayDictionaryEntryToIDRS(ScriptableObject keyAsset, GameObject[] displayPrefabs, List<ItemDisplayDictionary.DisplayRule> displayRules, ItemDisplayRuleSet targetIDRS)
        {
            ItemDisplayRuleSet.KeyAssetRuleGroup newKeyAssetRuleGroup = new ItemDisplayRuleSet.KeyAssetRuleGroup { keyAsset = keyAsset };
            for (int i = 0; i < displayRules.Count; i++)
            {
                ItemDisplayDictionary.DisplayRule displayRule = displayRules[i];
                //Setup for limbmask if specified.
                if (displayRule.ruleType == ItemDisplayRuleType.LimbMask)
                {
                    newKeyAssetRuleGroup.displayRuleGroup.AddDisplayRule(new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.LimbMask,
                        limbMask = displayRule.limbMask
                    });
                    continue;
                }

                GameObject displayPrefab = HG.ArrayUtils.GetSafe(displayPrefabs, displayRule.displayPrefabIndex);
                if(!displayPrefab)
                {
                    _wizardLog.AppendLine($"WARN: Cannot append rule {i} to {targetIDRS} because no GameObject exists at the index of {displayRule.displayPrefabIndex}.");
                    continue;
                }

                newKeyAssetRuleGroup.displayRuleGroup.AddDisplayRule(new ItemDisplayRule
                {
                    localAngles = displayRule.localAngles,
                    childName = displayRule.childName,
                    followerPrefab = displayPrefab,
                    localPos = displayRule.localPos,
                    localScale = displayRule.localScale,
                    ruleType = displayRule.ruleType
                });
            }

            HG.ArrayUtils.ArrayAppend(ref targetIDRS.keyAssetRuleGroups, newKeyAssetRuleGroup);
            _wizardLog.AppendLine($"Finished appending IDD values to {targetIDRS}");
        }
        #endregion

        #region AssetMatch utilization
        private static IEnumerator<AssetMatchContainer<T>[]> MatchAssets<T>(ICollection<string> potentialMatches, List<string> stringsToMatch, Dictionary<string, AssetMatchContainer<T>> cache) where T : UnityEngine.Object
        {
            AssetMatchContainer<T>[] results = new AssetMatchContainer<T>[stringsToMatch.Count];
            CoroutineWithResult<AssetMatchContainer<T>> coroutine = new CoroutineWithResult<AssetMatchContainer<T>>(null);
            for (int i = 0; i < stringsToMatch.Count; i++)
            {
                string toMatch = stringsToMatch[i];

                coroutine.StartNew(MatchAsset(potentialMatches, toMatch, cache));
                while (coroutine.MoveNext())
                {
                    yield return null;
                }

                results[i] = coroutine.result;
            }

            yield return results;
        }

        private static IEnumerator<AssetMatchContainer<T>> MatchAsset<T>(ICollection<string> potentialMatches, string toMatch, Dictionary<string, AssetMatchContainer<T>> cache) where T : UnityEngine.Object
        {
            if(cache.TryGetValue(toMatch, out AssetMatchContainer<T> result))
            {
                yield return result;
                yield break;
            }

            string guidMatch = "";

            T assetMatch = null;
            string assetPathMatch = "";

            foreach (var potentialMatch in potentialMatches)
            {
                //If this is a valid guid, the potential match is addressable, see if it's in the dictionary.
                if (GUID.TryParse(potentialMatch, out _) && AddressablesPathDictionary.instance.TryGetPathFromGUID(potentialMatch, out _))
                {
                    //Potential match is in the dictionary, load the asset
                    var subroutine = Addressables.LoadAssetAsync<T>(potentialMatch);
                    while (!subroutine.IsDone)
                    {
                        yield return default;
                    }

                    T asset = subroutine.Result;
                    if (asset.name.Equals(toMatch, StringComparison.OrdinalIgnoreCase))
                    {
                        guidMatch = potentialMatch;
                        break;
                    }
                } //if the if statement fails, it means its an asset path.
                else
                {
                    T asset = AssetDatabase.LoadAssetAtPath<T>(potentialMatch);
                    if (asset.name.Equals(toMatch, StringComparison.OrdinalIgnoreCase))
                    {
                        assetMatch = asset;
                        assetPathMatch = potentialMatch;
                        break;
                    }
                }
            }

            if (assetMatch)
            {
                result = new AssetMatchContainer<T>(assetMatch);
                cache.Add(toMatch, result);
                yield return result;
            }
            else if (!string.IsNullOrWhiteSpace(guidMatch))
            {
                result = new AssetMatchContainer<T>(guidMatch);
                cache.Add(toMatch, result);
                yield return result;
            }
            else
            {
                result = default;
                cache.Add(toMatch, result);
                yield return result;
            }
        }
        #endregion
        private IEnumerator WriteLog()
        {
            yield return 0;
            string path = "Assets/ItemDisplayMigrationWizard.log";
            File.WriteAllText(path, _wizardLog.ToString());
            yield return 1;
        }
        
        protected override bool ValidateUXMLPath(string path)
        {
            return path.ValidateUXMLPath();
        }
        private enum MatchResult
        {
            NoMatch,
            Guid,
            AssetReference
        }

        private struct AssetMatchContainer<T> where T : UnityEngine.Object
        {
            public MatchResult matchResult { get; }
            public string matchGUID { get; }
            public T assetMatch { get; }

            public AssetMatchContainer(string guid)
            {
                matchGUID = guid;
                matchResult = MatchResult.Guid;
                assetMatch = null;
            }

            public AssetMatchContainer(T asset)
            {
                assetMatch = asset;
                matchResult = MatchResult.AssetReference;
                matchGUID = null;
            }
        }
    }
}