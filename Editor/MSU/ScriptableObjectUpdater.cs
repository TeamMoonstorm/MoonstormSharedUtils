using Moonstorm;
using MSU;
using RoR2.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
using MSU.Editor;

public static class ScriptableObjectUpdater
{
    private static FieldInfo InteractableCardProvider_PairsField = typeof(InteractableCardProvider).GetField("_serializedCardPairs", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo MonsterCardProvider_PairsField = typeof(MonsterCardProvider).GetField("_serializedCardPairs", BindingFlags.Instance | BindingFlags.NonPublic);


    [MenuItem(MSUConstants.MSUMenuRoot + "Upgrade Scriptable Objects")]
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
            catch(Exception ex)
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

    private static void UpgradeMSEliteDef()
    {
        var allMSEliteDefs = AssetDatabaseUtil.FindAssetsByType<MSEliteDef>().ToArray();

        if (allMSEliteDefs.Length == 0)
            return;

        for(int i = 0; i < allMSEliteDefs.Length; i++)
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

    private static void UpgradeItemDisplayDictionary()
    {
        var allItemDisplayDictionaries = AssetDatabaseUtil.FindAssetsByType<Moonstorm.ItemDisplayDictionary>().ToArray();

        if (allItemDisplayDictionaries.Length == 0)
            return;

        for(int i = 0; i < allItemDisplayDictionaries.Length; i++)
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

    private static void UpgradeNamedIDRS()
    {
        var allNamedIDRS = AssetDatabaseUtil.FindAssetsByType<NamedIDRS>().ToArray();

        if (allNamedIDRS.Length == 0)
            return;

        for(int i = 0; i < allNamedIDRS.Length; i++)
        {
            var old_namedIDRS = allNamedIDRS[i];

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

    private static void UpgradeMSUnlockableDef()
    {
        var allMSUnlockableDef = AssetDatabaseUtil.FindAssetsByType<MSUnlockableDef>().ToArray();

        if (allMSUnlockableDef.Length == 0)
            return;

        for(int i = 0; i < allMSUnlockableDef.Length; i++)
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
            catch(Exception e)
            {
                MSULog.Error($"Could not upgrade {msUnlockableDef.cachedName}. {e}");
            }
        }
    }

    private static void UpgradeVanillaSkinDefinition()
    {
        var allVanillaSkinDefinitions = AssetDatabaseUtil.FindAssetsByType<Moonstorm.VanillaSkinDefinition>().ToArray();

        if (allVanillaSkinDefinitions.Length == 0)
            return;

        for(int i = 0; i < allVanillaSkinDefinitions.Length; i++)
        {
            var old_skinDef = allVanillaSkinDefinitions[i];

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

    private static void UpgradeVanillaSkinDefinitionToVanillaSkinDef()
    {
        var allVanillaSkinDefinitions = AssetDatabaseUtil.FindAssetsByType<MSU.VanillaSkinDefinition>().ToArray();

        if(allVanillaSkinDefinitions.Length == 0)
        {
            return;
        }

        for(int i = 0; i < allVanillaSkinDefinitions.Length; i++)
        {
            var old_vanillaSkinDefinition = allVanillaSkinDefinitions[i];

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
