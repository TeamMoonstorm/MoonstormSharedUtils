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
    /// <summary>
    /// The <see cref="CharacterModuleBase"/> is a <see cref="ContentModule{T}"/> that handles the <see cref="CharacterBase"/> class
    /// <para>The <see cref="CharacterModuleBase"/>'s main job is to handle the proper addition of CharacterBodies and CharacterMasters from <see cref="CharacterBase"/> inheriting classes</para>
    /// <para>For more specific uses there's the <see cref="MonsterBase"/> class, used for adding a Monster to the game and have it spawned via the Combat Director</para>
    /// <para>There's also the <see cref="SurvivorBase"/> class, used for adding a survivor to the game via a SurvivorDef</para>
    /// <para>Inherit from this module if you want to load and manage Characters with <see cref="CharacterBase"/> systems</para>
    /// </summary>
    public abstract class CharacterModuleBase : ContentModule<CharacterBase>
    {
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
        public static MonsterBase[] MonstersWithCards { get => MoonstormMonsters.Where(mb => mb.MonsterDirectorCard != null).ToArray(); }
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
        /// An action that gets invoked when the <see cref="MoonstormCharacters"/> dictionary has been populated
        /// </summary>
        public static Action<ReadOnlyDictionary<GameObject, CharacterBase>> OnDictionaryCreated;

        private static Dictionary<DirectorAPI.Stage, List<MSMonsterDirectorCard>> currentStageToCards = new Dictionary<DirectorAPI.Stage, List<MSMonsterDirectorCard>>();
        private static Dictionary<string, List<MSMonsterDirectorCard>> currentCustomStageToCards = new Dictionary<string, List<MSMonsterDirectorCard>>();
        #endregion

        [SystemInitializer(new Type[] { typeof(BodyCatalog), typeof(MasterCatalog) })]
        private static void SystemInit()
        {
            MSULog.Info("Initializing Character Module...");
            Run.onRunStartGlobal += PopulateDictionaries;
            DirectorAPI.MonsterActions += AddCustomMonsters;

            MoonstormCharacters = new ReadOnlyDictionary<GameObject, CharacterBase>(characters);
            characters = null;

            OnDictionaryCreated?.Invoke(MoonstormCharacters);
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

            switch(contentClass)
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
        }
        #endregion

        #region Hooks
        private static void PopulateDictionaries(Run run)
        {
            ClearDictionaries();
            ExpansionDef[] runExpansions = ExpansionCatalog.expansionDefs.Where(exp => run.IsExpansionEnabled(exp)).ToArray();
            MSMonsterDirectorCard[] cards = MonstersWithCards.Select(mb => mb.MonsterDirectorCard).ToArray();

            int num = 0;
            foreach (MSMonsterDirectorCard card in cards)
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
                        if (stageValue == DirectorAPI.Stage.Custom)
                        {
                            foreach (string baseStageName in card.customStages)
                            {
                                if (!currentCustomStageToCards.ContainsKey(baseStageName))
                                {
                                    currentCustomStageToCards.Add(baseStageName, new List<MSMonsterDirectorCard>());
                                }
                                currentCustomStageToCards[baseStageName].Add(card);
                            }
                            continue;
                        }

                        //Card can appear in current stage? add it to the dictionary
                        if (card.stages.HasFlag(stageValue))
                        {
                            if (!currentStageToCards.ContainsKey(stageValue))
                            {
                                currentStageToCards.Add(stageValue, new List<MSMonsterDirectorCard>());
                            }
                            currentStageToCards[stageValue].Add(card);
                        }
                    }
                    num++;
                }
                catch (Exception e)
                {
                    MSULog.Error($"{e}\nCard: {card}");
                }
            }

            MSULog.Info(num > 0 ? $"A total of {num} interactable cards added to the run" : $"No interactable cards added to the run");
        }

        private static void ClearDictionaries()
        {
            currentCustomStageToCards.Clear();
            currentStageToCards.Clear();
        }
        private static void AddCustomMonsters(DccsPool pool, List<DirectorAPI.DirectorCardHolder> cardList, DirectorAPI.StageInfo stageInfo)
        {
            List<MSMonsterDirectorCard> cards = new List<MSMonsterDirectorCard>();
            if(stageInfo.stage == DirectorAPI.Stage.Custom)
            {
                if(currentCustomStageToCards.TryGetValue(stageInfo.CustomStageName, out cards))
                {
                    AddCardsToPool(pool, cards);
                }
            }
            else if(currentStageToCards.TryGetValue(stageInfo.stage, out cards))
            {
                AddCardsToPool(pool, cards);
            }
            MSULog.Info(cards.Count > 0 ? $"Added a total of {cards.Count} monster cards to stage {stageInfo.ToInternalStageName()}" : $"No monster cards added to stage {stageInfo.ToInternalStageName()}");
        }

        private static void AddCardsToPool(DccsPool pool, List<MSMonsterDirectorCard> cards)
        {
            var standardCategory = pool.poolCategories.FirstOrDefault(category => category.name == DirectorAPI.Helpers.MonsterPoolCategories.Standard);
            if(standardCategory == null)
            {
                MSULog.Error($"Couldnt find standard category for current stage! not adding monsters!");
                return;
            }
            var dccs = standardCategory.alwaysIncluded.Select(pe => pe.dccs)
                .Concat(standardCategory.includedIfConditionsMet.Select(cpe => cpe.dccs))
                .Concat(standardCategory.includedIfNoConditionsMet.Select(pe => pe.dccs));

            foreach(MSMonsterDirectorCard card in cards)
            {
                try
                {
                    foreach(DirectorCardCategorySelection categorySelection in dccs)
                    {
                        categorySelection.AddCard(card.DirectorCardHolder);
                    }
                }
                catch(Exception e)
                {
                    MSULog.Error($"{e}\n(Card: {card})");
                }
            }
        }
        #endregion
    }
}
