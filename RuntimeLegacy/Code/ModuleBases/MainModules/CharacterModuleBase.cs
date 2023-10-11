using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// The <see cref="CharacterModuleBase"/> is a <see cref="ContentModule{T}"/> that handles the <see cref="CharacterBase"/> class
    /// <para>The <see cref="CharacterModuleBase"/>'s main job is to handle the proper addition of CharacterBodies and CharacterMasters from <see cref="CharacterBase"/> inheriting classes</para>
    /// <para>For more specific uses there's the <see cref="MonsterBase"/> class, used for adding a Monster to the game and have it spawned via the Combat Director</para>
    /// <para>There's also the <see cref="SurvivorBase"/> class, used for adding a survivor to the game via a SurvivorDef</para>
    /// <para>Inherit from this module if you want to load and manage Characters with <see cref="CharacterBase"/> systems</para>
    /// </summary>
    public abstract class CharacterModuleBase : ContentModule<CharacterBase>
    {
        private class MonsterCollectionFuncPair
        {
            public struct Comparer : IEqualityComparer<MonsterCollectionFuncPair>
            {
                bool IEqualityComparer<MonsterCollectionFuncPair>.Equals(MonsterCollectionFuncPair x, MonsterCollectionFuncPair y)
                {
                    if (x == null || y == null)
                        return false;

                    if (x.tiedMonsterBase == null || y.tiedMonsterBase == null) 
                        return false;

                    return x.tiedMonsterBase == y.tiedMonsterBase;
                }

                int IEqualityComparer<MonsterCollectionFuncPair>.GetHashCode(MonsterCollectionFuncPair obj)
                {
                    if (obj == null)
                        return -1;
                    if (obj.tiedMonsterBase == null)
                        return -1;
                    return obj.tiedMonsterBase.GetHashCode();
                }
            }

            public HashSet<MSMonsterDirectorCard> cards = new HashSet<MSMonsterDirectorCard>(new MSMonsterDirectorCard.PrefabComparer());
            public MonsterBase tiedMonsterBase;
            public MonsterBase.IsAvailableForDCCSDelegate IsAvailable => tiedMonsterBase.IsAvailableForDCCS;
        }
        #region Properties and Fields
        /// <summary>
        /// A ReadOnlyDictionary that can be used for loading a specific CharacterBase by giving it's tied CharacterBody prefab
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionaryCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<GameObject, CharacterBase> MoonstormCharacters { get; private set; }
        internal static Dictionary<GameObject, CharacterBase> characters = new Dictionary<GameObject, CharacterBase>();

        /// <summary>
        /// Returns all the MonsterBases from the <see cref="MoonstormCharacters"/>
        /// </summary>
        public static MonsterBase[] MoonstormMonsters { get => MoonstormCharacters.Values.Where(cb => cb.GetType().IsSubclassOf(typeof(MonsterBase))).Cast<MonsterBase>().ToArray(); }
        /// <summary>
        /// Returns all the MonsterBases that have <see cref="MSMonsterDirectorCard"/>
        /// </summary>
        public static MonsterBase[] MonstersWithCards { get => MoonstormMonsters.Where(mb => mb.MonsterDirectorCards.Count > 0).ToArray(); }
        /// <summary>
        /// Returns all the SurvivorBases from the <see cref="MoonstormCharacters"/>
        /// </summary>
        public static SurvivorBase[] MoonstormSurvivors { get => MoonstormCharacters.Values.Where(cb => cb.GetType().IsSubclassOf(typeof(SurvivorBase))).Cast<SurvivorBase>().ToArray(); }
        /// <summary>
        /// Returns all the CharacterBody prefabs from the <see cref="MoonstormCharacters"/>
        /// </summary>
        public static GameObject[] LoadedCharacterBodies { get => MoonstormCharacters.Values.Select(cb => cb.BodyPrefab).ToArray(); }
        /// <summary>
        /// Returns all the CharacterMaster prefabs from the <see cref="MoonstormCharacters"/>
        /// </summary>
        public static GameObject[] LoadedCharacterMasters { get => MoonstormCharacters.Values.Select(cb => cb.MasterPrefab).ToArray(); }

        /// <summary>
        /// Call moduleAvailability.CallWhenAvailable() to run a method after the Module is initialized.
        /// </summary>
        public static ResourceAvailability moduleAvailability;

        private static Dictionary<DirectorAPI.Stage, HashSet<MonsterCollectionFuncPair>> currentStageToCards = new Dictionary<DirectorAPI.Stage, HashSet<MonsterCollectionFuncPair>>();
        private static Dictionary<string, HashSet<MonsterCollectionFuncPair>> currentCustomStageToCards = new Dictionary<string, HashSet<MonsterCollectionFuncPair>>(StringComparer.OrdinalIgnoreCase);
        #endregion

        [SystemInitializer(new Type[] { typeof(BodyCatalog), typeof(MasterCatalog) })]
        private static void SystemInit()
        {
            MSULog.Info("Initializing Character Module...");
            Run.onRunStartGlobal += PopulateDictionaries;
            DirectorAPI.MonsterActions += AddCustomMonsters;

            MoonstormCharacters = new ReadOnlyDictionary<GameObject, CharacterBase>(characters);
            characters = null;

            moduleAvailability.MakeAvailable();
        }

        #region Characters
        /// <summary>
        /// <inheritdoc cref="ModuleBase{T}.GetContentClasses{T}(Type)"/>
        /// <para>T in this case is <see cref="CharacterBase"/></para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's <see cref="CharacterBase"/></returns>
        protected virtual IEnumerable<CharacterBase> GetCharacterBases()
        {
            MSULog.Debug($"Getting the Characters found inside {GetType().Assembly}...");
            return GetContentClasses<CharacterBase>();
        }

        /// <summary>
        /// Adds a CharacterBase's CharacterBody and CharacterMaster prefabs to the game and to the ContentPack
        /// <para>If the character is a <see cref="MonsterBase"/> and it inherits <see cref="MonsterBase.MonsterDirectorCard"/>, then it'll appear ingame using the CombatDirector</para>
        /// <para>If the character is a <see cref="SurvivorBase"/>, then it's SurvivorDef will be added to the ContentPack</para>
        /// </summary>
        /// <param name="character">The CharacterBase to add</param>
        /// <param name="characterDictionary">Optional, a dictionary to add your initialized CharacterBase and CharacterBody game objects</param>
        protected void AddCharacter(CharacterBase character, Dictionary<GameObject, CharacterBase> characterDictionary = null)
        {
            InitializeContent(character);
            characterDictionary?.Add(character.BodyPrefab, character);
        }

        /// <summary>
        /// <inheritdoc cref="AddCharacter(CharacterBase, Dictionary{GameObject, CharacterBase})"/>
        /// </summary>
        protected override void InitializeContent(CharacterBase contentClass)
        {
            AddSafely(ref SerializableContentPack.bodyPrefabs, contentClass.BodyPrefab, "BodyPrefabs");

            contentClass.Initialize();

            switch (contentClass)
            {
                case MonsterBase monster:
                    AddSafely(ref SerializableContentPack.masterPrefabs, monster.MasterPrefab, "MasterPrefabs");
                    MSULog.Debug($"Character {monster} Initialized and ensured it's body and master prefabs in {SerializableContentPack.name}");
                    break;
                case SurvivorBase survivor:
                    AddSafely(ref SerializableContentPack.survivorDefs, survivor.SurvivorDef);
                    AddSafely(ref SerializableContentPack.masterPrefabs, survivor.MasterPrefab, "MasterPrefabs");
                    MSULog.Debug($"Character {survivor} Initialized and ensured it's body, master prefabs & survivor def in {SerializableContentPack.name}");
                    break;
            }
            characters.Add(contentClass.BodyPrefab, contentClass);
        }
        #endregion

        #region Hooks
        private static void PopulateDictionaries(Run run)
        {
            ClearDictionaries();
            ExpansionDef[] runExpansions = ExpansionCatalog.expansionDefs.Where(exp => run.IsExpansionEnabled(exp)).ToArray();
            MonsterBase[] monsterBases = MonstersWithCards.ToArray();

            int num = 0;
            foreach(MonsterBase monsterBase in monsterBases)
            {
                AddMonsterBaseToRun(monsterBase, run, runExpansions, ref num);
            }

#if DEBUG
            MSULog.Info(num > 0 ? $"A total of {num} monster cards added to the run" : $"No monster cards added to the run");
#endif
        }

        private static void AddMonsterBaseToRun(MonsterBase monsterBase, Run run, ExpansionDef[] runExpansions, ref int totalMonstersAdded)
        {
            foreach (MSMonsterDirectorCard card in monsterBase.MonsterDirectorCards)
            {
                try
                {
                    //If card cant appear, skip
                    if (!card.IsAvailable(runExpansions))
                    {
                        continue;
                    }

                    foreach (DirectorAPI.Stage stageValue in Enum.GetValues(typeof(DirectorAPI.Stage)))
                    {
                        //Card has custom stage support? add them to the dictionaries.
                        if (stageValue == DirectorAPI.Stage.Custom && card.stages.HasFlag(stageValue))
                        {
                            AddCardToCustomStages(card, monsterBase);
                            continue;
                        }

                        //Card can appear in current stage? add it to the dictionary
                        if (card.stages.HasFlag(stageValue))
                        {
                            AddCardToStage(card, monsterBase, stageValue);
                        }
                    }
                    totalMonstersAdded++;
                }
                catch (Exception e)
                {
                    MSULog.Error($"{e}\nCard: {card}");
                }
            }
        }

        private static void AddCardToCustomStages(MSMonsterDirectorCard card, MonsterBase monsterBase)
        {
            foreach (string baseStageName in card.customStages)
            {
                //If the dictionary doesnt have an entry for this custom stage, create a new one alongside the list of monsters.
                if (!currentCustomStageToCards.ContainsKey(baseStageName))
                {
                    currentCustomStageToCards.Add(baseStageName, new HashSet<MonsterCollectionFuncPair>(new MonsterCollectionFuncPair.Comparer()));
                }

                HashSet<MonsterCollectionFuncPair> monsterCollectionSet = currentCustomStageToCards[baseStageName];
                //If there's no monster collection for this card's master prefab, create a new collection.
                MonsterCollectionFuncPair monsterCollection = FindMonsterCollection(monsterBase, monsterCollectionSet) ?? new MonsterCollectionFuncPair();

#if DEBUG
                if (monsterCollection.cards.Contains(card))
                {
                    MSULog.Warning($"There are two or more MSMonsterDirectorCards that are trying to add the same monster prefab to the stage {baseStageName}. (Card that triggered this warning: {card}))");
                    continue;
                }
#endif
                
                //Add card to monster collection hash set
                monsterCollection.cards.Add(card);
                monsterCollection.tiedMonsterBase = monsterBase;
                //Its a set, no neeed to check if the collection already exists.
                monsterCollectionSet.Add(monsterCollection);
            }
        }

        private static void AddCardToStage(MSMonsterDirectorCard card, MonsterBase monsterBase, DirectorAPI.Stage stageValue)
        {
            //If the dictionary doesnt have an entry for this stage, create a new one alongside the list of monsters.
            if (!currentStageToCards.ContainsKey(stageValue))
            {
                currentStageToCards[stageValue] = new HashSet<MonsterCollectionFuncPair>(new MonsterCollectionFuncPair.Comparer());
            }
            HashSet<MonsterCollectionFuncPair> monsterCollectionSet = currentStageToCards[stageValue];
            //If there's no monster collection for this card's master prefab, create a new collection.
            MonsterCollectionFuncPair monsterCollection = FindMonsterCollection(monsterBase, monsterCollectionSet) ?? new MonsterCollectionFuncPair();

#if DEBUG
            if (monsterCollection.cards.Contains(card))
            {
                MSULog.Warning($"There are two or more MSMonsterDirectorCards that are trying to add the same monster prefab to the stage {Enum.GetName(typeof(DirectorAPI.Stage), stageValue)}. (Card that triggered this warning: {card}))");
                return;
            }
#endif
            //Add card to monster collection hash set
            monsterCollection.cards.Add(card);
            monsterCollection.tiedMonsterBase = monsterBase;
            //Its a set, no neeed to check if the collection already exists.
            monsterCollectionSet.Add(monsterCollection);
        }

        private static MonsterCollectionFuncPair FindMonsterCollection(MonsterBase monsterBase, HashSet<MonsterCollectionFuncPair> set)
        {
            foreach(MonsterCollectionFuncPair collection in set)
            {
                if(collection.tiedMonsterBase == monsterBase)
                {
                    return collection;
                }
            }
            return null;
        }

        private static void ClearDictionaries()
        {
            currentCustomStageToCards.Clear();
            currentStageToCards.Clear();
        }
        private static void AddCustomMonsters(DccsPool pool, List<DirectorAPI.DirectorCardHolder> cardList, DirectorAPI.StageInfo stageInfo)
        {
            try
            {
                HashSet<MonsterCollectionFuncPair> monsters;
                if (stageInfo.stage == DirectorAPI.Stage.Custom)
                {
                    if (currentCustomStageToCards.TryGetValue(stageInfo.CustomStageName, out monsters))
                    {
                        AddMonstersToPool(pool, monsters, stageInfo);
                    }
                }
                else
                {
                    if (currentStageToCards.TryGetValue(stageInfo.stage, out monsters))
                        AddMonstersToPool(pool, monsters, stageInfo);
                }

                //MSULog.Info(cards.Count > 0 ? $"Added a total of {cards.Count} monster cards to stage {stageInfo.ToInternalStageName()}" : $"No monster cards added to stage {stageInfo.ToInternalStageName()}");
            }
            catch (Exception e)
            {
                MSULog.Error($"Failed to add custom monsters: {e}\n(Pool: {pool}, CardList: {cardList}, Stage: {stageInfo}, currentCustomStageToCards: {currentCustomStageToCards}, currentStageToCards: {currentStageToCards}");
            }
        }

        private static void AddMonstersToPool(DccsPool pool, HashSet<MonsterCollectionFuncPair> monsters, DirectorAPI.StageInfo stageInfo)
        {
            var standardCategory = pool.poolCategories.FirstOrDefault(category => category.name == DirectorAPI.Helpers.MonsterPoolCategories.Standard);
            if (standardCategory == null)
            {
                MSULog.Error($"Couldnt find standard category for current stage! not adding monsters!");
                return;
            }
            var dccs = standardCategory.alwaysIncluded.Select(pe => pe.dccs)
                .Concat(standardCategory.includedIfConditionsMet.Select(cpe => cpe.dccs))
                .Concat(standardCategory.includedIfNoConditionsMet.Select(pe => pe.dccs));

            foreach (MonsterCollectionFuncPair monsterCollection in monsters)
            {
                if(!monsterCollection.IsAvailable(stageInfo))
                {
                    continue;
                }
                try
                {
                    foreach (DirectorCardCategorySelection categorySelection in dccs)
                    {
                        foreach(var card in monsterCollection.cards)
                        {
                            categorySelection.AddCard(card.DirectorCardHolder);
                        }
                    }
                }
                catch (Exception e)
                {

                    MSULog.Error($"{e}\n(Monster Collection: {monsterCollection})");
                }
            }
        }
        #endregion
    }
}
