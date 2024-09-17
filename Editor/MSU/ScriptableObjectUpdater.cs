using Moonstorm;
using MSU;
using MSU.Editor;
using R2API;
using RoR2.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using MSULog = MSU.MSULog;
using Path = System.IO.Path;

public static class ScriptableObjectUpdater
{
    private static FieldInfo _interactableCardProvider_PairsField = typeof(InteractableCardProvider).GetField("_serializedCardPairs", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo _monsterCardProvider_PairsField = typeof(MonsterCardProvider).GetField("_serializedCardPairs", BindingFlags.Instance | BindingFlags.NonPublic);


    [MenuItem(MSUConstants.MSU_MENU_ROOT + "Upgrade Scriptable Objects")]
    private static void UpdateMSUScriptableObjects()
    {
        if (!EditorUtility.DisplayDialog("Upgrade Scriptable Objects", "By accepting this menu, Unity will attempt to upgrade your Legacy MoonstormSharedUtils ScriptableObjects into their new, MSU.Runtime Scriptable Object version (An example is MSEliteDef -> ExtendedEliteDef). This utility does not upgrade Event related scriptable objects.\n\n" +
            "It is Heavily recommended to make a backup of your project before continuing!", "I've made a backup, go ahead!", "Cancel"))
        {
            return;
        }
        MSULog.Info("Upgrading Scriptable Objects...");

        Action[] upgrades = new Action[]
        {
            UpgradeMSInteractableDirectorCard,
            UpgradeMSMonsterDirectorCard,
            UpgradeMSEliteDef,
            UpgradeSerializableEliteTierDef,
            UpgradeItemDisplayDictionary,
            UpgradeNamedIDRS,
            UpgradeMSUnlockableDef,
            UpgradeVanillaSkinDefinition,
            UpgradeVanillaSkinDefinitionToVanillaSkinDef
        };

        for (int i = 0; i < upgrades.Length; i++)
        {
            var action = upgrades[i];
            try
            {
                action();
            }
            catch (Exception ex)
            {
                MSULog.Error($"Could not finish upgrade method \"{action.Method.Name}\". {ex}");
            }
        }
    }

    private static void UpgradeMSInteractableDirectorCard()
    {
        var allMSInteractableDirectorCards = AssetDatabaseUtil.FindAssetsByType<MSInteractableDirectorCard>().ToArray();

        if (allMSInteractableDirectorCards.Length == 0)
            return;

        for (int i = 0; i < allMSInteractableDirectorCards.Length; i++)
        {
            var msInteractableDirectorCard = allMSInteractableDirectorCards[i];
            try
            {
                MSU.InteractableCardProvider provider = ScriptableObject.CreateInstance<InteractableCardProvider>();
                bool isCustomCategory = msInteractableDirectorCard.interactableCategory == DirectorAPI.InteractableCategory.Custom;
                AddressableDirectorCard addressableDirectorCard = msInteractableDirectorCard.addressReferencedDirectorCard;

                List<InteractableCardProvider.StageInteractableCardPair> pairs = new List<InteractableCardProvider.StageInteractableCardPair>();

                //Best way to transform the data to the new format is by iterating thru this.
                foreach (DirectorAPI.Stage stageEnum in Enum.GetValues(typeof(DirectorAPI.Stage)))
                {
                    if (stageEnum == DirectorAPI.Stage.Custom && msInteractableDirectorCard.stages.HasFlag(stageEnum))
                    {
                        foreach (string customStage in msInteractableDirectorCard.customStages)
                        {
                            pairs.Add(new InteractableCardProvider.StageInteractableCardPair
                            {
                                stage = DirectorAPI.Stage.Custom,
                                customStageNames = new List<string>() { customStage },
                                interactableCategory = msInteractableDirectorCard.interactableCategory,
                                customCategoryName = isCustomCategory ? msInteractableDirectorCard.customCategory : string.Empty,
                                customCategoryWeight = isCustomCategory ? msInteractableDirectorCard.customCategoryWeight : 0,
                                card = new AddressableDirectorCard
                                {
                                    forbiddenUnlockableDef = addressableDirectorCard.forbiddenUnlockableDef,
                                    minimumStageCompletions = addressableDirectorCard.minimumStageCompletions,
                                    preventOverhead = addressableDirectorCard.preventOverhead,
                                    requiredUnlockableDef = addressableDirectorCard.requiredUnlockableDef,
                                    selectionWeight = addressableDirectorCard.selectionWeight,
                                    spawnCard = addressableDirectorCard.spawnCard,
                                    spawnDistance = addressableDirectorCard.spawnDistance
                                }
                            });
                        }
                        continue;
                    }

                    if (msInteractableDirectorCard.stages.HasFlag(stageEnum))
                    {
                        pairs.Add(new InteractableCardProvider.StageInteractableCardPair
                        {
                            stage = stageEnum,
                            customCategoryName = isCustomCategory ? msInteractableDirectorCard.customCategory : string.Empty,
                            customCategoryWeight = isCustomCategory ? msInteractableDirectorCard.customCategoryWeight : 0,
                            customStageNames = new List<string>(),
                            interactableCategory = msInteractableDirectorCard.interactableCategory,
                            card = new AddressableDirectorCard
                            {
                                forbiddenUnlockableDef = addressableDirectorCard.forbiddenUnlockableDef,
                                minimumStageCompletions = addressableDirectorCard.minimumStageCompletions,
                                preventOverhead = addressableDirectorCard.preventOverhead,
                                requiredUnlockableDef = addressableDirectorCard.requiredUnlockableDef,
                                selectionWeight = addressableDirectorCard.selectionWeight,
                                spawnCard = addressableDirectorCard.spawnCard,
                                spawnDistance = addressableDirectorCard.spawnDistance
                            }
                        });
                    }
                }

                _interactableCardProvider_PairsField.SetValue(provider, pairs.ToArray());

                UpgradeAsset(msInteractableDirectorCard, provider);
            }
            catch (Exception e)
            {
                MSULog.Error($"Could not upgrade {msInteractableDirectorCard}. {e}");
            }
        }
    }

    private static void UpgradeMSMonsterDirectorCard()
    {
        var allMsMonsterDirectorCards = AssetDatabaseUtil.FindAssetsByType<MSMonsterDirectorCard>().ToArray();

        if (allMsMonsterDirectorCards.Length == 0)
            return;

        for (int i = 0; i < allMsMonsterDirectorCards.Length; i++)
        {
            var msMonsterDirectorCard = allMsMonsterDirectorCards[i];
            try
            {
                MSU.MonsterCardProvider provider = ScriptableObject.CreateInstance<MonsterCardProvider>();
                bool isCustomCategory = msMonsterDirectorCard.monsterCategory == DirectorAPI.MonsterCategory.Custom;
                AddressableDirectorCard addressableDirectorCard = msMonsterDirectorCard.addressReferencedDirectorCard;

                List<MonsterCardProvider.StageMonsterCardPair> pairs = new List<MonsterCardProvider.StageMonsterCardPair>();

                //Best way to transform the data to the new format is by iterating thru this.
                foreach (DirectorAPI.Stage stageEnum in Enum.GetValues(typeof(DirectorAPI.Stage)))
                {
                    if (stageEnum == DirectorAPI.Stage.Custom && msMonsterDirectorCard.stages.HasFlag(stageEnum))
                    {
                        foreach (string customStage in msMonsterDirectorCard.customStages)
                        {
                            pairs.Add(new MonsterCardProvider.StageMonsterCardPair
                            {
                                stage = DirectorAPI.Stage.Custom,
                                customStageNames = new List<string>() { customStage },
                                monsterCategory = msMonsterDirectorCard.monsterCategory,
                                customCategoryName = isCustomCategory ? msMonsterDirectorCard.customCategory : string.Empty,
                                customCategoryWeight = isCustomCategory ? msMonsterDirectorCard.customCategoryWeight : 0,
                                card = new AddressableDirectorCard
                                {
                                    forbiddenUnlockableDef = addressableDirectorCard.forbiddenUnlockableDef,
                                    minimumStageCompletions = addressableDirectorCard.minimumStageCompletions,
                                    preventOverhead = addressableDirectorCard.preventOverhead,
                                    requiredUnlockableDef = addressableDirectorCard.requiredUnlockableDef,
                                    selectionWeight = addressableDirectorCard.selectionWeight,
                                    spawnCard = addressableDirectorCard.spawnCard,
                                    spawnDistance = addressableDirectorCard.spawnDistance
                                }
                            });
                        }
                        continue;
                    }

                    if (msMonsterDirectorCard.stages.HasFlag(stageEnum))
                    {
                        pairs.Add(new MonsterCardProvider.StageMonsterCardPair
                        {
                            stage = stageEnum,
                            customCategoryName = isCustomCategory ? msMonsterDirectorCard.customCategory : string.Empty,
                            customCategoryWeight = isCustomCategory ? msMonsterDirectorCard.customCategoryWeight : 0,
                            customStageNames = new List<string>(),
                            monsterCategory = msMonsterDirectorCard.monsterCategory,
                            card = new AddressableDirectorCard
                            {
                                forbiddenUnlockableDef = addressableDirectorCard.forbiddenUnlockableDef,
                                minimumStageCompletions = addressableDirectorCard.minimumStageCompletions,
                                preventOverhead = addressableDirectorCard.preventOverhead,
                                requiredUnlockableDef = addressableDirectorCard.requiredUnlockableDef,
                                selectionWeight = addressableDirectorCard.selectionWeight,
                                spawnCard = addressableDirectorCard.spawnCard,
                                spawnDistance = addressableDirectorCard.spawnDistance
                            }
                        });
                    }
                }

                _monsterCardProvider_PairsField.SetValue(provider, pairs.ToArray());

                UpgradeAsset(msMonsterDirectorCard, provider);
            }
            catch (Exception e)
            {
                MSULog.Error($"Could not upgrade {msMonsterDirectorCard}. {e}");
            }
        }
    }

    private static void UpgradeMSEliteDef()
    {
        var allMSEliteDefs = AssetDatabaseUtil.FindAssetsByType<MSEliteDef>().ToArray();

        if (allMSEliteDefs.Length == 0)
            return;

        for (int i = 0; i < allMSEliteDefs.Length; i++)
        {
            var msEliteDef = allMSEliteDefs[i];

            try
            {
                ExtendedEliteDef extendedEliteDef = ScriptableObject.CreateInstance<ExtendedEliteDef>();

                extendedEliteDef.color = msEliteDef.color;
                extendedEliteDef.damageBoostCoefficient = msEliteDef.damageBoostCoefficient;
                extendedEliteDef.effect = msEliteDef.effect;
                extendedEliteDef.eliteEquipmentDef = msEliteDef.eliteEquipmentDef;
                extendedEliteDef.eliteRamp = msEliteDef.eliteRamp;

                ExtendedEliteDef.VanillaTier tier = ExtendedEliteDef.VanillaTier.None;
                switch (msEliteDef.eliteTier)
                {
                    case VanillaEliteTier.HonorActive: tier = ExtendedEliteDef.VanillaTier.HonorActive; break;
                    case VanillaEliteTier.HonorDisabled: tier = ExtendedEliteDef.VanillaTier.HonorDisabled; break;
                    case VanillaEliteTier.Lunar: tier = ExtendedEliteDef.VanillaTier.Lunar; break;
                    case VanillaEliteTier.None: break;
                    case VanillaEliteTier.PostLoop: tier = ExtendedEliteDef.VanillaTier.PostLoop; break;
                }
                extendedEliteDef.eliteTier = tier;

                extendedEliteDef.healthBoostCoefficient = msEliteDef.healthBoostCoefficient;
                extendedEliteDef.modifierToken = msEliteDef.modifierToken;
                extendedEliteDef.overlayMaterial = msEliteDef.overlay;
                extendedEliteDef.shaderEliteRampIndex = msEliteDef.shaderEliteRampIndex;

                UpgradeAsset(msEliteDef, extendedEliteDef);
            }
            catch (Exception e)
            {
                MSULog.Error($"Could not upgrade {msEliteDef}. {e}");
            }
        }
    }

    private static void UpgradeSerializableEliteTierDef()
    {
        var allSerializableEliteTierDefs = AssetDatabaseUtil.FindAssetsByType<Moonstorm.SerializableEliteTierDef>().ToArray();

        if (allSerializableEliteTierDefs.Length == 0)
            return;

        for (int i = 0; i < allSerializableEliteTierDefs.Length; i++)
        {
            var old_SerializableEliteTierDef = allSerializableEliteTierDefs[i];

            try
            {
                MSU.SerializableEliteTierDef new_SerializableEliteTierDef = ScriptableObject.CreateInstance<MSU.SerializableEliteTierDef>();

                new_SerializableEliteTierDef.canBeSelectedWithoutAvailableEliteDef = old_SerializableEliteTierDef.canSelectWithoutAvailableEliteDef;
                new_SerializableEliteTierDef.costMultiplier = old_SerializableEliteTierDef.costMultiplier;

                new_SerializableEliteTierDef.elites = new R2API.AddressReferencedAssets.AddressReferencedEliteDef[old_SerializableEliteTierDef.elites.Length];
                for (int j = 0; j < old_SerializableEliteTierDef.elites.Length; i++)
                {
                    new_SerializableEliteTierDef.elites[j] = old_SerializableEliteTierDef.elites[j];
                }

                UpgradeAsset(old_SerializableEliteTierDef, new_SerializableEliteTierDef);
            }
            catch (Exception e)
            {
                MSULog.Error($"Could not upgrade {old_SerializableEliteTierDef}. {e}");
            }
        }
    }

    private static void UpgradeItemDisplayDictionary()
    {
        var allItemDisplayDictionaries = AssetDatabaseUtil.FindAssetsByType<Moonstorm.ItemDisplayDictionary>().ToArray();

        if (allItemDisplayDictionaries.Length == 0)
            return;

        for (int i = 0; i < allItemDisplayDictionaries.Length; i++)
        {
            var old_IDD = allItemDisplayDictionaries[i];

            try
            {
                MSU.ItemDisplayDictionary new_IDD = ScriptableObject.CreateInstance<MSU.ItemDisplayDictionary>();

                new_IDD.keyAsset = old_IDD.keyAsset ? (ScriptableObject)old_IDD.keyAsset : null;
                new_IDD.displayPrefabs = HG.ArrayUtils.Clone(old_IDD.displayPrefabs);

                var displayDictionaryEntries = new List<MSU.ItemDisplayDictionary.DisplayDictionaryEntry>();
                foreach (var entry in old_IDD.namedDisplayDictionary)
                {
                    var newDictionaryEntry = new MSU.ItemDisplayDictionary.DisplayDictionaryEntry { idrsName = entry.idrsName };

                    foreach (var subEntry in entry.displayRules)
                    {
                        newDictionaryEntry.AddDisplayRule(new MSU.ItemDisplayDictionary.DisplayRule
                        {
                            localAngles = subEntry.localAngles,
                            childName = subEntry.childName,
                            displayPrefabIndex = subEntry.displayPrefabIndex,
                            limbMask = subEntry.limbMask,
                            localPos = subEntry.localPos,
                            localScale = subEntry.localScales,
                            ruleType = subEntry.ruleType
                        });
                    }
                    displayDictionaryEntries.Add(newDictionaryEntry);
                }
                new_IDD.displayDictionaryEntries = displayDictionaryEntries;

                UpgradeAsset(old_IDD, new_IDD);
            }
            catch (Exception e)
            {
                MSULog.Error($"Could not upgrade {old_IDD.name}. {e}");
            }
        }
    }

    private static void UpgradeNamedIDRS()
    {
        var allNamedIDRS = AssetDatabaseUtil.FindAssetsByType<NamedIDRS>().ToArray();

        if (allNamedIDRS.Length == 0)
            return;

        for (int i = 0; i < allNamedIDRS.Length; i++)
        {
            var old_namedIDRS = allNamedIDRS[i];

            try
            {
                NamedItemDisplayRuleSet new_namedIDRS = ScriptableObject.CreateInstance<NamedItemDisplayRuleSet>();

                new_namedIDRS.targetItemDisplayRuleSet = old_namedIDRS.idrs ? old_namedIDRS.idrs : null;

                List<NamedItemDisplayRuleSet.RuleGroup> newRuleGroups = new List<NamedItemDisplayRuleSet.RuleGroup>();

                foreach (var entry in old_namedIDRS.namedRuleGroups)
                {
                    var newRuleGroupEntry = new NamedItemDisplayRuleSet.RuleGroup { keyAssetName = entry.keyAssetName };

                    foreach (var subEntry in entry.rules)
                    {
                        newRuleGroupEntry.AddRule(new NamedItemDisplayRuleSet.DisplayRule
                        {
                            localAngles = subEntry.localAngles,
                            childName = subEntry.childName,
                            displayPrefabName = subEntry.displayPrefabName,
                            limbMask = subEntry.limbMask,
                            localScale = subEntry.localScales,
                            localPos = subEntry.localPos,
                            ruleType = subEntry.ruleType
                        });
                    }

                    newRuleGroups.Add(newRuleGroupEntry);
                }
                new_namedIDRS.rules = newRuleGroups;

                UpgradeAsset(old_namedIDRS, new_namedIDRS);
            }
            catch (Exception e)
            {
                MSULog.Error($"Could not upgrade {old_namedIDRS.name}. {e}");
            }
        }
    }

    private static void UpgradeMSUnlockableDef()
    {
        var allMSUnlockableDef = AssetDatabaseUtil.FindAssetsByType<MSUnlockableDef>().ToArray();

        if (allMSUnlockableDef.Length == 0)
            return;

        for (int i = 0; i < allMSUnlockableDef.Length; i++)
        {
            var msUnlockableDef = allMSUnlockableDef[i];

            try
            {
                AchievableUnlockableDef achievableUnlockableDef = ScriptableObject.CreateInstance<AchievableUnlockableDef>();

                achievableUnlockableDef.achievementCondition = msUnlockableDef.achievementCondition;
                achievableUnlockableDef.achievementDescriptionToken = msUnlockableDef.achievementDescToken;
                achievableUnlockableDef.achievementIcon = msUnlockableDef.achievementIcon;
                achievableUnlockableDef.achievementNameToken = msUnlockableDef.achievementNameToken;
                achievableUnlockableDef.displayModelPrefab = msUnlockableDef.displayModelPrefab;
                achievableUnlockableDef.hidden = msUnlockableDef.hidden;
                achievableUnlockableDef.nameToken = msUnlockableDef.nameToken;
                achievableUnlockableDef.serverAchievementCondition = msUnlockableDef.serverTracked ? msUnlockableDef.baseServerAchievement : default;

                UpgradeAsset(msUnlockableDef, achievableUnlockableDef);
            }
            catch (Exception e)
            {
                MSULog.Error($"Could not upgrade {msUnlockableDef.cachedName}. {e}");
            }
        }
    }

    private static void UpgradeVanillaSkinDefinition()
    {
    }

    private static void UpgradeVanillaSkinDefinitionToVanillaSkinDef()
    {

    }

    private static void UpgradeAsset(UnityEngine.Object originalAsset, UnityEngine.Object newVersion)
    {
        var origPath = AssetDatabase.GetAssetPath(originalAsset);
        var directory = Path.GetDirectoryName(origPath);
        var newPath = IOUtils.FormatPathForUnity(Path.Combine(directory, originalAsset.name + "_upgraded.asset"));
        AssetDatabase.CreateAsset(newVersion, newPath);

        string allTextUpgraded = File.ReadAllText(Path.GetFullPath(newPath));
        File.WriteAllText(origPath, allTextUpgraded);
        AssetDatabase.DeleteAsset(newPath);
        UnityEngine.Object.DestroyImmediate(newVersion, true);
    }
}
