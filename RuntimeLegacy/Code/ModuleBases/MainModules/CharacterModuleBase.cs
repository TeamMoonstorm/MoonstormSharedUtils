using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
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
        public static ReadOnlyDictionary<GameObject, CharacterBase> MoonstormCharacters { get; private set; }
        internal static Dictionary<GameObject, CharacterBase> characters = new Dictionary<GameObject, CharacterBase>();

        public static MonsterBase[] MoonstormMonsters { get => MoonstormCharacters.Values.Where(cb => cb.GetType().IsSubclassOf(typeof(MonsterBase))).Cast<MonsterBase>().ToArray(); }

        public static MonsterBase[] MonstersWithCards { get => MoonstormMonsters.Where(mb => mb.MonsterDirectorCards.Count > 0).ToArray(); }

        public static SurvivorBase[] MoonstormSurvivors { get => MoonstormCharacters.Values.Where(cb => cb.GetType().IsSubclassOf(typeof(SurvivorBase))).Cast<SurvivorBase>().ToArray(); }

        public static GameObject[] LoadedCharacterBodies { get => MoonstormCharacters.Values.Select(cb => cb.BodyPrefab).ToArray(); }

        public static GameObject[] LoadedCharacterMasters { get => MoonstormCharacters.Values.Select(cb => cb.MasterPrefab).ToArray(); }

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

        protected virtual IEnumerable<CharacterBase> GetCharacterBases()
        {
            MSULog.Debug($"Getting the Characters found inside {GetType().Assembly}...");
            return GetContentClasses<CharacterBase>();
        }

        protected void AddCharacter(CharacterBase character, Dictionary<GameObject, CharacterBase> characterDictionary = null)
        {
            InitializeContent(character);
            characterDictionary?.Add(character.BodyPrefab, character);
        }

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
            foreach (MonsterBase monsterBase in monsterBases)
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
            foreach (MonsterCollectionFuncPair collection in set)
            {
                if (collection.tiedMonsterBase == monsterBase)
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
                if (!monsterCollection.IsAvailable(stageInfo))
                {
                    continue;
                }
                try
                {
                    foreach (DirectorCardCategorySelection categorySelection in dccs)
                    {
                        foreach (var card in monsterCollection.cards)
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
