using HG;
using Moonstorm.ScriptableObjects;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using UnityEngine;
using static Moonstorm.EventDirectorCategorySelection;

namespace Moonstorm
{
    public static class EventCatalog
    {
        public static int RegisteredCategoriesCount => registeredCategories.Length;
        public static int RegisteredEventCount => registeredEventCards.Length;

        private static EventDirectorCategorySelection[] categories = Array.Empty<EventDirectorCategorySelection>();
        private static EventDirectorCategorySelection[] registeredCategories = Array.Empty<EventDirectorCategorySelection>();

        private static EventCard[] eventCards = Array.Empty<EventCard>();
        private static EventCard[] registeredEventCards = Array.Empty<EventCard>();

        private static readonly Dictionary<string, EventIndex> nameToEventIndex = new Dictionary<string, EventIndex>();
        private static readonly Dictionary<string, int> nameToCategoryIndex = new Dictionary<string, int>();

        private static readonly Dictionary<DirectorAPI.Stage, EventDirectorCategorySelection> stageToCategory = new Dictionary<DirectorAPI.Stage, EventDirectorCategorySelection>();
        private static readonly Dictionary<string, EventDirectorCategorySelection> baseSceneNameToCategory = new Dictionary<string, EventDirectorCategorySelection>();

        private static bool initialized = false;
        #region GetAndFindMethods
        public static EventIndex FindEventIndex(string eventName)
        {
            if (!initialized)
                throw new InvalidOperationException($"EventCatalog not initialized");

            if(nameToEventIndex.TryGetValue(eventName, out EventIndex eventIndex))
            {
                return eventIndex;
            }
            return EventIndex.None;
        }
        public static EventCard GetEventCard(EventIndex eventIndex)
        {
            if (!initialized)
                throw new InvalidOperationException($"EventCatalog not initialized");

            return ArrayUtils.GetSafe(registeredEventCards, (int)eventIndex);
        }

        public static int FindCategorySelectionIndex(string name)
        {
            if (!initialized)
                throw new InvalidOperationException($"EventCatalog not initialized");

            if(nameToCategoryIndex.TryGetValue(name, out var indeex))
            {
                return indeex;
            }
            return -1;
        }
        public static EventDirectorCategorySelection GetCategorySelection(int categoryIndex)
        {
            if (!initialized)
                throw new InvalidOperationException($"EventCatalog not initialized");

            return ArrayUtils.GetSafe(registeredCategories, categoryIndex);
        }

        public static EventDirectorCategorySelection GetCategoryFromCurrentStage(DirectorAPI.Stage stage, string customName = null)
        {
            try
            {
                if (!initialized)
                    throw new InvalidOperationException($"EventCatalog not initialized");

                if(stage == DirectorAPI.Stage.Custom)
                {
                    if (string.IsNullOrEmpty(customName))
                        return null;

                    if(baseSceneNameToCategory.TryGetValue(customName, out var cat1))
                    {
                        return cat1;
                    }
                    return null;
                }
                if(stageToCategory.TryGetValue(stage, out var cat2))
                {
                    return cat2;
                }
                return null;
            }
            catch(Exception ex)
            {
                MSULog.Error(ex);
                return null;
            }
        }

        public static EventDirectorCategorySelection GetCategoryFromSceneDef(SceneDef sceneDef)
        {
            try
            {
                if (!initialized)
                    throw new InvalidOperationException($"EventCatalog not initialized");

                DirectorAPI.Stage stage = DirectorAPI.GetStageEnumFromSceneDef(sceneDef);
                if(stage == DirectorAPI.Stage.Custom)
                {
                    string key = sceneDef.baseSceneName;
                    return baseSceneNameToCategory[key];
                }
                return stageToCategory[stage];
            }
            catch (Exception ex)
            {
                MSULog.Error(ex);
                return null;
            }
        }
        #endregion

        #region Add Methods
        public static void AddCards(EventCard[] cards)
        {
            if (initialized)
                throw new InvalidOperationException($"EventCatalog already initialized");

            cards.ToList().ForEach(card => AddCard(card));
        }

