using HG;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Moonstorm.EventDirectorCategorySelection;

namespace Moonstorm
{
    /// <summary>
    /// The Event Catalog for MSU's Event System
    /// </summary>
    public static class EventCatalog
    {
        /// <summary>
        /// Returns the amount of Categories that are registered
        /// </summary>
        public static int RegisteredCategoriesCount => registeredCategories.Length;
        /// <summary>
        /// Returns the amount of Events that are registered.
        /// </summary>
        public static int RegisteredEventCount => registeredEventCards.Length;

        private static EventDirectorCategorySelection[] categories = Array.Empty<EventDirectorCategorySelection>();
        private static EventDirectorCategorySelection[] registeredCategories = Array.Empty<EventDirectorCategorySelection>();

        private static EventCard[] eventCards = Array.Empty<EventCard>();
        private static EventCard[] registeredEventCards = Array.Empty<EventCard>();

        private static readonly Dictionary<string, EventIndex> nameToEventIndex = new Dictionary<string, EventIndex>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, int> nameToCategoryIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<DirectorAPI.Stage, EventDirectorCategorySelection> stageToCategory = new Dictionary<DirectorAPI.Stage, EventDirectorCategorySelection>();
        private static readonly Dictionary<string, EventDirectorCategorySelection> baseSceneNameToCategory = new Dictionary<string, EventDirectorCategorySelection>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Call ResourceAvailability.CallWhenAvailable() to run a method after the EventCatalog is initialized
        /// </summary>
        public static ResourceAvailability resourceAvailability = default(ResourceAvailability);
        private static bool initialized = false;
        #region GetAndFindMethods
        /// <summary>
        /// Returns the EventIndex tied to the string supplied in <paramref name="eventName"/>
        /// </summary>
        /// <param name="eventName">The name of this event</param>
        /// <returns>An EventIndex if valid, <see cref="EventIndex.None"/> if no event matches.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static EventIndex FindEventIndex(string eventName)
        {
            if (!initialized)
                throw new InvalidOperationException($"EventCatalog not initialized");

            if (nameToEventIndex.TryGetValue(eventName.ToLowerInvariant(), out EventIndex eventIndex))
            {
                return eventIndex;
            }
            return EventIndex.None;
        }

        /// <summary>
        /// Returns the EventCard that's tied to the supplied EventIndex in <paramref name="eventIndex"/>
        /// </summary>
        /// <param name="eventIndex">The EventIndex</param>
        /// <returns>An event card that's tied to <paramref name="eventIndex"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static EventCard GetEventCard(EventIndex eventIndex)
        {
            if (!initialized)
                throw new InvalidOperationException($"EventCatalog not initialized");

            return ArrayUtils.GetSafe(registeredEventCards, (int)eventIndex);
        }

        /// <summary>
        /// Returns the index that's tied to a CategorySelection's name by using <paramref name="name"/>
        /// </summary>
        /// <param name="name">The name of the category to find</param>
        /// <returns>The index of the category, returns -1 if no category is found.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static int FindCategorySelectionIndex(string name)
        {
            if (!initialized)
                throw new InvalidOperationException($"EventCatalog not initialized");

            if (nameToCategoryIndex.TryGetValue(name.ToLowerInvariant(), out var index))
            {
                return index;
            }
            return -1;
        }

        /// <summary>
        /// Returns the EventDirectorCategorySelection tied to the index provided in <paramref name="categoryIndex"/>
        /// </summary>
        /// <param name="categoryIndex">The index to use for the lookup</param>
        /// <returns>The EventDirectorCategorySelection</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static EventDirectorCategorySelection GetCategorySelection(int categoryIndex)
        {
            if (!initialized)
                throw new InvalidOperationException($"EventCatalog not initialized");

            return ArrayUtils.GetSafe(registeredCategories, categoryIndex);
        }

