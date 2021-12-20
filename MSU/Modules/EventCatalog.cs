using HG;
using Moonstorm.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// Moonstorm Shared Utils's EventCatalog
    /// </summary>
    public static class EventCatalog
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

        /// <summary>
        /// Returns true if the amount of registered events is greater than 0, false otherwise
        /// </summary>
        public static bool HasAnyEventRegistered { get => loadedSceneDecks.Count > 0; }

        /// <summary>
        /// An array of all the event names
        /// </summary>
        public static string[] eventNames = Array.Empty<string>();

        /// <summary>
        /// A Dictionary to go from an event name to it's event index
        /// </summary>
        public static readonly Dictionary<string, EventIndex> eventNameToIndex = new Dictionary<string, EventIndex>();

        /// <summary>
        /// A Dictionary to go from an event index to the EventDirectorCard
        /// </summary>
        public static readonly Dictionary<EventIndex, EventDirectorCard> eventIndexToCard = new Dictionary<EventIndex, EventDirectorCard>();

        /// <summary>
        /// A Dictionary to go from a SceneDef to the EventDirectorCards that belong in said scene
        /// </summary>
        public static readonly Dictionary<SceneDef, EventDirectorCard[]> sceneToCards = new Dictionary<SceneDef, EventDirectorCard[]>();

        /// <summary>
        /// Adds the given EventSceneDecks to the EventCatalog
        /// </summary>
        /// <param name="decks">An Array of EventSceneDecks</param>
        public static void AddEventDecks(EventSceneDeck[] decks)
        {
            foreach (var newDeck in decks)
                AddEventDeck(newDeck);
        }

        /// <summary>
        /// Adds the given EventSceneDeck to the EventCatalog
        /// </summary>
        /// <param name="newDeck">The EventSceneDeck to add</param>
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

        /// <summary>
        /// Gets the current stage's events
        /// </summary>
        /// <returns>Returns the current stage's EventCardDeck</returns>
        public static EventCardDeck GetCurrentStageEvents()
        {
            return GetStageEvents(SceneInfo.instance.sceneDef);
        }

        /// <summary>
        /// Get a stage's events
        /// </summary>
        /// <param name="scene">The scene to get the events</param>
        /// <returns>Returns the given scene's EventCardDeck</returns>
        public static EventCardDeck GetStageEvents(SceneDef scene)
        {
            return new EventCardDeck
            {
                eventCards = sceneToCards[scene]
            };
        }

        /// <summary>
        /// Tries to find a director card by its name
        /// </summary>
        /// <param name="cardName">The name of the card</param>
        /// <param name="card">The card itself, if not found, it'll be null</param>
        /// <returns>True if succesfully found, false otherwise</returns>
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

        /// <summary>
        /// Tries to find a director card by using the EventIndex
        /// </summary>
        /// <param name="eventIndex">The EventIndex</param>
        /// <param name="card">The card that has the given EventIndex, if no card is found, this will be null</param>
        /// <returns>true if succesful, false otherwise</returns>
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

    }

}