        public static void AddCard(EventCard card)
        {
            if (initialized)
                throw new InvalidOperationException($"EventCatalog already initialized");
            HG.ArrayUtils.ArrayAppend(ref eventCards, card);
        }

        public static void AddCategories(EventDirectorCategorySelection[] categories) 
        {
            if (initialized)
                throw new InvalidOperationException($"EventCatalog already initialized");
            categories.ToList().ForEach(category => AddCategory(category));
        }
        public static void AddCategory(EventDirectorCategorySelection category) 
        {
            if (initialized)
                throw new InvalidOperationException($"EventCatalog already initialized");
            HG.ArrayUtils.ArrayAppend(ref categories, category);
        }
        #endregion

        #region Internal Methods
        [SystemInitializer(typeof(SceneCatalog), typeof(EntityStateCatalog))]
        private static void Init()
        {
            AddCategories(MoonstormSharedUtils.MSUAssetBundle.LoadAllAssets<EventDirectorCategorySelection>());

            nameToEventIndex.Clear();
            stageToCategory.Clear();
            baseSceneNameToCategory.Clear();

            TurnStringsInCardsAndCategoriesToLowercase();

            categories = categories.OrderBy(so => so.name).ToArray();
            eventCards = eventCards.OrderBy(so => so.name).ToArray();

            registeredCategories = RegisterCategories(categories).ToArray();
            categories = null;

            registeredEventCards = RegisterEventCards(eventCards).ToArray();
            eventCards = null;

            initialized = true;
        }

        private static void TurnStringsInCardsAndCategoriesToLowercase()
        {
            foreach(EventDirectorCategorySelection category in categories)
            {
                category.stageName = category.stageName.ToLowerInvariant();
                for(int i = 0; i < category.categories.Length; i++)
                {
                    EventCategory cat = category.categories[i];
                    cat.categoryName = cat.categoryName.ToLowerInvariant();
                    category.categories[i] = cat;
                }
            }

            foreach(EventCard card in eventCards)
            {
                card.category = card.category.ToLowerInvariant();
                for(int i = 0; i < card.customStageNames.Count; i++)
                {
                    var customStageName = card.customStageNames[i];
                    customStageName = customStageName.ToLowerInvariant();
                }
            }
        }

        private static List<EventDirectorCategorySelection> RegisterCategories(EventDirectorCategorySelection[] eventDirectorCategorySelections)
        {
            List<EventDirectorCategorySelection> validCategories = new List<EventDirectorCategorySelection>();
            for (int i = 0; i < eventDirectorCategorySelections.Length; i++)
            {
                try
                {
                    EventDirectorCategorySelection category = eventDirectorCategorySelections[i];
                    bool? isCustom = null;

                    if (HasMultipleFlags(category.stage))
                    {
                        MSULog.Warning($"Cannot add category {category.name}, Since it's \"Stage\" enum has multiple flags set!" +
                            $"An EventDirectorCategorySelection can only have 1 enum value!" +
                            $"(Enum values: {category.stage})");
                        continue;
                    }

                    //Categories for custom stages check the baseSceneNameToCategory, all other enums check the stageToCategory dictionary.
                    if (category.stage == DirectorAPI.Stage.Custom)
                    {
                        isCustom = true;
                        if (baseSceneNameToCategory.ContainsKey(category.stageName))
                        {
                            MSULog.Warning($"Cannot add category {category.name}, since an already registered category uses the stageName {category.stageName}" +
                                $"Already registereed category: {baseSceneNameToCategory[category.stageName].name}");
                            continue;
                        }
                    }
                    else
                    {
                        isCustom = false;
                        if(stageToCategory.ContainsKey(category.stage))
                        {
                            MSULog.Warning($"Cannot add category {category.name}, since an already registered category uses the stage enum {category.stage}!" +
                                $"Already registered category: {stageToCategory[category.stage].name}");
                            continue;
                        }
                    }

                    if (!IsCategorySelectionValid(category))
                        continue;

                    if (isCustom == null)
                    {
                        MSULog.Warning($"Could not check if the category {category.name} is for a vanilla stage or a custom stage!");
                        continue;
                    }

                    if (isCustom == true)
                        baseSceneNameToCategory.Add(category.stageName, category);
                    else if (isCustom == false)
                        stageToCategory.Add(category.stage, category);

                    nameToCategoryIndex.Add(category.name, i);
                    validCategories.Add(category);
                }
                catch (Exception ex)
                {
                    MSULog.Error(ex);
                }
            }
            return validCategories;
        }


