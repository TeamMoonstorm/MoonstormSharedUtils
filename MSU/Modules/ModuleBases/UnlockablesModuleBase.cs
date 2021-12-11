using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm
{
    public abstract class UnlockablesModuleBase : ModuleBase
    {
        public static Dictionary<MSUnlockableDef, UnlockableBase> unlockableDefToUnlockableBase = new Dictionary<MSUnlockableDef, UnlockableBase>();

        private static List<UnlockableBase> unlocksToAdd = new List<UnlockableBase>();

        //Things for AchievementManager
        private static Dictionary<string, AchievementDef> achievementNameToDef = new Dictionary<string, AchievementDef>();
        private static List<string> achievementIdentifiers = new List<string>();
        private static AchievementDef[] achievementDefs = Array.Empty<AchievementDef>();
        private static AchievementDef[] serverAchievementDefs = Array.Empty<AchievementDef>();

        [SystemInitializer]
        private static void HookInit()
        {
            MSULog.LogI($"Subscribing to Delegates related to Unlockables");
            AchievementManager.onAchievementsRegistered += AddUnlockables;
        }

        #region Unlockables
        public virtual IEnumerable<UnlockableBase> InitializeUnlockables()
        {
            MSULog.LogD($"Getting the Unlockables found inside {GetType().Assembly}");
            return GetContentClasses<UnlockableBase>();
        }

        public void AddUnlockable(UnlockableBase unlockableBase, SerializableContentPack contentPack, Dictionary<MSUnlockableDef, UnlockableBase> unlockableDefToUnlockableBaseDict = null)
        {
            unlockableBase.Initialize();
            if (CheckIfRequiredTypeIsAdded(unlockableBase))
            {
                HG.ArrayUtils.ArrayAppend(ref contentPack.unlockableDefs, unlockableBase.UnlockableDef);
                unlockableDefToUnlockableBase.Add(unlockableBase.UnlockableDef, unlockableBase);

                if (unlockableDefToUnlockableBaseDict != null) unlockableDefToUnlockableBaseDict.Add(unlockableBase.UnlockableDef, unlockableBase);

                unlocksToAdd.Add(unlockableBase);
                MSULog.LogD($"Added {unlockableBase.UnlockableDef}");
            }
            else
            {
                MSULog.LogD($"Not Adding unlockable {unlockableBase.UnlockableDef} since one of its required types is not added to the game.");
            }
        }

        #endregion

        #region Checks
        private bool CheckIfRequiredTypeIsAdded(UnlockableBase unlockBase)
        {
            if (unlockBase.RequiredTypes.Length > 0)
            {
                List<bool> flags = new List<bool>();
                foreach (Type type in unlockBase.RequiredTypes)
                {
                    bool isAdded = false;
                    switch (type)
                    {
                        case Type t when t.IsSubclassOf(typeof(ArtifactBase)): isAdded = CheckArtifacts(type); break;
                        case Type t when t.IsSubclassOf(typeof(BuffBase)): isAdded = CheckBuffs(type); break;
                        case Type t when t.IsSubclassOf(typeof(DamageTypeBase)): isAdded = CheckDamageTypes(type); break;
                        case Type t when t.IsSubclassOf(typeof(EliteEquipmentBase)): isAdded = CheckEliteEquipments(type); break;
                        case Type t when t.IsSubclassOf(typeof(EquipmentBase)): isAdded = CheckEquipments(type); break;
                        case Type t when t.IsSubclassOf(typeof(ItemBase)): isAdded = CheckItems(type); break;
                        case Type t when t.IsSubclassOf(typeof(MonsterBase)): isAdded = CheckMonsters(type); break;
                        case Type t when t.IsSubclassOf(typeof(ProjectileBase)): isAdded = CheckProjectiles(type); break;
                        case Type t when t.IsSubclassOf(typeof(SceneBase)): isAdded = CheckScenes(type); break;
                        case Type t when t.IsSubclassOf(typeof(UnlockableBase)): isAdded = CheckUnlockables(type); break;
                        case Type t when t.IsSubclassOf(typeof(SurvivorBase)): isAdded = CheckSurvivors(type); break;
                    }

                    if (!isAdded)
                    {
                        isAdded = OnFailedToCheck(type);
                    }
                    flags.Add(isAdded);
                }
                if (flags.Any(flag => flag == false))
                {
                    return false;
                }
            }
            return true;
        }

        public virtual bool OnFailedToCheck(Type type) { return false; }

        private static bool CheckArtifacts(Type type)
        {
            var allArtifactBases = ArtifactModuleBase.MoonstormArtifacts.Values.Select(ab => ab.GetType());
            return allArtifactBases.Contains(type);
        }

        private static bool CheckBuffs(Type type)
        {
            var allBuffs = BuffModuleBase.MoonstormBuffs.Values.Select(bb => bb.GetType());
            return allBuffs.Contains(type);
        }

        private static bool CheckDamageTypes(Type type)
        {
            var allDamageTypes = DamageTypeModuleBase.MoonstormDamageTypes.Values.Select(dtb => dtb.GetType());
            return allDamageTypes.Contains(type);
        }

        private static bool CheckEliteEquipments(Type type)
        {
            var allEliteEquipments = PickupModuleBase.MoonstormEliteEquipments.Values.Select(eeb => eeb.GetType());
            return allEliteEquipments.Contains(type);
        }

        private static bool CheckEquipments(Type type)
        {
            var allEquipments = PickupModuleBase.MoonstormNonEliteEquipments.Values.Select(eb => eb.GetType());
            return allEquipments.Contains(type);
        }

        private static bool CheckItems(Type type)
        {
            var allItems = PickupModuleBase.MoonstormItems.Values.Select(ib => ib.GetType());
            return allItems.Contains(type);
        }

        private static bool CheckMonsters(Type type)
        {
            var allMonsters = CharacterModuleBase.MoonstormCharacters.Where(charBase => charBase.GetType().IsSubclassOf(typeof(MonsterBase))).Select(mb => mb.GetType());
            return allMonsters.Contains(type);
        }

        private static bool CheckProjectiles(Type type)
        {
            var allProjectiles = ProjectileModuleBase.MoonstormProjectiles.Values.Select(pb => pb.GetType());
            return allProjectiles.Contains(type);
        }

        private static bool CheckScenes(Type type)
        {
            var allScenes = SceneModuleBase.MoonstormScenes.Values.Select(sb => sb.GetType());
            return allScenes.Contains(type);
        }

        private static bool CheckUnlockables(Type type)
        {
            var allUnlocks = unlockableDefToUnlockableBase.Values.Select(ub => ub.GetType());
            return allUnlocks.Contains(type);
        }
        private static bool CheckSurvivors(Type type)
        {
            var allSurvivors = CharacterModuleBase.MoonstormCharacters.Where(charBase => charBase.GetType().IsSubclassOf(typeof(SurvivorBase))).Select(sb => sb.GetType());
            return allSurvivors.Contains(type);
        }
        #endregion

        #region Hooks
        private static void AddUnlockables()
        {
            List<AchievementDef> list = new List<AchievementDef>();
            achievementNameToDef.Clear();

            foreach (UnlockableBase unlockable in unlocksToAdd)
            {
                try
                {
                    var achievementIdentifier = unlockable.UnlockableDef.cachedName + ".Achievement";
                    var unlockableDef = unlockable.UnlockableDef;

                    if (achievementNameToDef.ContainsKey(achievementIdentifier))
                        throw new ArgumentException($"UnlockableBase {unlockable.GetType().FullName} attempted to register as achievement {achievementIdentifier}, but class {achievementNameToDef[achievementIdentifier].type.FullName}");

                    AchievementDef achievementDef = unlockableDef.GetOrCreateAchievementDef();

                    achievementIdentifiers.Add(achievementIdentifier);
                    achievementNameToDef.Add(achievementIdentifier, achievementDef);
                    list.Add(achievementDef);

                    unlockableDef.getHowToUnlockString = () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", Language.GetString(achievementDef.nameToken), Language.GetString(achievementDef.descriptionToken));
                    unlockableDef.getUnlockedString = () => Language.GetStringFormatted("UNLOCKED_FORMAT", Language.GetString(achievementDef.nameToken), Language.GetString(achievementDef.descriptionToken));
                }
                catch (Exception e)
                {
                    MSULog.LogE($"An Exception has Ocurred while trying to add {unlockable}: {e}");
                }
            }

            achievementDefs = list.ToArray();
            SortAchievements(achievementDefs);
            serverAchievementDefs = achievementDefs.Where(achievementDef => achievementDef.serverTrackerType != null).ToArray();

            for (int i = 0; i < achievementDefs.Length; i++)
            {
                achievementDefs[i].index = new AchievementIndex
                {
                    intValue = AchievementManager.achievementCount + i
                };
            }
            for (int j = 0; j < serverAchievementDefs.Length; j++)
            {
                serverAchievementDefs[j].serverIndex = new ServerAchievementIndex
                {
                    intValue = AchievementManager.serverAchievementCount + j
                };
            }
            for (int k = 0; k < achievementIdentifiers.Count; k++)
            {
                string currentIdentifier = achievementIdentifiers[k];
                achievementNameToDef[currentIdentifier].childAchievementIdentifiers = achievementIdentifiers.Where(identifier => achievementNameToDef[identifier].prerequisiteAchievementIdentifier == currentIdentifier).ToArray();
            }

            AchievementManager.achievementNamesToDefs = AchievementManager.achievementNamesToDefs.Union(achievementNameToDef).ToDictionary(x => x.Key, y => y.Value);
            AchievementManager.achievementIdentifiers.AddRange(achievementIdentifiers);
            AchievementManager.achievementDefs = AchievementManager.achievementDefs.ToList().Union(achievementDefs).ToArray();
            AchievementManager.serverAchievementDefs = AchievementManager.serverAchievementDefs.ToList().Union(serverAchievementDefs).ToArray();

            MSULog.LogI($"Succesfully added a total of {achievementIdentifiers.Count} Achievements.");
            achievementNameToDef.Clear();
            achievementIdentifiers.Clear();
            achievementDefs = Array.Empty<AchievementDef>();
            serverAchievementDefs = Array.Empty<AchievementDef>();
        }

        private static void SortAchievements(AchievementDef[] achievementDefsArray)
        {
            AchievementManager.AchievementSortPair[] array = new AchievementManager.AchievementSortPair[achievementDefsArray.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new AchievementManager.AchievementSortPair
                {
                    score = UnlockableCatalog.GetUnlockableSortScore(achievementDefsArray[i].unlockableRewardIdentifier),
                    achievementDef = achievementDefsArray[i]
                };
            }
            Array.Sort(array, (AchievementManager.AchievementSortPair a, AchievementManager.AchievementSortPair b) => a.score - b.score);
            for (int j = 0; j < array.Length; j++)
            {
                achievementDefsArray[j] = array[j].achievementDef;
            }
        }
        #endregion
    }
}
