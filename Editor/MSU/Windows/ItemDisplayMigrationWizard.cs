using HG;
using RoR2;
using RoR2.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
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
        private List<ScriptableObject> _validUpgradeCandidates;
        private ReadOnlyCollection<string> _availableIDRS;
        private ReadOnlyCollection<string> _availableKeyAssets;
        private ReadOnlyCollection<string> _availableDisplayPrefabs;
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

            if(upgradeItemDisplayDictionary)
            {
                var itemDisplayDictionaryPaths = AssetDatabase.FindAssets("t:ItemDisplayDictionary")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .ToArray();

                foreach(var path in itemDisplayDictionaryPaths)
                {
                    if(path.Contains(objectFilter))
                    {
                        newCandidates.Add(AssetDatabase.LoadAssetAtPath<ItemDisplayDictionary>(path));
                    }
                }
            }

            if(upgradeNamedItemDisplayRuleSet)
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

            if(additively)
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
            _wizardCoroutineHelper = new WizardCoroutineHelper(this);

            if(upgradeItemDisplayDictionary)
            {
                _wizardCoroutineHelper.AddStep(FilterItemsToUpgrade<ItemDisplayDictionary>(), "Filtering ItemDisplayDictionaries");
                _wizardCoroutineHelper.AddStep(QueryItemDisplayRuleSets(), "Querying ItemDisplayRuleSets");
                _wizardCoroutineHelper.AddStep(UpgradeItemDisplayDictionaries(), "Upgrading ItemDisplayDictionaries");
            }

            if(upgradeNamedItemDisplayRuleSet)
            {
                _wizardCoroutineHelper.AddStep(FilterItemsToUpgrade<NamedItemDisplayRuleSet>(), "Filtering ItemDisplayDictionaries");
                _wizardCoroutineHelper.AddStep(QueryKeyAssets(), "Querying KeyAssets");
                _wizardCoroutineHelper.AddStep(QueryDisplayPrefabs(), "Querying Display Prefabs");
                _wizardCoroutineHelper.AddStep(UpgradeNamedItemDisplayRuleSets(), "Upgrading NamedItemDisplayRuleSet");
            }

            while(_wizardCoroutineHelper.MoveNext())
            {
                yield return null;
            }

            yield break;
        }

        private IEnumerator FilterItemsToUpgrade<T>() where T : ScriptableObject
        {
            _validUpgradeCandidates.Clear();
            for(int i = 0; i < itemsToUpgrade.Count; i++)
            {
                yield return R2EKMath.Remap(i, 0, itemsToUpgrade.Count, 0, 1);

                ScriptableObject scriptableObject = itemsToUpgrade[i];
                if(scriptableObject is T t)
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
            while(subroutine.MoveNext())
            {
                yield return 0;
            }
            keyAssets.AddRange(lookup.results);

            yield return 0.33f;

            var equipmentGUIDS = AssetDatabase.FindAssets("t:EquipmentDef");
            for(int i = 0; i < equipmentGUIDS.Length; i++)
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
            for(int i = 0; i < allGameObjects.Length; i++)
            {
                yield return R2EKMath.Remap(i, 0, allGameObjects.Length, 0.5f, 1);

                GameObject gameObject = allGameObjects[i];
                if(gameObject.TryGetComponent<ItemDisplay>(out _))
                {
                    result.Add(AssetDatabase.GetAssetPath(gameObject));
                }
            }

            _availableDisplayPrefabs = new ReadOnlyCollection<string>(result);
        }

        private IEnumerator UpgradeNamedItemDisplayRuleSets()
        {
            for(int i = 0; i < _validUpgradeCandidates.Count; i++)
            {
                NamedItemDisplayRuleSet nidrs = (NamedItemDisplayRuleSet)_validUpgradeCandidates[i];
                //First we'll get the target IDRS, very very likely this is empty.
                ItemDisplayRuleSet target = nidrs.targetItemDisplayRuleSet;
                var subroutine = FillDataFromNamedItemDisplayRuleSet(nidrs, target);
                while(subroutine.MoveNext())
                {
                    yield return R2EKMath.Remap((float)subroutine.Current, 0, 1, 0, 1 / ((_validUpgradeCandidates.Count) - i));
                }
            }
        }

        private IEnumerator FillDataFromNamedItemDisplayRuleSet(NamedItemDisplayRuleSet from, ItemDisplayRuleSet to)
        {
            var displayPrefabNames = ListPool<string>.RentCollection();

            to.keyAssetRuleGroups = Array.Empty<ItemDisplayRuleSet.KeyAssetRuleGroup>();

            for (int i = 0; i < from.rules.Count; i++)
            {
                displayPrefabNames.Clear();

                yield return R2EKMath.Remap(i, 0, from.rules.Count, 0, 1);

                var ruleGroup = from.rules[i];

                //We need to match the key asset by name AssetMatchContainer helps with that.
                var keyAssetSubroutine = new CoroutineWithResult<AssetMatchContainer<ScriptableObject>>(AssetMatchContainer<ScriptableObject>.MatchAsset(_availableKeyAssets, ruleGroup.keyAssetName));
                while (keyAssetSubroutine.MoveNext())
                {
                    yield return null;
                }

                //If there was no match, continue.
                AssetMatchContainer<ScriptableObject> scrobjMatchCointainer = keyAssetSubroutine.result;
                if (scrobjMatchCointainer.matchResult == MatchResult.NoMatch)
                {
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
                foreach(var rule in ruleGroup.rules)
                {
                    displayPrefabNames.Add(rule.displayPrefabName);
                }
                var displayPrefabSubroutine = new CoroutineWithResult<AssetMatchContainer<GameObject>[]>(AssetMatchContainer<GameObject>.MatchAssets(_availableDisplayPrefabs, displayPrefabNames));
                while(displayPrefabSubroutine.MoveNext())
                {
                    yield return null;
                }

                var displayPrefabMatches = displayPrefabSubroutine.result;
                for(int matchIndex = 0; matchIndex < displayPrefabMatches.Length; matchIndex++)
                {
                    AssetMatchContainer<GameObject> prefabMatchContainer = displayPrefabMatches[matchIndex];
                    NamedItemDisplayRuleSet.DisplayRule associatedDisplayRule = ruleGroup.rules[matchIndex];

                    //Regardless of match, if the rule is a limb mask, then add it.
                    if(associatedDisplayRule.ruleType == ItemDisplayRuleType.LimbMask)
                    {
                        keyAssetRuleGroup.displayRuleGroup.AddDisplayRule(new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.LimbMask,
                            limbMask = associatedDisplayRule.limbMask
                        });
                        continue;
                    }

                    //No match? continue.
                    if(prefabMatchContainer.matchResult == MatchResult.NoMatch)
                    {
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
                    if(prefabMatchContainer.matchResult == MatchResult.AssetReference)
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
                if(keyAssetRuleGroup.displayRuleGroup.isEmpty)
                {
                    continue;
                }

                HG.ArrayUtils.ArrayAppend(ref to.keyAssetRuleGroups, keyAssetRuleGroup);
            }
            ListPool<string>.ReturnCollection(displayPrefabNames);
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
        }

        private IEnumerator UpgradeItemDisplayDictionaries()
        {
            yield break;
        }
        #endregion

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
            public static IEnumerator<AssetMatchContainer<T>[]> MatchAssets(ReadOnlyCollection<string> potentialMatches, List<string> stringsToMatch)
            {
                AssetMatchContainer<T>[] results = new AssetMatchContainer<T>[stringsToMatch.Count];
                CoroutineWithResult<AssetMatchContainer<T>> coroutine = new CoroutineWithResult<AssetMatchContainer<T>>(null);
                for (int i = 0; i < stringsToMatch.Count; i++)
                {
                    string toMatch = stringsToMatch[i];

                    coroutine.StartNew(MatchAsset(potentialMatches, toMatch));
                    while(coroutine.MoveNext())
                    {
                        yield return null;
                    }

                    results[i] = coroutine.result;
                }

                yield return results;
            }

            public static IEnumerator<AssetMatchContainer<T>> MatchAsset(ReadOnlyCollection<string> potentialMatches, string toMatch)
            {
                string guidMatch = "";
                T assetMatch = null;

                for (int i = 0; i < potentialMatches.Count; i++)
                {
                    var potentialMatch = potentialMatches[i];

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
                        }
                    } //if the if statement fails, it means its an asset path.
                    else
                    {
                        T asset = AssetDatabase.LoadAssetAtPath<T>(potentialMatch);
                        if (asset.name.Equals(toMatch, StringComparison.OrdinalIgnoreCase))
                        {
                            assetMatch = asset;
                        }
                    }
                }

                if (assetMatch)
                {
                    yield return new AssetMatchContainer<T>(assetMatch);
                }
                else if (string.IsNullOrWhiteSpace(guidMatch))
                {
                    yield return new AssetMatchContainer<T>(guidMatch);
                }
                else
                {
                    yield return default;
                }
            }

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