        private static List<EventCard> RegisterEventCards(EventCard[] eventCards)
        {
            List<EventCard> validCards = new List<EventCard>();
            for (int i = 0; i < eventCards.Length; i++)
            {
                try
                {
                    EventCard card = eventCards[i];
                
                    if(card.selectionWeight <= 0)
                    {
                        MSULog.Warning($"Cannot add EventCard {card.name} because it's selectionWeight is less or equal to 0!" +
                            $"(Weight: {card.selectionWeight})");
                        continue;
                    }

                    if(card.cost <= 0)
                    {
                        MSULog.Warning($"Cannot add EventCard {card.name} because it's cost is less or equal to 0!" +
                            $"(Cost: {card.cost})");
                        continue;
                    }

                    if(card.eventState.stateType == null)
                    {
                        MSULog.Warning($"Cannot add EventCard {card.name} because it's eventState is null!");
                        continue;
                    }

                    if(!card.eventState.stateType.IsSubclassOf(typeof(EntityStates.Events.EventState)))
                    {
                        MSULog.Warning($"Cannot add EventCard {card.name} because it's eventState does not inherit from \"EntityStates.Events.EventState\"!" +
                            $"(State Type: {card.eventState.stateType}");
                        continue;
                    }

                    validCards.Add(card);
                }
                catch(Exception ex)
                {
                    MSULog.Error(ex);
                }
            }

            int cardAmount = validCards.ToArray().Length;
            for(EventIndex eventIndex = (EventIndex)0; (int)eventIndex < cardAmount; eventIndex++)
            {
                try
                {
                    RegisterCard(validCards[(int)eventIndex], eventIndex);
                    AddCardToCategories(validCards[(int)eventIndex]);
                }
                catch(Exception ex)
                {
                    MSULog.Error(ex);
                }
            }
            return validCards;
        }

        private static void RegisterCard(EventCard card, EventIndex index)
        {
            card.EventIndex = index;
            nameToEventIndex.Add(card.name, index);
        }

        private static void AddCardToCategories(EventCard card)
        {
            foreach(DirectorAPI.Stage stage in Enum.GetValues(typeof(DirectorAPI.Stage)))
            {
                if(card.availableStages.HasFlag(stage))
                {
                    if(stage == DirectorAPI.Stage.Custom)
                    {
                        AddCardToCustomStages(card);
                        continue;
                    }
                    AddCardToStage(card, stage);
                }
            }
        }

        private static void AddCardToCustomStages(EventCard card)
        {
            foreach(string customStageName in card.customStageNames)
            {
                if(baseSceneNameToCategory.TryGetValue(customStageName, out var category))
                {
                    var categoryIndex = category.FindCategoryIndexByName(card.category);

                    if(categoryIndex == -1)
                    {
                        MSULog.Warning($"Cannot add card {card.name}, because the EventCategory specified ({card.category}) does not exist in the EventDirectorCategorySelection {category.name}!" +
                            $"Valid categories for {category.name}:" +
                            string.Join(", ", category.categories.Select(ctg => ctg.categoryName)));
                        continue;
                    }

                    category.AddCard(categoryIndex, card);
                    continue;
                }
                MSULog.Warning($"Cannot add card {card.name}, because it has the flag stage \"Custom\", but the catalog has no EventDirectorCategorySelection for stage {customStageName}!");
            }
        }