        /// <summary>
        /// Returns the EventDirectorCategorySelection that's tied to the given <paramref name="stage"/> or the given <paramref name="customName"/>
        /// </summary>
        /// <param name="stage">The Stage to obtain</param>
        /// <param name="customName">A custom stage's name, <paramref name="stage"/> needs to be set to <see cref="DirectorAPI.Stage.Custom"/> for it to work properly</param>
        /// <returns>The EventDirectorCategorySelection</returns>
        public static EventDirectorCategorySelection GetCategoryFromCurrentStage(DirectorAPI.Stage stage, string customName = null)
        {
            try
            {
                if (!initialized)
                    throw new InvalidOperationException($"EventCatalog not initialized");

                if (stage == DirectorAPI.Stage.Custom)
                {
                    if (string.IsNullOrEmpty(customName))
                        return null;

                    customName = customName.ToLowerInvariant();
                    if (baseSceneNameToCategory.TryGetValue(customName.ToLowerInvariant(), out var cat1))
                    {
                        return cat1;
                    }
                    return null;
                }
                if (stageToCategory.TryGetValue(stage, out var cat2))
                {
                    return cat2;
                }
                return null;
            }
            catch (Exception ex)
            {
                MSULog.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Returns the EventDirectorCategorySelection that's tied to <paramref name="sceneDef"/>.
        /// </summary>
        /// <param name="sceneDef">The scene def to use for the lookup</param>
        /// <returns>The EventDirectorCategorySelection</returns>
        public static EventDirectorCategorySelection GetCategoryFromSceneDef(SceneDef sceneDef)
        {
            try
            {
                if (!initialized)
                    throw new InvalidOperationException($"EventCatalog not initialized");

                DirectorAPI.Stage stage = DirectorAPI.GetStageEnumFromSceneDef(sceneDef);
                if (stage == DirectorAPI.Stage.Custom)
                {
                    string key = sceneDef.baseSceneName.ToLowerInvariant();
                    if (baseSceneNameToCategory.TryGetValue(key, out var selection))
                    {
                        return selection;
                    }
                    MSULog.Warning($"Scene {sceneDef} not found in the baseSceneNameToCategory dictionary.");
                    return null;
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
        /// <summary>
        /// Adds multiple cards to the EventCatalog
        /// </summary>
        /// <param name="cards">The cards to addd</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void AddCards(EventCard[] cards)
        {
            if (initialized)
                throw new InvalidOperationException($"EventCatalog already initialized");

            cards.ToList().ForEach(card => AddCard(card));
        }

        /// <summary>
        /// Adds a single card to the EventCatalog
        /// </summary>
        /// <param name="card">The card to add</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void AddCard(EventCard card)
        {
            if (initialized)
                throw new InvalidOperationException($"EventCatalog already initialized");
            HG.ArrayUtils.ArrayAppend(ref eventCards, card);
        }

        /// <summary>
        /// Adds multiple EventDirectorCategorySelection to the EventCatalog
        /// </summary>
        /// <param name="categories">The categories to add</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void AddCategories(EventDirectorCategorySelection[] categories)
        {
            if (initialized)
                throw new InvalidOperationException($"EventCatalog already initialized");
            categories.ToList().ForEach(category => AddCategory(category));
        }
        /// <summary>
        /// Adds a single EventDirectorCategorySelection to the EventCatalog
        /// </summary>
        /// <param name="category">The category to add</param>
        /// <exception cref="InvalidOperationException"></exception>
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
#if DEBUG
            if (MSUConfig.addDummyEvent)
            {
                AddCard(MoonstormSharedUtils.MSUAssetBundle.LoadAsset<EventCard>("DummyEventCard"));
            }
#endif

            nameToEventIndex.Clear();
            stageToCategory.Clear();
            baseSceneNameToCategory.Clear();

            categories = categories.OrderBy(so => so.name).ToArray();
            eventCards = eventCards.OrderBy(so => so.name).ToArray();

            registeredCategories = RegisterCategories(categories).ToArray();
            categories = null;

            registeredEventCards = RegisterEventCards(eventCards).ToArray();
            eventCards = null;

            initialized = true;
            resourceAvailability.MakeAvailable();
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
#if DEBUG
                        MSULog.Warning($"Cannot add category {category.name}, Since it's \"Stage\" enum has multiple flags set!" +
                            $"An EventDirectorCategorySelection can only have 1 enum value!" +
                            $"(Enum values: {category.stage})");
#endif
                        continue;
                    }

                    //Categories for custom stages check the baseSceneNameToCategory, all other enums check the stageToCategory dictionary.
                    if (category.stage == DirectorAPI.Stage.Custom)
                    {
                        isCustom = true;
                        if (baseSceneNameToCategory.ContainsKey(category.stageName.ToLowerInvariant()))
                        {
#if DEBUG
                            MSULog.Warning($"Cannot add category {category.name}, since an already registered category uses the stageName {category.stageName}" +
                                $"Already registereed category: {baseSceneNameToCategory[category.stageName].name}");
#endif
                            continue;
                        }
                    }
                    else
                    {
                        isCustom = false;
                        if (stageToCategory.ContainsKey(category.stage))
                        {
#if DEBUG
                            MSULog.Warning($"Cannot add category {category.name}, since an already registered category uses the stage enum {category.stage}!" +
                                $"Already registered category: {stageToCategory[category.stage].name}");
#endif
                            continue;
                        }
                    }

                    if (!IsCategorySelectionValid(category))
                        continue;

                    if (isCustom == null)
                    {
#if DEBUG
                        MSULog.Warning($"Could not check if the category {category.name} is for a vanilla stage or a custom stage!");
#endif
                        continue;
                    }

                    if (isCustom == true)
                        baseSceneNameToCategory.Add(category.stageName.ToLowerInvariant(), category);
                    else if (isCustom == false)
                        stageToCategory.Add(category.stage, category);

                    nameToCategoryIndex.Add(category.name.ToLowerInvariant(), i);
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

                    if (card.selectionWeight <= 0)
                    {
#if DEBUG
                        MSULog.Warning($"Cannot add EventCard {card.name} because it's selectionWeight is less or equal to 0!" +
                            $"(Weight: {card.selectionWeight})");
#endif
                        continue;
                    }

                    if (card.cost <= 0)
                    {
#if DEBUG
                        MSULog.Warning($"Cannot add EventCard {card.name} because it's cost is less or equal to 0!" +
                            $"(Cost: {card.cost})");
#endif
                        continue;
                    }

                    if (card.eventState.stateType == null)
                    {
#if DEBUG
                        MSULog.Warning($"Cannot add EventCard {card.name} because it's eventState is null!");
#endif
                        continue;
                    }

                    if (!card.eventState.stateType.IsSubclassOf(typeof(EntityStates.Events.EventState)))
                    {
#if DEBUG
                        MSULog.Warning($"Cannot add EventCard {card.name} because it's eventState does not inherit from \"EntityStates.Events.EventState\"!" +
                            $"(State Type: {card.eventState.stateType}");
#endif
                        continue;
                    }

                    validCards.Add(card);
                }
                catch (Exception ex)
                {
                    MSULog.Error(ex);
                }
            }

            int cardAmount = validCards.ToArray().Length;
            for (EventIndex eventIndex = (EventIndex)0; (int)eventIndex < cardAmount; eventIndex++)
            {
                try
                {
                    RegisterCard(validCards[(int)eventIndex], eventIndex);
                    AddCardToCategories(validCards[(int)eventIndex]);
                }
                catch (Exception ex)
                {
                    MSULog.Error(ex);
                }
            }
            return validCards;
        }

        private static void RegisterCard(EventCard card, EventIndex index)
        {
            card.EventIndex = index;
            nameToEventIndex.Add(card.name.ToLowerInvariant(), index);
        }

        private static void AddCardToCategories(EventCard card)
        {
            foreach (DirectorAPI.Stage stage in Enum.GetValues(typeof(DirectorAPI.Stage)))
            {
                if (card.availableStages.HasFlag(stage))
                {
                    if (stage == DirectorAPI.Stage.Custom)
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
            foreach (string customStageName in card.customStageNames)
            {
                if (baseSceneNameToCategory.TryGetValue(customStageName.ToLowerInvariant(), out var category))
                {
                    var categoryIndex = category.FindCategoryIndexByName(card.category.ToLowerInvariant());

                    if (categoryIndex == -1)
                    {
#if DEBUG
                        MSULog.Warning($"Cannot add card {card.name}, because the EventCategory specified ({card.category}) does not exist in the EventDirectorCategorySelection {category.name}!" +
                            $"Valid categories for {category.name}:" +
                            string.Join(", ", category.categories.Select(ctg => ctg.categoryName)));
#endif
                        continue;
                    }

                    category.AddCard(categoryIndex, card);
                    continue;
                }
#if DEBUG
                MSULog.Warning($"Cannot add card {card.name}, because it has the flag stage \"Custom\", but the catalog has no EventDirectorCategorySelection for stage {customStageName}!");
#endif
            }
        }

        private static void AddCardToStage(EventCard card, DirectorAPI.Stage stage)
        {
            if (stageToCategory.TryGetValue(stage, out var category))
            {
                var categoryIndex = category.FindCategoryIndexByName(card.category.ToLowerInvariant());

                if (categoryIndex == -1)
                {
#if DEBUG
                    MSULog.Warning($"Cannot add card {card.name}, because the EventCategory specified ({card.category}) does not exist in the EventDirectorCategorySelection {category.name}!" +
                        $"Valid categories for {category.name}:" +
                        string.Join(", ", category.categories.Select(ctg => ctg.categoryName)));
#endif
                    return;
                }

                category.AddCard(categoryIndex, card);
                return;
            }
#if DEBUG
            MSULog.Warning($"Cannot add card {card.name}, because it has the flag \"{Enum.GetName(typeof(DirectorAPI.Stage), stage)}\", but the catalog has no EventDirectorCategorySelection for that stage!");
#endif
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

            foreach (DirectorAPI.Stage enumValue in enumValues)
            {
                if (stage.HasFlag(enumValue))
                    flagAmount++;
            }

            return flagAmount <= -1 || flagAmount >= 1;
        }

#if DEBUG
        [ConCommand(commandName = "ms_list_events", flags = ConVarFlags.None, helpText = "Prints all loaded events")]
        private static void ListEvents(ConCommandArgs args)
        {
            for (int i = 0; i < RegisteredEventCount; i++)
                Debug.Log($"[{i}]\t{registeredEventCards[i].name.ToLowerInvariant()}");
        }
#endif
        #endregion
    }
}

