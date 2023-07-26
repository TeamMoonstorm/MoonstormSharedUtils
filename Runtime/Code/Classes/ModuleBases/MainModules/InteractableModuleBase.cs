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
        private class InteractableCollectionFuncPair
        {
            public struct Comparer : IEqualityComparer<InteractableCollectionFuncPair>
            {
                bool IEqualityComparer<InteractableCollectionFuncPair>.Equals(InteractableCollectionFuncPair x, InteractableCollectionFuncPair y)
                {
                    if (x == null || y == null)
                        return false;

                    if (x.tiedInteractableBase == null || y.tiedInteractableBase == null)
                        return false;

                    return x.tiedInteractableBase == y.tiedInteractableBase;
                }

                int IEqualityComparer<InteractableCollectionFuncPair>.GetHashCode(InteractableCollectionFuncPair obj)
                {
                    if (obj == null)
                        return -1;
                    if (obj.tiedInteractableBase == null)
                        return -1;
                    return obj.tiedInteractableBase.GetHashCode();
                }
            }

            public HashSet<MSInteractableDirectorCard> cards = new HashSet<MSInteractableDirectorCard>(new MSInteractableDirectorCard.PrefabComparer());
            public InteractableBase tiedInteractableBase;
            public InteractableBase.IsAvailableForDCCSDelegate IsAvailable => tiedInteractableBase.IsAvailableForDCCS;
        }
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
        public static InteractableBase[] InteractablesWithCards { get => MoonstormInteractables.Values.Where(ib => ib.InteractableDirectorCards.Count > 0).ToArray(); }
        /// <summary>
        /// Loads all the <see cref="InteractableBase"/> from the <see cref="MoonstormInteractables"/> dictionary that do not have a <see cref="MSInteractableDirectorCard"/>
        /// </summary>
        public static InteractableBase[] InteractablesWithoutCards { get => MoonstormInteractables.Values.Where(ib => ib.InteractableDirectorCards.Count == 0).ToArray(); }
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

        private static Dictionary<DirectorAPI.Stage, HashSet<InteractableCollectionFuncPair>> currentStageToCards = new Dictionary<DirectorAPI.Stage, HashSet<InteractableCollectionFuncPair>>();
        private static Dictionary<string, HashSet<InteractableCollectionFuncPair>> currentCustomStageToCards = new Dictionary<string, HashSet<InteractableCollectionFuncPair>>(StringComparer.OrdinalIgnoreCase);
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

#pragma warning disable CS0618 // Type or member is obsolete
            var cardObsolete = contentClass.InteractableDirectorCard;
#pragma warning restore CS0618 // Type or member is obsolete
            if (cardObsolete && !contentClass.InteractableDirectorCards.Contains(cardObsolete))
            {
                contentClass.InteractableDirectorCards.Add(cardObsolete);
            }
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
            InteractableBase[] interactableBases = InteractablesWithCards.ToArray();

            int num = 0;
            foreach(InteractableBase interactableBase in interactableBases)
            {
                AddInteractableBaseToRun(interactableBase, run, runExpansions, ref num);
            }
#if DEBUG
            MSULog.Info(num > 0 ? $"A total of {num} interactable cards added to the run" : $"No interactable cards added to the run");