        private static void AddCardToStage(EventCard card, DirectorAPI.Stage stage)
        {
            if (stageToCategory.TryGetValue(stage, out var category))
            {
                var categoryIndex = category.FindCategoryIndexByName(card.category);

                if (categoryIndex == -1)
                {
                    MSULog.Warning($"Cannot add card {card.name}, because the EventCategory specified ({card.category}) does not exist in the EventDirectorCategorySelection {category.name}!" +
                        $"Valid categories for {category.name}:" +
                        string.Join(", ", category.categories.Select(ctg => ctg.categoryName)));
                    return;
                }

                category.AddCard(categoryIndex, card);
                return;
            }
            MSULog.Warning($"Cannot add card {card.name}, because it has the flag \"{Enum.GetName(typeof(DirectorAPI.Stage), stage)}\", but the catalog has no EventDirectorCategorySelection for that stage!");
        }

        private static bool IsCategorySelectionValid(EventDirectorCategorySelection categorySelection)
        {
            for (int i = 0; i < categorySelection.categories.Length; i++)
            {
                EventCategory category = categorySelection.categories[i];
                if (category.selectionWeight <= 0f)
                {
                    MSULog.Error($"{category.categoryName} in {categorySelection} has no weight!");
                    return false;
                }
            }
            return true;
        }


        private static bool HasMultipleFlags(DirectorAPI.Stage stage)
        {
            //-1 means that the enum has "None" flags, while any number above 0 means it has multiple, so if the flagAmount is 0 (Only one flag) then its valid.
            int flagAmount = -1;
            var enumValues = Enum.GetValues(typeof(DirectorAPI.Stage));

            foreach(DirectorAPI.Stage enumValue in enumValues)
            {
                if (stage.HasFlag(enumValue))
                    flagAmount++;
            }

            return flagAmount <= -1 || flagAmount >= 1;
        }

