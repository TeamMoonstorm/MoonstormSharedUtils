using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Moonstorm
{
    public abstract class UnlockablesModuleBase : ContentModule<UnlockableBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<MSUnlockableDef, UnlockableBase> MoonstormUnlockables { get; private set; }
        private static Dictionary<MSUnlockableDef, UnlockableBase> unlocks = new Dictionary<MSUnlockableDef, UnlockableBase>();

        public static MSUnlockableDef[] LoadedUnlockables { get => MoonstormUnlockables.Keys.ToArray(); }

        public static AchievementDef[] LoadedAchievements { get => MoonstormUnlockables.Values.Select(ub => ub.GetAchievementDef).ToArray(); }

        public static ResourceAvailability moduleAvailability;
        #endregion

        [SystemInitializer(typeof(UnlockableCatalog))]
        private static void SystemInit()
        {
            MSULog.Info($"Initializing Unlockables Module...");
            RoR2BepInExPack.VanillaFixes.SaferAchievementManager.OnCollectAchievementDefs += AddMSUDefs;

            MoonstormUnlockables = new ReadOnlyDictionary<MSUnlockableDef, UnlockableBase>(unlocks);
            unlocks = null;

            moduleAvailability.MakeAvailable();
        }

        private static void AddMSUDefs(List<string> arg1, Dictionary<string, AchievementDef> arg2, List<AchievementDef> arg3)
        {
            foreach (AchievementDef def in LoadedAchievements)
            {
                arg1.Add(def.identifier);
                arg2.Add(def.identifier, def);
                arg3.Add(def);
            }
        }

        #region Unlockables
        protected virtual IEnumerable<UnlockableBase> GetUnlockableBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Unlockables found inside {GetType().Assembly}");
#endif
            return GetContentClasses<UnlockableBase>();
        }

        protected void AddUnlockable(UnlockableBase unlockableBase, Dictionary<MSUnlockableDef, UnlockableBase> unlockableDictionary = null)
        {
            unlockableBase.Initialize();

            if (!CheckIfRequiredTypeIsAdded(unlockableBase))
            {
# if DEBUG
                MSULog.Debug($"Not adding {unlockableBase.UnlockableDef} since one of its required types is not added to the game.");
#endif
                return;
            }

            InitializeContent(unlockableBase);
            unlockableDictionary?.Add(unlockableBase.UnlockableDef, unlockableBase);
#if DEBUG
            MSULog.Debug($"Unlockable {unlockableBase} Initialized and ensured in {SerializableContentPack.name}");
#endif
        }

        protected override void InitializeContent(UnlockableBase contentClass)
        {
            AddSafely(ref SerializableContentPack.unlockableDefs, contentClass.UnlockableDef);
            contentClass.OnCheckPassed();
            FinishUnlockAndCreateAchievement(contentClass);
            unlocks.Add(contentClass.UnlockableDef, contentClass);
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
                        case Type t when t.IsSubclassOf(typeof(EliteTierDefBase)): isAdded = CheckEliteTierDefs(type); break;
                        case Type t when t.IsSubclassOf(typeof(EquipmentBase)): isAdded = CheckEquipments(type); break;
                        case Type t when t.IsSubclassOf(typeof(InteractableBase)): isAdded = CheckInteractables(type); break;
                        case Type t when t.IsSubclassOf(typeof(ItemBase)): isAdded = CheckItems(type); break;
                        case Type t when t.IsSubclassOf(typeof(MonsterBase)): isAdded = CheckMonsters(type); break;
                        case Type t when t.IsSubclassOf(typeof(ProjectileBase)): isAdded = CheckProjectiles(type); break;
                        case Type t when t.IsSubclassOf(typeof(SceneBase)): isAdded = CheckScenes(type); break;
                        case Type t when t.IsSubclassOf(typeof(SurvivorBase)): isAdded = CheckSurvivors(type); break;
                        case Type t when t.IsSubclassOf(typeof(UnlockableBase)): isAdded = CheckUnlockables(type); break;
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

        private static bool CheckEliteTierDefs(Type type)
        {
            var allEliteTierDefs = EliteTierDefModuleBase.eliteTierDefs.Values.Select(etd => etd.GetType());
            return allEliteTierDefs.Contains(type);
        }

        private static bool CheckEquipments(Type type)
        {
            var allEquipments = EquipmentModuleBase.nonEliteEquip.Values.Select(eb => eb.GetType());
            return allEquipments.Contains(type);
        }

        private static bool CheckInteractables(Type type)
        {
            var allInteractables = InteractableModuleBase.interactables.Values.Select(interactable => interactable.GetType());
            return allInteractables.Contains(type);
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