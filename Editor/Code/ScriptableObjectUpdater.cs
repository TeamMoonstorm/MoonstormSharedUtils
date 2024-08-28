using Moonstorm;
using MSU;
using RoR2.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ThunderKit.Common.Logging;
using UnityEditor;
using UnityEngine;
using R2API;
using System.Reflection;
using System.IO;
using RoR2;
using MSULog = MSU.MSULog;
using Path = System.IO.Path;
using UnityEditor.VersionControl;
using UnityEngine.Profiling;
using MSU.AddressReferencedAssets;

public static class ScriptableObjectUpdater
{
    private static FieldInfo InteractableCardProvider_PairsField = typeof(InteractableCardProvider).GetField("_serializedCardPairs", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo MonsterCardProvider_PairsField = typeof(MonsterCardProvider).GetField("_serializedCardPairs", BindingFlags.Instance | BindingFlags.NonPublic);

    [MenuItem("Tools/Penis")]
    private static void Penis()
    {
        var materials = AssetDatabaseUtils.FindAssetsByType<Material>();

        foreach(var material in materials)
        {
            if(material.shader.name.Contains("Addressable"))
            {
                MSULog.Info($"{material.name} using {material.shader.name}. (Path={AssetDatabase.GetAssetPath(material)}");
            }
        }
    }
    [MenuItem("Tools/MSEU/Upgrade Scriptable Objects")]
    private static void UpdateMSUScriptableObjects()
    {
        if (!EditorUtility.DisplayDialog("Upgrade Scriptable Objects", "By accepting this menu, Unity will attempt to upgrade your Legacy MoonstormSharedUtils ScriptableObjects into their new, MSU.Runtime Scriptable Object version (An example is MSEliteDef -> ExtendedEliteDef). This utility does not upgrade Event related scriptable objects.\n\n" +
            "It is Heavily recommended to make a backup of your project before continuing!", "I've made a backup, go ahead!", "Cancel"))
        {
            return;
        }
        MSULog.Info("Upgrading Scriptable Objects...");

        Action<ProgressBar>[] upgrades = new Action<ProgressBar>[]
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

        using (var progressBar = new ProgressBar("Upgrading Scriptable Objects"))
        {
            for (int i = 0; i < upgrades.Length; i++)
            {
                var action = upgrades[i];
                var actionName = action.Method.Name;
                try
                {
                    action(progressBar);
                }
                catch (Exception e)
                {
                    MSULog.Error($"Could not finish upgrade method \"{actionName}\". {e}");
                }
            }
        }
    }

    private static void UpgradeMSInteractableDirectorCard(ProgressBar progressBar)
    {
        var allMSInteractableDirectorCards = AssetDatabaseUtils.FindAssetsByType<MSInteractableDirectorCard>().ToArray();

        if (allMSInteractableDirectorCards.Length == 0)
            return;

        for (int i = 0; i < allMSInteractableDirectorCards.Length; i++)
        {
            var msInteractableDirectorCard = allMSInteractableDirectorCards[i];
            UpdateProgressBar(progressBar, $"Upgrading {msInteractableDirectorCard.name}", Util.Remap(i, 0, allMSInteractableDirectorCards.Length, 0, 0.11f));
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
                                stageEnum = DirectorAPI.Stage.Custom,
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
                            stageEnum = stageEnum,
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

                InteractableCardProvider_PairsField.SetValue(provider, pairs.ToArray());

                UpgradeAsset(msInteractableDirectorCard, provider);
            }
            catch (Exception e)
            {
                MSULog.Error($"Could not upgrade {msInteractableDirectorCard}. {e}");
            }
        }
    }

