using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm
{
    /// <summary>
    /// A Module Base for Managing Unlockables and Achievements
    /// <para>Automatically handles the creation of AchievementDefs</para>
    /// </summary>
    public abstract class UnlockablesModuleBase : ModuleBase
    {
        /// <summary>
        /// Dictionary of all the Unlockables loaded by Moonstorm Shared Utils
        /// </summary>
        public static Dictionary<MSUnlockableDef, UnlockableBase> MoonstormUnlockables = new Dictionary<MSUnlockableDef, UnlockableBase>();

        /// <summary>
        /// Returns all the UnlockableDefs loaded by Moonstorm Shared Utils
        /// </summary>
        public static UnlockableDef[] LoadedUnlockables { get => MoonstormUnlockables.Keys.Cast<UnlockableDef>().ToArray(); }
        //private static List<UnlockableBase> unlocksToAdd = new List<UnlockableBase>();

        #region Unlockables
        /// <summary>
        /// Finds all the UnlockableBase inheriting classes in your assembly and creates instances for each found
        /// <para>Ignores classes with the DisabledContent attribute</para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's UnlockableBases</returns>
        public virtual IEnumerable<UnlockableBase> InitializeUnlockables()
        {
            MSULog.LogD($"Getting the Unlockables found inside {GetType().Assembly}");
            return GetContentClasses<UnlockableBase>();
        }

        /// <summary>
        /// Initializes and Adds an Unlockable
        /// <para>If the unlockable has required types, MSU will look if said types are initialized, if not, the unlockable is not added to the game</para>
        /// </summary>
        /// <param name="unlockableBase">The UnlockableBase class</param>
        /// <param name="contentPack">Your Mod's content pack</param>
        /// <param name="unlockableDefToUnlockableBaseDict">Optional, a dictionary for getting an UnlockableBase by feeding it the corresponding UnlockableDef</param>
        public void AddUnlockable(UnlockableBase unlockableBase, SerializableContentPack contentPack, Dictionary<MSUnlockableDef, UnlockableBase> unlockableDefToUnlockableBaseDict = null)
        {
            unlockableBase.Initialize();
            if (CheckIfRequiredTypeIsAdded(unlockableBase))
            {
                unlockableBase.LateInitialization();
                FinishUnlockAndCreateAchievement(unlockableBase);

                HG.ArrayUtils.ArrayAppend(ref contentPack.unlockableDefs, unlockableBase.UnlockableDef);
                R2API.UnlockableAPI.AddAchievement(unlockableBase.GetAchievementDef);
                MoonstormUnlockables.Add(unlockableBase.UnlockableDef, unlockableBase);

                if (unlockableDefToUnlockableBaseDict != null) unlockableDefToUnlockableBaseDict.Add(unlockableBase.UnlockableDef, unlockableBase);

                MSULog.LogD($"Added {unlockableBase.UnlockableDef}");
            }
            else
            {
                MSULog.LogD($"Not Adding unlockable {unlockableBase.UnlockableDef} since one of its required types is not added to the game.");
            }
        }

        private void FinishUnlockAndCreateAchievement(UnlockableBase unlockable)
        {
            UnlockableDef unlockableDef = unlockable.UnlockableDef as UnlockableDef;

            AchievementDef achievementDef = unlockable.UnlockableDef.GetOrCreateAchievementDef();

            unlockableDef.getHowToUnlockString = () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", Language.GetString(achievementDef.nameToken), Language.GetString(achievementDef.descriptionToken));
            unlockableDef.getUnlockedString = () => Language.GetStringFormatted("UNLOCKED_FORMAT", Language.GetString(achievementDef.nameToken), Language.GetString(achievementDef.descriptionToken));
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

        /// <summary>
        /// If MoonstormSharedUtils fails to check if the required type is added, you can add your own workaround here
        /// </summary>
        /// <param name="type">The type that failed to check</param>
        /// <returns>True if the unlockable should be added, false otherwise</returns>
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
            var allUnlocks = MoonstormUnlockables.Values.Select(ub => ub.GetType());
            return allUnlocks.Contains(type);
        }
        private static bool CheckSurvivors(Type type)
        {
            var allSurvivors = CharacterModuleBase.MoonstormCharacters.Where(charBase => charBase.GetType().IsSubclassOf(typeof(SurvivorBase))).Select(sb => sb.GetType());
            return allSurvivors.Contains(type);
        }
        #endregion
    }
}