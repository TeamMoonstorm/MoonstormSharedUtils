using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// Represents an index for a <see cref="GameplayEvent"/>, these are assigned to different gameplay event prefabs at runtime.
    /// </summary>
    public enum GameplayEventIndex
    {
        /// <summary>
        /// Represents an invalid gameplay event index
        /// </summary>
        None = -1,
    }

    /// <summary>
    /// The <see cref="GameplayEventCatalog"/> is a catalog used for managing, adding and cataloguing <see cref="GameplayEvent"/> game objects to the game.
    /// </summary>
    public static class GameplayEventCatalog
    {
        /// <summary>
        /// Returns the total amount of registered gameplay events
        /// </summary>
        public static int registeredGameplayEventCount => _registeredGameplayEventObjects.Length;

        private static GameObject[] _gameplayEvents = Array.Empty<GameObject>();
        private static GameObject[] _registeredGameplayEventObjects = Array.Empty<GameObject>();
        private static GameplayEvent[] _registeredGameplayEventComponents = Array.Empty<GameplayEvent>();

        private static readonly Dictionary<string, GameplayEventIndex> _nameToEventIndex = new Dictionary<string, GameplayEventIndex>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Represents the availability for this Catalog, methods subscribed to this get called when the catalog finishes initializing
        /// </summary>
        public static ResourceAvailability catalogAvailability = default(ResourceAvailability);
        private static bool _initialized = false;

        #region Find/Get methods
        /// <summary>
        /// Finds a <see cref="GameplayEventIndex"/> with the name <paramref name="eventName"/> and returns it.
        /// <br>Throws an exception if the catalog has not been initialized.</br>
        /// </summary>
        /// <param name="eventName">The name of the event to find</param>
        /// <returns>A valid <see cref="GameplayEventIndex"/>, or <see cref="GameplayEventIndex.None"/> if no GameplayEvent is found.</returns>
        public static GameplayEventIndex FindEventIndex(string eventName)
        {
            ThrowIfNotInitialized();

            if (_nameToEventIndex.TryGetValue(eventName, out var index))
                return index;

            return GameplayEventIndex.None;
        }

        /// <summary>
        /// Retrieves the GameObject associated to the specified <see cref="GameplayEventIndex"/>.
        /// <br>Throws an exception if the catalog has not been initialized</br>
        /// </summary>
        /// <param name="index">The index of the event</param>
        /// <returns>The GameObject of the <see cref="GameplayEvent"/></returns>
        public static GameObject GetGameplayEventObject(GameplayEventIndex index)
        {
            ThrowIfNotInitialized();

            return HG.ArrayUtils.GetSafe(_registeredGameplayEventObjects, (int)index);
        }

        /// <summary>
        /// Retrieves the GameplayEvent component associated to the specified <see cref="GameplayEventIndex"/>
        /// <br>Throws an exception if the catalog has not been initialized</br>
        /// </summary>
        /// <param name="index">The index of the event</param>
        /// <returns>The GameplayEvent which index is equal to <paramref name="index"/></returns>
        public static GameplayEvent GetGameplayEvent(GameplayEventIndex index)
        {
            ThrowIfNotInitialized();

            return HG.ArrayUtils.GetSafe(_registeredGameplayEventComponents, (int)index);
        }
        #endregion

        #region Add Methods
        /// <summary>
        /// Adds all the <see cref="GameObject"/> found within <paramref name="gameplayEventGameObjects"/> to the <see cref="GameplayEventCatalog"/>.
        /// <br>Throws an exception if the catalog has already initialized.</br>
        /// </summary>
        /// <param name="gameplayEventGameObjects">The GameplayEvent GameObjects to add</param>
        public static void AddGameplayEvents(GameObject[] gameplayEventGameObjects)
        {
            ThrowIfInitialized();
            foreach (GameObject go in gameplayEventGameObjects)
            {
                AddGameplayEvent(go);
            }
        }

        /// <summary>
        /// Adds a single <paramref name="gameplayEventGameObject"/> to the <see cref="GameplayEventCatalog"/>
        /// <br>Throws an exception if the catalog has already initialized</br>
        /// </summary>
        /// <param name="gameplayEventGameObject">The GameplayEvent GameObject to add</param>
        public static void AddGameplayEvent(GameObject gameplayEventGameObject)
        {
            ThrowIfInitialized();

            if (!gameplayEventGameObject.TryGetComponent<GameplayEvent>(out var @event))
            {
#if DEBUG
                MSULog.Warning($"GameObject {gameplayEventGameObject} does not have a GameplayEvent component!");
#endif
                return;
            }

            HG.ArrayUtils.ArrayAppend(ref _gameplayEvents, gameplayEventGameObject);
        }
        #endregion

        #region Internal Methods
        [SystemInitializer]
        private static void Init()
        {
            _nameToEventIndex.Clear();

            _gameplayEvents = _gameplayEvents.OrderBy(go => go.name).ToArray();

            _registeredGameplayEventObjects = RegisterGameplayEvents().ToArray();
            _registeredGameplayEventComponents = _registeredGameplayEventObjects.Select(g => g.GetComponent<GameplayEvent>()).ToArray();
            _gameplayEvents = null;

            _initialized = true;
            catalogAvailability.MakeAvailable();
        }

        private static List<GameObject> RegisterGameplayEvents()
        {
            List<GameObject> validEvents = new List<GameObject>();
            for (int i = 0; i < _gameplayEvents.Length; i++)
            {
                try
                {
                    RegisterGameplayEvent(_gameplayEvents[i], validEvents);
                }
                catch (Exception e)
                {
                    MSULog.Error(e);
                }
            }

            for (int i = 0; i < validEvents.Count; i++)
            {
                var gameplayEventObject = validEvents[i];
                var gameplayEventComponent = gameplayEventObject.GetComponent<GameplayEvent>();

                gameplayEventComponent.gameplayEventIndex = (GameplayEventIndex)i;
                _nameToEventIndex.Add(gameplayEventObject.name, gameplayEventComponent.gameplayEventIndex);
            }
            return validEvents;
        }

        private static void RegisterGameplayEvent(GameObject gameplayEvent, List<GameObject> validEvents)
        {
            if (!gameplayEvent.TryGetComponent<GameplayEvent>(out var eventComponent))
            {
                throw new NullReferenceException($"GameObject {gameplayEvent} does not contain a GameplayEvent component.");
            }

            validEvents.Add(gameplayEvent);
        }
        private static void ThrowIfNotInitialized()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("GameplayEventCatalog not Initialized. Use \"catalogAvailability\" to call a method when the GameplayEventCatalog is initialized.");
            }
        }

        private static void ThrowIfInitialized()
        {
            if (_initialized)
            {
                throw new InvalidOperationException("GameplayEventCatalog has already initialized.");
            }
        }
        #endregion
    }
}