        [ConCommand(commandName = "list_events", flags = ConVarFlags.None, helpText = "Prints all loaded events")]
        private static void ListEvents(ConCommandArgs args)
        {
            for (int i = 0; i < RegisteredEventCount; i++)
                Debug.Log($"[{i}]\t{registeredEventCards[i].name}");
        }
        #endregion
    }
    /*public static class EventCatalog
    {
        private static readonly List<EventSceneDeck> loadedSceneDecks = new List<EventSceneDeck>();

        //Returns the amount of available events
        public static int eventCount
        {
            get
            {
                return eventCards.Length;
            }
        }

        public static bool HasAnyEventRegistered { get => loadedSceneDecks.Count > 0; }

        public static string[] eventNames = Array.Empty<string>();

        public static readonly Dictionary<string, EventIndex> eventNameToIndex = new Dictionary<string, EventIndex>();

        public static readonly Dictionary<EventIndex, EventDirectorCard> eventIndexToCard = new Dictionary<EventIndex, EventDirectorCard>();

        public static readonly Dictionary<SceneDef, EventDirectorCard[]> sceneToCards = new Dictionary<SceneDef, EventDirectorCard[]>();

        public static void AddEventDecks(EventSceneDeck[] decks)
        {
            foreach (var newDeck in decks)
                AddEventDeck(newDeck);
        }

        public static void AddEventDeck(EventSceneDeck newDeck)
        {
            loadedSceneDecks.Add(newDeck);
        }

        [SystemInitializer(dependencies: typeof(SceneCatalog))]
        private static void Init()
        {
            var cardCollection = new List<EventDirectorCard>();
            foreach (var sceneDef in SceneCatalog.allStageSceneDefs)
            {
                var sceneCards = Array.Empty<EventDirectorCard>();

                var validSceneDecks = loadedSceneDecks.Where(sceneDeck =>
                {
                    var sceneName = sceneDeck.sceneName.ToLower();

                    if (sceneName == "all" || SceneCatalog.GetSceneDefFromSceneName(sceneName) == sceneDef)
                        return true;
                    return false;
                });
                sceneCards = validSceneDecks.SelectMany(deck => deck.sceneDeck.eventCards)
                                            .Distinct()
                                            .ToArray();

                cardCollection.AddRange(sceneCards);
                sceneToCards.Add(sceneDef, sceneCards);
            }
            SetEventCards(cardCollection.Distinct().ToArray());
        }

        private static void SetEventCards(EventDirectorCard[] newEvents)
        {
            eventCards = newEvents;
            eventNames = eventCards.Select(card => card.identifier).ToArray();
            Array.Sort(eventNames, eventCards, StringComparer.Ordinal);

            for (EventIndex eventIndex = 0; eventIndex < (EventIndex)eventCards.Length; eventIndex++)
            {
                var eventCard = eventCards[(int)eventIndex];
                string key = eventNames[(int)eventIndex];
                eventCard.EventIndex = eventIndex;
                eventNameToIndex.Add(key, eventIndex);
                eventIndexToCard.Add(eventIndex, eventCard);
            }
        }

        public static EventCardDeck GetCurrentStageEvents()
        {
            return GetStageEvents(SceneInfo.instance.sceneDef);
        }

        public static EventCardDeck GetStageEvents(SceneDef scene)
        {
            return new EventCardDeck
            {
                eventCards = sceneToCards[scene]
            };
        }

        public static bool TryFindDirectorCard(string cardName, out EventDirectorCard card)
        {
            string name = cardName.ToLower();
            EventIndex eventIndex = EventIndex.None;
            //If the name matches exactly
            if (eventNameToIndex.TryGetValue(name, out eventIndex))
                return TryFindDirectorCard(eventIndex, out card);
            //If if it is a number
            if (int.TryParse(name, out var index) && index < eventCount)
                return TryFindDirectorCard((EventIndex)index, out card);


            //If the name is impartial or the case doesn't match
            eventIndex = eventNameToIndex.Where(keyValuePair => keyValuePair.Key.ToLower().Contains(name))
                                         .OrderBy(keyValuePair => keyValuePair.Key.ToLower().IndexOf(name))
                                         .Select(keyValuePair => keyValuePair.Value)
                                         .DefaultIfEmpty(EventIndex.None)
                                         .First();
            if (eventIndex != EventIndex.None && (int)eventIndex < eventCount)
            {
                card = eventIndexToCard[eventIndex];
                return true;
            }

            card = null;
            return false;
        }

        public static bool TryFindDirectorCard(EventIndex eventIndex, out EventDirectorCard card)
        {
            return eventIndexToCard.TryGetValue(eventIndex, out card);
        }

        [ConCommand(commandName = "list_events", flags = ConVarFlags.None, helpText = "Prints all loaded events")]
        private static void ListEvents(ConCommandArgs args)
        {
            for (int i = 0; i < eventCount; i++)
                Debug.Log($"[{i}]\t{eventNames[i]}");
        }

        [ConCommand(commandName = "list_scene_events", flags = ConVarFlags.None, helpText = "Prints all loaded events for this scene")]
        private static void ListSceneEvents(ConCommandArgs args)
        {
            for (int i = 0; i < eventCount; i++)
                Debug.Log($"[{i}]\t{eventNames[i]}");
        }


        private static EventDirectorCard[] eventCards = Array.Empty<EventDirectorCard>();
    }


    public enum EventIndex
    {
        None = -1
    }

    [Serializable]
    public struct EventCardDeck
    {
        public EventDirectorCard[] eventCards;

        public WeightedSelection<EventDirectorCard> GenerateDirectorCardWeightedSelection()
        {
            if (eventCards.Length > 0)
            {
                WeightedSelection<EventDirectorCard> weightedSelection = new WeightedSelection<EventDirectorCard>(8);
                foreach (EventDirectorCard directorCard in eventCards)
                {
                    if (directorCard.CardIsValid())
                        weightedSelection.AddChoice(directorCard, (float)directorCard.selectionWeight);
                }
                return weightedSelection;
            }
            return null;
        }

        public void RemoveCardsThatFailFilter(Predicate<EventDirectorCard> predicate)
        {
            for (int i = eventCards.Length - 1; i >= 0; i--)
            {
                EventDirectorCard obj = eventCards[i];
                if (!predicate(obj))
                {
                    ArrayUtils.ArrayRemoveAtAndResize(ref eventCards, i, 1);
                }
            }
        }

        public void ValidateCards()
        {
            eventCards = eventCards.Where(card => card.CardIsValid()).ToArray();
        }

        public void Clear()
        {
            eventCards = Array.Empty<EventDirectorCard>();
        }

    }*/

}