    private static void UpgradeMSMonsterDirectorCard(ProgressBar progressBar)
    {
        var allMsMonsterDirectorCards = AssetDatabaseUtils.FindAssetsByType<MSMonsterDirectorCard>().ToArray();

        if (allMsMonsterDirectorCards.Length == 0)
            return;

        for (int i = 0; i < allMsMonsterDirectorCards.Length; i++)
        {
            var msMonsterDirectorCard = allMsMonsterDirectorCards[i];
            UpdateProgressBar(progressBar, $"Upgrading {msMonsterDirectorCard.name}", Util.Remap(i, 0, allMsMonsterDirectorCards.Length, 0.11f, 0.22f));
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
                                stageEnum = DirectorAPI.Stage.Custom,
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
                            stageEnum = stageEnum,
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

                MonsterCardProvider_PairsField.SetValue(provider, pairs.ToArray());

                UpgradeAsset(msMonsterDirectorCard, provider);
            }
            catch (Exception e)
            {
                MSULog.Error($"Could not upgrade {msMonsterDirectorCard}. {e}");
            }
        }
    }

    private static void UpgradeMSEliteDef(ProgressBar progressBar)
    {
        var allMSEliteDefs = AssetDatabaseUtils.FindAssetsByType<MSEliteDef>().ToArray();

        if (allMSEliteDefs.Length == 0)
            return;

        for(int i = 0; i < allMSEliteDefs.Length; i++)
        {
            var msEliteDef = allMSEliteDefs[i];
            UpdateProgressBar(progressBar, $"Upgrading {msEliteDef.name}", Util.Remap(i, 0, allMSEliteDefs.Length, 0.22f, 0.33f));

            try
            {
                ExtendedEliteDef extendedEliteDef = ScriptableObject.CreateInstance<ExtendedEliteDef>();

                extendedEliteDef.color = msEliteDef.color;
                extendedEliteDef.damageBoostCoefficient = msEliteDef.damageBoostCoefficient;
                extendedEliteDef.effect = msEliteDef.effect;
                extendedEliteDef.eliteEquipmentDef = msEliteDef.eliteEquipmentDef;
                extendedEliteDef.eliteRamp = msEliteDef.eliteRamp;

                ExtendedEliteDef.VanillaTier tier = ExtendedEliteDef.VanillaTier.None;
                switch(msEliteDef.eliteTier)
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
            catch(Exception e)
            {
                MSULog.Error($"Could not upgrade {msEliteDef}. {e}");
            }
        }
    }

    private static void UpgradeSerializableEliteTierDef(ProgressBar progressBar)
    {
        var allSerializableEliteTierDefs = AssetDatabaseUtils.FindAssetsByType<Moonstorm.SerializableEliteTierDef>().ToArray();

        if (allSerializableEliteTierDefs.Length == 0)
            return;

        for (int i = 0; i < allSerializableEliteTierDefs.Length; i++)
        {
            var old_SerializableEliteTierDef = allSerializableEliteTierDefs[i];
            UpdateProgressBar(progressBar, $"Upgrading {old_SerializableEliteTierDef.name}", Util.Remap(i, 0, allSerializableEliteTierDefs.Length, 0.33f, 0.44f));

            try
            {
                MSU.SerializableEliteTierDef new_SerializableEliteTierDef = ScriptableObject.CreateInstance<MSU.SerializableEliteTierDef>();

                new_SerializableEliteTierDef.canBeSelectedWithoutAvailableEliteDef = old_SerializableEliteTierDef.canSelectWithoutAvailableEliteDef;
                new_SerializableEliteTierDef.costMultiplier = old_SerializableEliteTierDef.costMultiplier;

                new_SerializableEliteTierDef.elites = new R2API.AddressReferencedAssets.AddressReferencedEliteDef[old_SerializableEliteTierDef.elites.Length];
                for(int j = 0; j < old_SerializableEliteTierDef.elites.Length; i++)
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

    private static void UpgradeItemDisplayDictionary(ProgressBar progressBar)
    {
        var allItemDisplayDictionaries = AssetDatabaseUtils.FindAssetsByType<Moonstorm.ItemDisplayDictionary>().ToArray();

        if (allItemDisplayDictionaries.Length == 0)
            return;

        for(int i = 0; i < allItemDisplayDictionaries.Length; i++)
        {
            var old_IDD = allItemDisplayDictionaries[i];
            UpdateProgressBar(progressBar, $"Upgrading {old_IDD.name}", Util.Remap(i, 0, allItemDisplayDictionaries.Length, 0.44f, 0.55f));

            try
            {
                MSU.ItemDisplayDictionary new_IDD = ScriptableObject.CreateInstance<MSU.ItemDisplayDictionary>();

                new_IDD.keyAsset = old_IDD.keyAsset ? (ScriptableObject)old_IDD.keyAsset : null;
                new_IDD.displayPrefabs = HG.ArrayUtils.Clone(old_IDD.displayPrefabs);

                var displayDictionaryEntries = new List<MSU.ItemDisplayDictionary.DisplayDictionaryEntry>();
                foreach (var entry in old_IDD.namedDisplayDictionary)
                {
                    var newDictionaryEntry = new MSU.ItemDisplayDictionary.DisplayDictionaryEntry { idrsName = entry.idrsName };

                    foreach(var subEntry in entry.displayRules)
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
            catch(Exception e)
            {
                MSULog.Error($"Could not upgrade {old_IDD.name}. {e}");
            }
        }
    }

    private static void UpgradeNamedIDRS(ProgressBar progressBar)
    {
        var allNamedIDRS = AssetDatabaseUtils.FindAssetsByType<NamedIDRS>().ToArray();

        if (allNamedIDRS.Length == 0)
            return;

        for(int i = 0; i < allNamedIDRS.Length; i++)
        {
            var old_namedIDRS = allNamedIDRS[i];
            UpdateProgressBar(progressBar, $"Upgrading {old_namedIDRS.name}", Util.Remap(i, 0, allNamedIDRS.Length, 0.55f, 0.66f));

            try
            {
                NamedItemDisplayRuleSet new_namedIDRS = ScriptableObject.CreateInstance<NamedItemDisplayRuleSet>();

                new_namedIDRS.targetItemDisplayRuleSet = old_namedIDRS.idrs ? old_namedIDRS.idrs : null;

                List<NamedItemDisplayRuleSet.RuleGroup> newRuleGroups = new List<NamedItemDisplayRuleSet.RuleGroup>();

                foreach(var entry in old_namedIDRS.namedRuleGroups)
                {
                    var newRuleGroupEntry = new NamedItemDisplayRuleSet.RuleGroup { keyAssetName = entry.keyAssetName };

                    foreach(var subEntry in entry.rules)
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
            catch(Exception e)
            {
                MSULog.Error($"Could not upgrade {old_namedIDRS.name}. {e}");
            }
        }
    }

    private static void UpgradeMSUnlockableDef(ProgressBar progressBar)
    {
        var allMSUnlockableDef = AssetDatabaseUtils.FindAssetsByType<MSUnlockableDef>().ToArray();

        if (allMSUnlockableDef.Length == 0)
            return;

        for(int i = 0; i < allMSUnlockableDef.Length; i++)
        {
            var msUnlockableDef = allMSUnlockableDef[i];
            UpdateProgressBar(progressBar, $"Upgrading {msUnlockableDef.cachedName}", Util.Remap(i, 0, allMSUnlockableDef.Length, 0.66f, 0.77f));

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
            catch(Exception e)
            {
                MSULog.Error($"Could not upgrade {msUnlockableDef.cachedName}. {e}");
            }
        }
    }

    private static void UpgradeVanillaSkinDefinition(ProgressBar progressBar)
    {
        var allVanillaSkinDefinitions = AssetDatabaseUtils.FindAssetsByType<Moonstorm.VanillaSkinDefinition>().ToArray();

        if (allVanillaSkinDefinitions.Length == 0)
            return;

        for(int i = 0; i < allVanillaSkinDefinitions.Length; i++)
        {
            var old_skinDef = allVanillaSkinDefinitions[i];
            UpdateProgressBar(progressBar, $"Upgrading {old_skinDef.name}", Util.Remap(i, 0, allVanillaSkinDefinitions.Length, 0.77f, 0.88f));

            try
            {
                var new_skinDef = ScriptableObject.CreateInstance<MSU.VanillaSkinDefinition>();

                new_skinDef.bodyAddress = old_skinDef.bodyAddress;
                new_skinDef.displayAddress = old_skinDef.displayAddress;
                new_skinDef.icon = old_skinDef.icon;
                new_skinDef.nameToken = old_skinDef.nameToken;
                new_skinDef.unlockableDef = old_skinDef.unlockableDef;

                List<AddressReferencedSkinDef> _baseSkinDefs = new List<AddressReferencedSkinDef>();
                foreach(var baseSkinDef in old_skinDef._baseSkins)
                {
                    var asset = baseSkinDef.skin;
                    if(asset)
                    {
                        _baseSkinDefs.Add(new AddressReferencedSkinDef { Asset = asset });
                    }
                    else
                    {
                        _baseSkinDefs.Add(new AddressReferencedSkinDef { Address = baseSkinDef.skinAddress });
                    }
                }
                new_skinDef._baseSkins = _baseSkinDefs.ToArray();

                var _gameObjectActivations = new List<MSU.VanillaSkinDefinition.CustomGameObjectActivation>();
                foreach(var gameObjectActivation in old_skinDef._gameObjectActivations)
                {
                    _gameObjectActivations.Add(new MSU.VanillaSkinDefinition.CustomGameObjectActivation
                    {
                        isCustomActivation = gameObjectActivation.isCustomActivation,
                        localAngles = Vector3.zero,
                        shouldActivate = gameObjectActivation.shouldActivate,
                        localScale = Vector3.one,
                        childLocatorEntry = gameObjectActivation.childName,
                        customObject = gameObjectActivation.gameObjectPrefab,
                        localPos = Vector3.zero,
                        renderer = gameObjectActivation.rendererIndex
                    });
                }
                new_skinDef._gameObjectActivations = _gameObjectActivations.ToArray();

                var _meshReplacements = new List<MSU.VanillaSkinDefinition.CustomMeshReplacement>();
                foreach(var meshReplacement in old_skinDef._meshReplacements)
                {
                    _meshReplacements.Add(new MSU.VanillaSkinDefinition.CustomMeshReplacement
                    {
                        newMesh = meshReplacement.mesh,
                        renderer = meshReplacement.rendererIndex
                    });
                }
                new_skinDef._meshReplacements = _meshReplacements.ToArray();

                var _minionSkinReplacements = new List<MSU.VanillaSkinDefinition.AddressedMinionSkinReplacement>();
                foreach(var minionSkinReplacement in old_skinDef._minionSkinReplacements)
                {
                    _minionSkinReplacements.Add(new MSU.VanillaSkinDefinition.AddressedMinionSkinReplacement
                    {
                        minionPrefab = minionSkinReplacement.minionPrefabAddress,
                        minionSkin = minionSkinReplacement.minionSkin,
                    });
                }
                new_skinDef._minionSkinReplacements = _minionSkinReplacements.ToArray();

                var _projectileGhostReplacements = new List<MSU.VanillaSkinDefinition.AddressedProjectileGhostReplacement>();
                foreach(var projectileGhostReplacement in old_skinDef._projectileGhostReplacements)
                {
                    _projectileGhostReplacements.Add(new MSU.VanillaSkinDefinition.AddressedProjectileGhostReplacement
                    {
                        ghostReplacement = projectileGhostReplacement.projectileGhostReplacement,
                        projectilePrefab = projectileGhostReplacement.projectilePrefabAddress
                    });
                }
                new_skinDef._projectileGhostReplacements = _projectileGhostReplacements.ToArray();

                var _rendererInfos = new List<MSU.VanillaSkinDefinition.RendererInfo>();
                foreach(var rendererInfo in old_skinDef._rendererInfos)
                {
                    _rendererInfos.Add(new MSU.VanillaSkinDefinition.RendererInfo
                    {
                        defaultMaterial = rendererInfo.defaultMaterial,
                        defaultShadowCastingMode = rendererInfo.defaultShadowCastingMode,
                        hideOnDeath = rendererInfo.hideOnDeath,
                        ignoreOverlays = rendererInfo.ignoreOverlays,
                        renderer = rendererInfo.rendererIndex
                    });
                }
                new_skinDef._rendererInfos = _rendererInfos.ToArray();

                UpgradeAsset(old_skinDef, new_skinDef);
            }
            catch(Exception e)
            {
                MSULog.Error($"Could not upgrade {old_skinDef.name}. {e}");
            }
        }
    }

    private static void UpgradeVanillaSkinDefinitionToVanillaSkinDef(ProgressBar progressBar)
    {
        var allVanillaSkinDefinitions = AssetDatabaseUtils.FindAssetsByType<MSU.VanillaSkinDefinition>().ToArray();

        if(allVanillaSkinDefinitions.Length == 0)
        {
            return;
        }

        for(int i = 0; i < allVanillaSkinDefinitions.Length; i++)
        {
            var old_vanillaSkinDefinition = allVanillaSkinDefinitions[i];
            UpdateProgressBar(progressBar, $"Upgrading {old_vanillaSkinDefinition.name}", Util.Remap(i, 0, allVanillaSkinDefinitions.Length, 0.88f, 1f));

            try
            {
                var new_skinDef = ScriptableObject.CreateInstance<MSU.VanillaSkinDef>();

                new_skinDef._bodyAddress = old_vanillaSkinDefinition.bodyAddress;
                new_skinDef._displayAddress = old_vanillaSkinDefinition.displayAddress;
                new_skinDef.icon = old_vanillaSkinDefinition.icon;
                new_skinDef.nameToken = old_vanillaSkinDefinition.nameToken;
                new_skinDef.unlockableDef = old_vanillaSkinDefinition.unlockableDef;

                List<VanillaSkinDef.MoonstormBaseSkin> _baseSkinDefs = new List<VanillaSkinDef.MoonstormBaseSkin>();
                foreach (var baseSkinDef in old_vanillaSkinDefinition._baseSkins)
                {
                    if (baseSkinDef.AssetExists)
                    {
                        _baseSkinDefs.Add(new VanillaSkinDef.MoonstormBaseSkin { _skinDef = baseSkinDef.Asset });
                    }
                    else
                    {
                        _baseSkinDefs.Add(new VanillaSkinDef.MoonstormBaseSkin { _skinAddress = baseSkinDef.Address });
                    }
                }
                new_skinDef._baseSkins = _baseSkinDefs.ToArray();

                var _gameObjectActivations = new List<VanillaSkinDef.MoonstormGameObjectActivation>();
                foreach (var gameObjectActivation in old_vanillaSkinDefinition._gameObjectActivations)
                {
                    _gameObjectActivations.Add(new VanillaSkinDef.MoonstormGameObjectActivation
                    {
                        isCustomActivation = gameObjectActivation.isCustomActivation,
                        localAngles = Vector3.zero,
                        shouldActivate = gameObjectActivation.shouldActivate,
                        localScale = Vector3.one,
                        childName = gameObjectActivation.childLocatorEntry,
                        gameObjectPrefab = gameObjectActivation.customObject,
                        localPos = Vector3.zero,
                        rendererIndex = gameObjectActivation.renderer
                    });
                }
                new_skinDef._gameObjectActivations = _gameObjectActivations.ToArray();

                var _meshReplacements = new List<MSU.VanillaSkinDef.MoonstormMeshReplacement>();
                foreach (var meshReplacement in old_vanillaSkinDefinition._meshReplacements)
                {
                    _meshReplacements.Add(new MSU.VanillaSkinDef.MoonstormMeshReplacement
                    {
                        mesh = meshReplacement.newMesh,
                        rendererIndex = meshReplacement.renderer
                    });
                }
                new_skinDef._meshReplacements = _meshReplacements.ToArray();

                var _minionSkinReplacements = new List<MSU.VanillaSkinDef.MoonstormMinionSkinReplacement>();
                foreach (var minionSkinReplacement in old_vanillaSkinDefinition._minionSkinReplacements)
                {
                    _minionSkinReplacements.Add(new MSU.VanillaSkinDef.MoonstormMinionSkinReplacement
                    {
                        _minionPrefabAddress = minionSkinReplacement.minionPrefab.Address,
                        _minionSkin = minionSkinReplacement.minionSkin,
                    });
                }
                new_skinDef._minionSkinReplacements = _minionSkinReplacements.ToArray();

                var _projectileGhostReplacements = new List<MSU.VanillaSkinDef.MoonstormProjectileGhostReplacement>();
                foreach (var projectileGhostReplacement in old_vanillaSkinDefinition._projectileGhostReplacements)
                {
                    _projectileGhostReplacements.Add(new MSU.VanillaSkinDef.MoonstormProjectileGhostReplacement
                    {
                        _projectileGhostReplacement = projectileGhostReplacement.ghostReplacement,
                        _projectilePrefabAddress = projectileGhostReplacement.projectilePrefab.Address
                    });
                }
                new_skinDef._projectileGhostReplacements = _projectileGhostReplacements.ToArray();

                var _rendererInfos = new List<MSU.VanillaSkinDef.MoonstormRendererInfo>();
                foreach (var rendererInfo in old_vanillaSkinDefinition._rendererInfos)
                {
                    _rendererInfos.Add(new MSU.VanillaSkinDef.MoonstormRendererInfo
                    {
                        defaultMaterial = rendererInfo.defaultMaterial,
                        defaultShadowCastingMode = rendererInfo.defaultShadowCastingMode,
                        hideOnDeath = rendererInfo.hideOnDeath,
                        ignoreOverlays = rendererInfo.ignoreOverlays,
                        rendererIndex = rendererInfo.renderer
                    });
                }
                new_skinDef._rendererInfos = _rendererInfos.ToArray();

                UpgradeAsset(old_vanillaSkinDefinition, new_skinDef);
            }
            catch (Exception e)
            {
                MSULog.Error($"Could not upgrade {old_vanillaSkinDefinition.name}. {e}");
            }
        }
    }

    private static void UpdateProgressBar(ProgressBar bar, string message, float progress)
    {
        bar.Update(message, "Upgrading Scriptable Objects", progress);
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