#endif
        }

        private static void AddInteractableBaseToRun(InteractableBase interactableBase, Run run, ExpansionDef[] runExpansions, ref int totalInteractablesAdded)
        {
            foreach(MSInteractableDirectorCard card in interactableBase.InteractableDirectorCards)
            {
                try
                {
                    //If card cant appear, skip
                    if(!card.IsAvailable(runExpansions))
                    {
                        continue;
                    }

                    foreach(DirectorAPI.Stage stageValue in Enum.GetValues(typeof(DirectorAPI.Stage)))
                    {
                        if(stageValue == DirectorAPI.Stage.Custom && card.stages.HasFlag
                            (stageValue))
                        {
                            AddCardToCustomStages(card, interactableBase);
                            continue;
                        }

                        if(card.stages.HasFlag(stageValue))
                        {
                            AddCardToStage(card, interactableBase, stageValue);
                        }
                    }
                    totalInteractablesAdded++;
                }
                catch(Exception e)
                {
                    MSULog.Error($"{e}\nCard: {card}");
                }
            }
        }

        private static void AddCardToCustomStages(MSInteractableDirectorCard card, InteractableBase interactableBase)
        {
            foreach(string baseStageName in card.customStages)
            {
                //If the dictionary doesnt have an entry for this custom stage, create a new one alongside the list of monsters.
                if (!currentCustomStageToCards.ContainsKey(baseStageName))
                {
                    currentCustomStageToCards.Add(baseStageName, new HashSet<InteractableCollectionFuncPair>(new InteractableCollectionFuncPair.Comparer()));
                }

                HashSet<InteractableCollectionFuncPair> interactableCollectionSet = currentCustomStageToCards[baseStageName];
                //If there's no monster collection for this card's master prefab, create a new collection.
                InteractableCollectionFuncPair interactableCollection = FindInteractableCollection(interactableBase, interactableCollectionSet) ?? new InteractableCollectionFuncPair();

#if DEBUG
                if (interactableCollection.cards.Contains(card))
                {
                    MSULog.Warning($"There are two or more MSInteractableDirectorCard that are trying to add the same interactable prefab to the stage {baseStageName}. (Card that triggered this warning: {card}))");
                    continue;
                }
#endif
                //Add card to monster collection hash set
                interactableCollection.cards.Add(card);

                //Its a set, no neeed to check if the collection already exists.
                interactableCollectionSet.Add(interactableCollection);
            }
        }

        private static void AddCardToStage(MSInteractableDirectorCard card, InteractableBase monsterBase, DirectorAPI.Stage stageValue)
        {
            //If the dictionary doesnt have an entry for this stage, create a new one alongside the list of monsters.
            if (!currentStageToCards.ContainsKey(stageValue))
            {
                currentStageToCards[stageValue] = new HashSet<InteractableCollectionFuncPair>(new InteractableCollectionFuncPair.Comparer());
            }
            HashSet<InteractableCollectionFuncPair> interactableCollectionSet = currentStageToCards[stageValue];
            //If there's no monster collection for this card's master prefab, create a new collection.
            InteractableCollectionFuncPair interactableCollection = FindInteractableCollection(monsterBase, interactableCollectionSet) ?? new InteractableCollectionFuncPair();

#if DEBUG
            if (interactableCollection.cards.Contains(card))
            {
                MSULog.Warning($"There are two or more MSInteractableDirectorCards that are trying to add the same interactable  prefab to the stage {Enum.GetName(typeof(DirectorAPI.Stage), stageValue)}. (Card that triggered this warning: {card}))");
                return;
            }
#endif
            //Add card to monster collection hash set
            interactableCollection.cards.Add(card);

            //Its a set, no neeed to check if the collection already exists.
            interactableCollectionSet.Add(interactableCollection);
        }

        private static InteractableCollectionFuncPair FindInteractableCollection(InteractableBase monsterBase, HashSet<InteractableCollectionFuncPair> set)
        {
            foreach (InteractableCollectionFuncPair collection in set)
            {
                if (collection.tiedInteractableBase == monsterBase)
                {
                    return collection;
                }
            }
            return null;
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
                HashSet<InteractableCollectionFuncPair> interactables;
                if (stageInfo.stage == DirectorAPI.Stage.Custom)
                {
                    if (currentCustomStageToCards.TryGetValue(stageInfo.CustomStageName, out interactables))
                    {
                        AddCardsToPool(pool, interactables);
                    }
                }
                else
                {
                    if (currentStageToCards.TryGetValue(stageInfo.stage, out interactables))
                    {
                        AddCardsToPool(pool, interactables);
                    }
                }

                //MSULog.Info(cards.Count > 0 ? $"Added a total of {cards.Count} interactable cards to stage {stageInfo.ToInternalStageName()}" : $"No interactable cards added to stage {stageInfo.ToInternalStageName()}");
            }
            catch (Exception e)
            {
                MSULog.Error($"Failed to add custom interactables: {e}\n(Pool: {pool}, Stage: {stageInfo}, currentCustomStageToCards: {currentCustomStageToCards}, currentStageToCards: {currentStageToCards}");
            }
        }

        private static void AddCardsToPool(DccsPool pool, HashSet<InteractableCollectionFuncPair> interactables)
        {
            var alwaysIncluded = pool.poolCategories.SelectMany(pc => pc.alwaysIncluded.Select(pe => pe.dccs)).ToList();
            var includedIfConditionsMet = pool.poolCategories.SelectMany(pc => pc.includedIfConditionsMet.Select(cpe => cpe.dccs)).ToList();
            var includedIfNoConditions = pool.poolCategories.SelectMany(pc => pc.includedIfNoConditionsMet.Select(pe => pe.dccs)).ToList();

            List<DirectorCardCategorySelection> cardSelections = alwaysIncluded.Concat(includedIfConditionsMet).Concat(includedIfNoConditions).ToList();
            foreach (InteractableCollectionFuncPair collection in interactables)
            {
                if(!collection.IsAvailable())
                {
                    continue;
                }

                try
                {
                    foreach (DirectorCardCategorySelection cardCategorySelection in cardSelections)
                    {
                        foreach(var card in collection.cards)
                        {
                            cardCategorySelection.AddCard(card.DirectorCardHolder);
                        }
                    }
                }
                catch (Exception e)
                {
                    MSULog.Error($"{e}\n(Interactable Collection: {collection}");
                }
            }
        }
        #endregion
    }
}
