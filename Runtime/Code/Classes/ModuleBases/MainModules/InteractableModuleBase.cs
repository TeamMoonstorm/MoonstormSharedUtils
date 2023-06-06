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
    /// The <see cref="InteractableModuleBase"/> is a <see cref="ContentModule{T}"/> that handles the <see cref="InteractableBase"/> class
    /// <para><see cref="InteractableModuleBase"/>'s main job is to handle the proper addition of Interactable prefabs from <see cref="InteractableBase"/> inheriting classes</para>
    /// <para><see cref="InteractableBase"/>s that implement <see cref="InteractableBase.InteractableDirectorCard"/> will spawn in runs</para>
    /// <para>Inherit from this module if you want to load and manage Interactables with <see cref="InteractableBase"/> systems</para>
    /// </summary>
    public abstract class InteractableModuleBase : ContentModule<InteractableBase>
    {
        #region Properties and Fields
        /// <summary>
        /// A ReadOnlyDictionary that can be used for loading a specific <see cref="InteractableBase"/> by giving it's tied <see cref="GameObject"/>
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionaryCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<GameObject, InteractableBase> MoonstormInteractables { get; private set; }
        internal static Dictionary<GameObject, InteractableBase> interactables = new Dictionary<GameObject, InteractableBase>();

        /// <summary>
        /// Loads all the <see cref="InteractableBase"/> from the <see cref="MoonstormInteractables"/> dictionary that have a <see cref="MSInteractableDirectorCard"/>
        /// </summary>
        public static InteractableBase[] InteractablesWithCards { get => MoonstormInteractables.Values.Where(ib => ib.InteractableDirectorCard != null).ToArray(); }
        /// <summary>
        /// Loads all the <see cref="InteractableBase"/> from the <see cref="MoonstormInteractables"/> dictionary that do not have a <see cref="MSInteractableDirectorCard"/>
        /// </summary>
        public static InteractableBase[] InteractablesWithoutCards { get => MoonstormInteractables.Values.Where(ib => ib.InteractableDirectorCard == null).ToArray(); }
        /// <summary>
        /// Loads all the interactable game objects
        /// </summary>
        public static GameObject[] LoadedInteractables { get => MoonstormInteractables.Keys.ToArray(); }
        /// <summary>
        /// An action that gets invoked when the <see cref="MoonstormInteractables"/> dictionary has been populated
        /// </summary>
        [Obsolete("use \"moduleAvailability.CallWhenAvailable()\" instead")]
        public static Action<ReadOnlyDictionary<GameObject, InteractableBase>> OnDictionaryCreated;
        /// <summary>
        /// Call moduleAvailability.CallWhenAvailable() to run a method after the Module is initialized.
        /// </summary>
        public static ResourceAvailability moduleAvailability;
        private static Dictionary<DirectorAPI.Stage, List<MSInteractableDirectorCard>> currentStageToCards = new Dictionary<DirectorAPI.Stage, List<MSInteractableDirectorCard>>();
        private static Dictionary<string, List<MSInteractableDirectorCard>> currentCustomStageToCards = new Dictionary<string, List<MSInteractableDirectorCard>>(StringComparer.OrdinalIgnoreCase);
        #endregion

        [SystemInitializer]
        private static void SystemInit()
        {
            MSULog.Info($"Initializing Interactable Module...");
            Run.onRunStartGlobal += PopulateDictionaries;
            DirectorAPI.InteractableActions += AddCustomInteractables;

            MoonstormInteractables = new ReadOnlyDictionary<GameObject, InteractableBase>(interactables);
            interactables = null;

            OnDictionaryCreated?.Invoke(MoonstormInteractables);
            moduleAvailability.MakeAvailable();
        }


        #region Interactables
        /// <summary>
        /// <inheritdoc cref="ModuleBase{T}.GetContentClasses{T}(Type)"/>
        /// <para>T in this case is <see cref="InteractableBase"/></para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's <see cref="InteractableBase"/></returns>
        protected virtual IEnumerable<InteractableBase> GetInteractableBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Interactables found inside {GetType().Assembly}...");
#endif
            return GetContentClasses<InteractableBase>();
        }
        /// <summary>
        /// Adds an InteractableBase's InteractablePrefab to the game and to the ContentPack's NetworkedObjects array
        /// </summary>
        /// <param name="interactableBase">The InteractableBase to add</param>
        /// <param name="interactableDictionary">Optional, a dictionary to add your initialized InteractableBase and InteractablePrefab</param>
        protected void AddInteractable(InteractableBase interactableBase, Dictionary<GameObject, InteractableBase> interactableDictionary = null)
        {
            InitializeContent(interactableBase);
            interactableDictionary?.Add(interactableBase.Interactable, interactableBase);
#if DEBUG
            MSULog.Debug($"Interactable {interactableBase} Initialized and Ensured in {SerializableContentPack.name}");
#endif
        }

        /// <summary>
        /// Adds the InteractablePrefab of <paramref name="contentClass"/> to your mod's SerializableContentPack's NetworkedObjects array
        /// <para>Once added, it'll call <see cref="ContentBase.Initialize"/></para>
        /// </summary>
        /// <param name="contentClass">The content class being initialized</param>
        protected override void InitializeContent(InteractableBase contentClass)
        {
            AddSafely(ref SerializableContentPack.networkedObjectPrefabs, contentClass.Interactable, "NetworkedObjectPrefabs");
            contentClass.Initialize();
            interactables.Add(contentClass.Interactable, contentClass);
        }
        #endregion

        #region Hooks
        //When the run starts, MSU looks thru the interactable cards and adds them to the dictionaries, the cards in the dictionaries are the ones used during the run.
        private static void PopulateDictionaries(Run run)
        {
            ClearDictionaries();
            //Expansions enabled in this run
            ExpansionDef[] runExpansions = ExpansionCatalog.expansionDefs.Where(exp => run.IsExpansionEnabled(exp)).ToArray();
            MSInteractableDirectorCard[] cards = InteractablesWithCards.Select(ib => ib.InteractableDirectorCard).ToArray();

            int num = 0;
            foreach (MSInteractableDirectorCard card in cards)
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
                                    currentCustomStageToCards.Add(baseStageName, new List<MSInteractableDirectorCard>());
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
                                currentStageToCards.Add(stageValue, new List<MSInteractableDirectorCard>());
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
#if DEBUG
            MSULog.Info(num > 0 ? $"A total of {num} interactable cards added to the run" : $"No interactable cards added to the run");
#endif
        }

        private static void ClearDictionaries()
        {
            currentStageToCards.Clear();
            currentCustomStageToCards.Clear();
        }

        private static void AddCustomInteractables(DccsPool pool, DirectorAPI.StageInfo stageInfo)
        {
            try
            {
                List<MSInteractableDirectorCard> cards = new List<MSInteractableDirectorCard>();
                if (stageInfo.stage == DirectorAPI.Stage.Custom)
                {
                    if (currentCustomStageToCards.TryGetValue(stageInfo.CustomStageName, out cards))
                    {
                        AddCardsToPool(pool, cards);
                    }
                }
                else
                {
                    if (currentStageToCards.TryGetValue(stageInfo.stage, out cards))
                    {
                        AddCardsToPool(pool, cards);
                    }
                }

                //MSULog.Info(cards.Count > 0 ? $"Added a total of {cards.Count} interactable cards to stage {stageInfo.ToInternalStageName()}" : $"No interactable cards added to stage {stageInfo.ToInternalStageName()}");
            }
            catch (Exception e)
            {
                MSULog.Error($"Failed to add custom interactables: {e}\n(Pool: {pool}, Stage: {stageInfo}, currentCustomStageToCards: {currentCustomStageToCards}, currentStageToCards: {currentStageToCards}");
            }
        }

        private static void AddCardsToPool(DccsPool pool, List<MSInteractableDirectorCard> cards)
        {
            var alwaysIncluded = pool.poolCategories.SelectMany(pc => pc.alwaysIncluded.Select(pe => pe.dccs)).ToList();
            var includedIfConditionsMet = pool.poolCategories.SelectMany(pc => pc.includedIfConditionsMet.Select(cpe => cpe.dccs)).ToList();
            var includedIfNoConditions = pool.poolCategories.SelectMany(pc => pc.includedIfNoConditionsMet.Select(pe => pe.dccs)).ToList();

            List<DirectorCardCategorySelection> cardSelections = alwaysIncluded.Concat(includedIfConditionsMet).Concat(includedIfNoConditions).ToList();
            foreach (MSInteractableDirectorCard card in cards)
            {
                try
                {
                    foreach (DirectorCardCategorySelection cardCategorySelection in cardSelections)
                    {
                        cardCategorySelection.AddCard(card.DirectorCardHolder);
                    }
                }
                catch (Exception e)
                {
                    MSULog.Error($"{e}\n(Card: {card}");
                }
            }
        }
        #endregion
    }
}
