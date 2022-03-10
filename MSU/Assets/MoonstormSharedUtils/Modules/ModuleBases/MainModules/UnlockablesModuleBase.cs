using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Moonstorm
{
    public abstract class UnlockablesModuleBase : ContentModule<UnlockableBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<MSUnlockableDef, UnlockableBase> MoonstormUnlockables
        {
            get
            {
                if (!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve dictionary {MoonstormUnlockables}", typeof(UnlockablesModuleBase));
                    return null;
                }
                return MoonstormUnlockables;
            }
            private set
            {
                MoonstormUnlockables = value;
            }
        }
        private static Dictionary<MSUnlockableDef, UnlockableBase> unlocks = new Dictionary<MSUnlockableDef, UnlockableBase>();
        public static Action<ReadOnlyDictionary<MSUnlockableDef, UnlockableBase>> OnDictionaryCreated;

        public static UnlockableDef[] LoadedUnlockables { get => MoonstormUnlockables.Keys.Cast<UnlockableDef>().ToArray(); }
        public static AchievementDef[] LoadedAchievements { get => MoonstormUnlockables.Values.Select(ub => ub.GetAchievementDef).ToArray(); }
        public static bool Initialized { get; private set; } = false;
        #endregion

        [SystemInitializer(typeof(UnlockableCatalog), typeof(AchievementManager))]
        private static void SystemInit()
        {
            Initialized = true;
            MSULog.Info($"Initializing Unlockables Module...");

            MoonstormUnlockables = new ReadOnlyDictionary<MSUnlockableDef, UnlockableBase>(unlocks);
            unlocks.Clear();
            unlocks = null;

            OnDictionaryCreated?.Invoke(MoonstormUnlockables);
        }

        #region Unlockables
        protected virtual IEnumerable<UnlockableBase> GetUnlockableBases()
        {
            if(Initialized)
            {
                ThrowModuleInitialized($"Retrieve UnlockableBase list", typeof(UnlockablesModuleBase));
                return null;
            }

            MSULog.Debug($"Getting the Unlockables found inside {GetType().Assembly}");
            return GetContentClasses<UnlockableBase>();
        }

        protected void AddUnlockable(UnlockableBase unlockableBase, Dictionary<MSUnlockableDef, UnlockableBase> unlockableDictionary = null)
        {
            unlockableBase.Initialize();
            if(!CheckIfRequiredTypeIsAdded(unlockableBase))
            {
                MSULog.Debug($"Not adding {unlockableBase.UnlockableDef} since one of its required types is not added to the game.");
                return;
            }

            if(InitializeContent(unlockableBase) && unlockableDictionary != null)
                AddSafelyToDict(ref unlockableDictionary, unlockableBase.UnlockableDef, unlockableBase);

            MSULog.Debug($"Added {unlockableBase.UnlockableDef}");
        }

        protected override bool InitializeContent(UnlockableBase contentClass)
        {
            if(AddSafely(ref SerializableContentPack.unlockableDefs, contentClass.UnlockableDef))
            {
                contentClass.OnCheckPassed();
                FinishUnlockAndCreateAchievement(contentClass);

                R2API.UnlockableAPI.AddAchievement(contentClass.GetAchievementDef);

                AddSafelyToDict(ref unlocks, contentClass.UnlockableDef, contentClass);
                return true;
            }
            return false;
        }
        private void FinishUnlockAndCreateAchievement(UnlockableBase unlockable)
        {
            UnlockableDef unlockableDef = unlockable.UnlockableDef;

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

        public virtual bool OnFailedToCheck(Type type) { return false; }

        private static bool CheckArtifacts(Type type)
        {
            var allArtifactBases = ArtifactModuleBase.artifacts.Values.Select(ab => ab.GetType());
            return allArtifactBases.Contains(type);
        }

        private static bool CheckBuffs(Type type)
        {
            var allBuffs = BuffModuleBase.buffs.Values.Select(bb => bb.GetType());
            return allBuffs.Contains(type);
        }

        private static bool CheckDamageTypes(Type type)
        {
            var allDamageTypes = DamageTypeModuleBase.damageTypes.Values.Select(dtb => dtb.GetType());
            return allDamageTypes.Contains(type);
        }

        private static bool CheckEliteEquipments(Type type)
        {
            var allEliteEquipments = EquipmentModuleBase.eliteEquip.Values.Select(eeb => eeb.GetType());
            return allEliteEquipments.Contains(type);
        }

        private static bool CheckEquipments(Type type)
        {
            var allEquipments = EquipmentModuleBase.nonEliteEquip.Values.Select(eb => eb.GetType());
            return allEquipments.Contains(type);
        }

        private static bool CheckItems(Type type)
        {
            var allItems = ItemModuleBase.items.Values.Select(ib => ib.GetType());
            return allItems.Contains(type);
        }

        private static bool CheckMonsters(Type type)
        {
            var allMonsters = CharacterModuleBase.characters.Where(charBase => charBase.GetType().IsSubclassOf(typeof(MonsterBase))).Select(mb => mb.GetType());
            return allMonsters.Contains(type);
        }

        private static bool CheckProjectiles(Type type)
        {
            var allProjectiles = ProjectileModuleBase.projectiles.Values.Select(pb => pb.GetType());
            return allProjectiles.Contains(type);
        }

        private static bool CheckScenes(Type type)
        {
            var allScenes = SceneModuleBase.scenes.Values.Select(sb => sb.GetType());
            return allScenes.Contains(type);
        }

        private static bool CheckUnlockables(Type type)
        {
            var allUnlocks = unlocks.Values.Select(ub => ub.GetType());
            return allUnlocks.Contains(type);
        }
        private static bool CheckSurvivors(Type type)
        {
            var allSurvivors = CharacterModuleBase.characters.Where(charBase => charBase.GetType().IsSubclassOf(typeof(SurvivorBase))).Select(sb => sb.GetType());
            return allSurvivors.Contains(type);
        }
        #endregion
    }
}