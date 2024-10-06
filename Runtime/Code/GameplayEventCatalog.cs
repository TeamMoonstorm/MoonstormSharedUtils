using RoR2;
using System;
using System.Collections;
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

        private static GameObject[] _registeredGameplayEventObjects = Array.Empty<GameObject>();
        private static GameplayEvent[] _registeredGameplayEventComponents = Array.Empty<GameplayEvent>();

        private static readonly Dictionary<string, GameplayEventIndex> _nameToEventIndex = new Dictionary<string, GameplayEventIndex>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Event ran when the <see cref="GameplayEventCatalog"/> is collecting all the content providers, from which the events will be obtained
        /// </summary>
        public static event CollectGameplayEventContentProvidersDelegate collectGameplayEventContentProviders;

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

        #region Internal Methods
        [SystemInitializer]
        private static IEnumerator Init()
        {
            MSULog.Info("Initializing the GameplayEvent Catalog...");
            List<IGameplayEventContentProvider> contentProviders = new List<IGameplayEventContentProvider>();

            collectGameplayEventContentProviders?.Invoke(AddGameplayEventContentProvider);

            List<GameObject> loadedEvents = new List<GameObject>();

            ParallelMultiStartCoroutine coroutine = new ParallelMultiStartCoroutine();
            foreach (var contentProvider in contentProviders)
            {
                yield return new WaitForEndOfFrame();
                coroutine.Add(contentProvider.LoadGameplayEventsAsync, loadedEvents);
            }

            coroutine.Start();
            while (!coroutine.isDone)
                yield return new WaitForEndOfFrame();

            _nameToEventIndex.Clear();

            loadedEvents = loadedEvents.OrderBy(go => go.name).ToList();

            _registeredGameplayEventObjects = RegisterGameplayEvents(loadedEvents).ToArray();
            _registeredGameplayEventComponents = _registeredGameplayEventObjects.Select(g => g.GetComponent<GameplayEvent>()).ToArray();

            _initialized = true;
            catalogAvailability.MakeAvailable();

            void AddGameplayEventContentProvider(IGameplayEventContentProvider contentProvider)
            {
                contentProviders.Add(contentProvider);
            }
        }

        private static List<GameObject> RegisterGameplayEvents(List<GameObject> _gameplayEvents)
        {
            List<GameObject> validEvents = new List<GameObject>();
            for (int i = 0; i < _gameplayEvents.Count; i++)
            {
                try
                {
                    EnsureValidity(_gameplayEvents[i], validEvents);
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

        private static void EnsureValidity(GameObject gameplayEvent, List<GameObject> validEvents)
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
        #endregion

        /// <summary>
        /// Delegate used to add a new <see cref="IGameplayEventContentProvider"/> to the catalog
        /// </summary>
        /// <param name="provider">The provider to add</param>
        public delegate void AddGameplayEventContentProviderDelegate(IGameplayEventContentProvider provider);

        /// <summary>
        /// Delegate used to collect <see cref="IGameplayEventContentProvider"/> to the catalog.
        /// </summary>
        /// <param name="addGameplayEventContentProvider">The add method</param>
        public delegate void CollectGameplayEventContentProvidersDelegate(AddGameplayEventContentProviderDelegate addGameplayEventContentProvider);

        /// <summary>
        /// An Interface you can implement to add new entries to the <see cref="GameplayEventCatalog"/>
        /// </summary>
        public interface IGameplayEventContentProvider
        {
            /// <summary>
            /// Implement this interface to load any events you'd like to add to the <see cref="GameplayEventCatalog"/>
            /// </summary>
            /// <param name="dest">The list with all the vent game objects, do not remove, clear or replace this list, only add to it</param>
            /// <returns>A coroutine that the catalog uses to await the process</returns>
            IEnumerator LoadGameplayEventsAsync(List<GameObject> dest);
        }
    }
}