using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    public enum GameplayEventIndex
    {
        None = -1,
    }

    public static class GameplayEventCatalog
    {
        public static int RegisteredGameplayEventCount => _registeredGameplayEventObjects.Length;

        private static GameObject[] _gameplayEvents = Array.Empty<GameObject>();
        private static GameObject[] _registeredGameplayEventObjects = Array.Empty<GameObject>();
        private static GameplayEvent[] _registeredGameplayEventComponents = Array.Empty<GameplayEvent>();

        private static readonly Dictionary<string, GameplayEventIndex> _nameToEventIndex = new Dictionary<string, GameplayEventIndex>(StringComparer.OrdinalIgnoreCase);

        public static ResourceAvailability catalogAvailability = default(ResourceAvailability);
        private static bool _initialized = false;

        #region Find/Get methods
        public static GameplayEventIndex FindEventIndex(string eventName)
        {
            ThrowIfNotInitialized();

            if (_nameToEventIndex.TryGetValue(eventName, out var index))
                return index;

            return GameplayEventIndex.None;
        }

        public static GameObject GetGameplayEventObject(GameplayEventIndex index)
        {
            ThrowIfNotInitialized();

            return HG.ArrayUtils.GetSafe(_registeredGameplayEventObjects, (int)index);
        }

        public static GameplayEvent GetGameplayEvent(GameplayEventIndex index)
        {
            ThrowIfNotInitialized();

            return HG.ArrayUtils.GetSafe(_registeredGameplayEventComponents, (int)index);
        }
        #endregion

        #region Add Methods
        public static void AddGameplayEvents(GameObject[] gameplayEventGameObjects)
        {
            ThrowIfInitialized();
            foreach(GameObject go in gameplayEventGameObjects)
            {
                AddGameplayEvent(go);
            }
        }

        public static void AddGameplayEvent(GameObject gameplayEventGameObject)
        {
            ThrowIfInitialized();

            if(!gameplayEventGameObject.TryGetComponent<GameplayEvent>(out var @event))
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
            for(int i = 0; i < _gameplayEvents.Length; i++)
            {
                try
                {
                    RegisterGameplayEvent(_gameplayEvents[i], validEvents);
                }
                catch(Exception e)
                {
                    MSULog.Error(e);
                }
            }

            for(int i = 0; i < validEvents.Count; i++)
            {
                var gameplayEventObject = validEvents[i];
                var gameplayEventComponent = gameplayEventObject.GetComponent<GameplayEvent>();

                gameplayEventComponent.GameplayEventIndex = (GameplayEventIndex)i;
                _nameToEventIndex.Add(gameplayEventObject.name, gameplayEventComponent.GameplayEventIndex);
            }
            return validEvents;
        }

        private static void RegisterGameplayEvent(GameObject gameplayEvent, List<GameObject> validEvents)
        {
            if(!gameplayEvent.TryGetComponent<GameplayEvent>(out var eventComponent))
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
            if(_initialized)
            {
                throw new InvalidOperationException("GameplayEventCatalog has already initialized.");
            }
        }
        #endregion
    }
}