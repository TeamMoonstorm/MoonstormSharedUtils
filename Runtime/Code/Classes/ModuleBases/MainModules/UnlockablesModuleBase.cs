using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Moonstorm
{
    /// <summary>
    /// The <see cref="UnlockablesModuleBase"/> is a <see cref="ContentModule{T}"/> that handles the <see cref="UnlockableBase"/> class
    /// <para><see cref="UnlockablesModuleBase"/>'s main job is to create and handle the UnlockableBase classes, alongside adding AchievementDefs created by <see cref="MSUnlockableDef"/></para>
    /// <para>Inherit from this module if you want to use Unlockables tied to Achievements for your mod</para>
    /// </summary>
    public abstract class UnlockablesModuleBase : ContentModule<UnlockableBase>
    {
        #region Properties and Fields
        /// <summary>
        /// A ReadOnlyDictionary that can be used for loading a specific <see cref="UnlockableBase"/> by giving it's tied <see cref="MSUnlockableDef"/>
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionaryCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<MSUnlockableDef, UnlockableBase> MoonstormUnlockables { get; private set; }
        private static Dictionary<MSUnlockableDef, UnlockableBase> unlocks = new Dictionary<MSUnlockableDef, UnlockableBase>();

        /// <summary>
        /// Loads all the <see cref="MSUnlockableDef"/>s from the <see cref="MoonstormUnlockables"/> dictionary.
        /// </summary>
        public static MSUnlockableDef[] LoadedUnlockables { get => MoonstormUnlockables.Keys.ToArray(); }
        /// <summary>
        /// Loads all the <see cref="AchievementDef"/>s from the unlockables in <see cref="MoonstormUnlockables"/> dictionary.
        /// </summary>
        public static AchievementDef[] LoadedAchievements { get => MoonstormUnlockables.Values.Select(ub => ub.GetAchievementDef).ToArray(); }
        /// <summary>
        /// An action that gets invoked when the <see cref="MoonstormUnlockables"/> dictionary has been populated
        /// </summary>
        public static Action<ReadOnlyDictionary<MSUnlockableDef, UnlockableBase>> OnDictionaryCreated;
        #endregion

        [SystemInitializer(typeof(UnlockableCatalog))]
        private static void SystemInit()
        {
            MSULog.Info($"Initializing Unlockables Module...");
            RoR2BepInExPack.VanillaFixes.SaferAchievementManager.OnCollectAchievementDefs += AddMSUDefs;

            MoonstormUnlockables = new ReadOnlyDictionary<MSUnlockableDef, UnlockableBase>(unlocks);
            unlocks = null;

            OnDictionaryCreated?.Invoke(MoonstormUnlockables);
        }

        private static void AddMSUDefs(List<string> arg1, Dictionary<string, AchievementDef> arg2, List<AchievementDef> arg3)
        {
            foreach(AchievementDef def in LoadedAchievements)
            {
                arg1.Add(def.identifier);
                arg2.Add(def.identifier, def);
                arg3.Add(def);
            }
        }

        #region Unlockables
        /// <summary>
        /// <inheritdoc cref="ModuleBase{T}.GetContentClasses{T}(Type)"/>
        /// <para>T in this case is <see cref="UnlockableBase"/></para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's <see cref="UnlockableBase"/></returns>
        protected virtual IEnumerable<UnlockableBase> GetUnlockableBases()
        {
            MSULog.Debug($"Getting the Unlockables found inside {GetType().Assembly}");
            return GetContentClasses<UnlockableBase>();
        }

        /// <summary>
        /// Adds an UnlockableBase to the game.
        /// <para>The UnlockableDef and AchievementDefs are only added if the required type for the unlockable base has been added</para>
        /// <para>For more information regarding RequiredTypes, check <see cref="UnlockableBase.AddRequiredType{T}"/></para>
        /// </summary>
        /// <param name="unlockableBase">The unlockable base to add</param>
        /// <param name="unlockableDictionary">Optional, an Dictionary to add your initialized unlockable bases and unlockable defs</param>
        protected void AddUnlockable(UnlockableBase unlockableBase, Dictionary<MSUnlockableDef, UnlockableBase> unlockableDictionary = null)
        {
            unlockableBase.Initialize();

            if(!CheckIfRequiredTypeIsAdded(unlockableBase))
            {
                MSULog.Debug($"Not adding {unlockableBase.UnlockableDef} since one of its required types is not added to the game.");
                return;
            }

            InitializeContent(unlockableBase);
            unlockableDictionary?.Add(unlockableBase.UnlockableDef, unlockableBase);
            MSULog.Debug($"Unlockable {unlockableBase} Initialized and ensured in {SerializableContentPack.name}");
        }

        /// <summary>
        /// Adds the unlockableDef of <paramref name="contentClass"/> to your mod's SerializableContentPack.
        /// <para>Once added, it'll call <see cref="UnlockableBase.OnCheckPassed"/></para>
        /// </summary>
        /// <param name="contentClass">The content class being initialized</param>
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

        /// <summary>
        /// Overwrite this method to add your own Required Typpe checking.
        /// </summary>
        /// <param name="type">The class that's required for an UnlockableBase to be added</param>
        /// <returns>defaults to false</returns>
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