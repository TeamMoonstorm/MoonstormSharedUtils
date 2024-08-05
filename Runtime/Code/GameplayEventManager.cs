using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MSU
{
    /// <summary>
    /// The <see cref="GameplayEventManager"/> is a static class that contains utility methods for managing the spawning, lifetime, and more related to <see cref="GameplayEvent"/> components
    /// </summary>
    public static class GameplayEventManager
    {
        /// <summary>
        /// Checks if any instance of the specified GameplayEvent exists
        /// </summary>
        /// <param name="gameplayEvent">The gameplay event to check</param>
        /// <returns>True if the event exists</returns>
        public static bool GameplayEventExists(GameplayEvent gameplayEvent) => GameplayEventExists(gameplayEvent ? gameplayEvent.GameplayEventIndex : GameplayEventIndex.None);

        /// <summary>
        /// Checks if any instance of the GameplayEvent of index <paramref name="index"/> exists.
        /// </summary>
        /// <param name="index">The index to check</param>
        /// <returns>True if the event exists</returns>
        public static bool GameplayEventExists(GameplayEventIndex index)
        {
            if (index == GameplayEventIndex.None)
                return false;

            foreach (GameplayEvent gameplayEvent in InstanceTracker.GetInstancesList<GameplayEvent>())
            {
                if (gameplayEvent.GameplayEventIndex == index)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if any instance of thespecified GameplayEvent is playing
        /// </summary>
        /// <param name="gameplayEvent">The gameplay event to check</param>
        /// <returns>True if the event is playing</returns>
        public static bool GameplayEventIsPlaying(GameplayEvent gameplayEvent) => GameplayEventIsPlaying(gameplayEvent ? gameplayEvent.GameplayEventIndex : GameplayEventIndex.None);

        /// <summary>
        /// Checks if any instance of the GameplayEvent of index <paramref name="index"/> is playing
        /// </summary>
        /// <param name="index">The index to check</param>
        /// <returns>True if the event is playing</returns>
        public static bool GameplayEventIsPlaying(GameplayEventIndex index)
        {
            foreach(GameplayEvent gameplayEvent in InstanceTracker.GetInstancesList<GameplayEvent>())
            {
                if(gameplayEvent.GameplayEventIndex == index)
                    return gameplayEvent.IsPlaying;
            }

            return false;
        }

        /// <summary>
        /// Returns the instance of the specified GameplayEvent in <paramref name="prefabComponent"/>
        /// </summary>
        /// <param name="prefabComponent">The GameplayEvent to retrieve</param>
        /// <returns>The GameplayEvent instance, null if no instance is found</returns>
        public static GameplayEvent GetGameplayEventInstance(GameplayEvent prefabComponent) => GetGameplayEventInstance(prefabComponent ? prefabComponent.GameplayEventIndex : GameplayEventIndex.None);

        /// <summary>
        /// Returns the instance of the specified GameplayEvent that has the index <paramref name="index"/>
        /// </summary>
        /// <param name="index">The GameplayEvent to retrieve</param>
        /// <returns>The GameplayEvent instance, null if no instance is found</returns>
        public static GameplayEvent GetGameplayEventInstance(GameplayEventIndex index)
        {
            if (!GameplayEventExists(index))
                return null;

            foreach(var evt in InstanceTracker.GetInstancesList<GameplayEvent>())
            {
                if (evt.GameplayEventIndex == index)
                    return evt;
            }

            return null;
        }

        /// <summary>
        /// Checks if any gameplay event is currently playing
        /// </summary>
        /// <returns>True if any gameplay event is playing, false otherwise</returns>
        public static bool AnyGameplayEventIsPlaying()
        {
            var list = InstanceTracker.GetInstancesList<GameplayEvent>();
            
            if (list.Count == 0)
                return false;

            foreach(GameplayEvent evt in list)
            {
                if (evt.IsPlaying)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if any gameplay event exists
        /// </summary>
        /// <returns>True if at least one gameplay event exists</returns>
        public static bool AnyGameplayEventExists()
        {
            var list = InstanceTracker.GetInstancesList<GameplayEvent>();

            return list.Count > 0;
        }

        /// <summary>
        /// <br>Server only method</br>
        /// Spawns a GameplayEvent using the metadata found within <paramref name="args"/>, and returns the instance.
        /// <para>More info about the possible arguments can be found by checking <see cref="GameplayEventSpawnArgs"/>'s documentation and field documentation.</para>
        /// </summary>
        /// <param name="args">The arguments to use to spawn the event</param>
        /// <returns>The gameplay event that has been spawned</returns>
        public static GameplayEvent SpawnGameplayEvent(GameplayEventSpawnArgs args)
        {
            if (!NetworkServer.active)
                return null;

            var eventPrefab = args.gameplayEventPrefab;
            if (eventPrefab.TryGetComponent<GameplayEvent>(out var evt))
                return null;

            if (evt.GameplayEventIndex == GameplayEventIndex.None)
                return null;

            if(!args.skipEventRequirementChecks && eventPrefab.TryGetComponent<GameplayEventRequirement>(out var requirements))
            {
                if (!requirements.IsAvailable())
                    return null;
            }

            if(!args.ignoreDuplicateEvents)
            {
                if (GameplayEventExists(evt.GameplayEventIndex))
                    return null;
            }

            GameplayEvent evtInstance = UnityEngine.Object.Instantiate(eventPrefab).GetComponent<GameplayEvent>();
            NetworkServer.Spawn(evtInstance.gameObject);
            if(args.announcementDuration.HasValue)
            {
                evtInstance.announcementDuration = args.announcementDuration.Value;
            }

            if(args.expirationTimer.HasValue)
            {
                evtInstance.gameObject.AddComponent<DestroyOnTimer>().duration = args.expirationTimer.Value;
            }

            if(args.beginOnStartOverride.HasValue)
            {
                evtInstance.beginOnStart = args.beginOnStartOverride.Value;
            }

            if(!args.doNotAnnounceStart && GameplayEventTextController.Instance)
            {
                GameplayEventTextController.Instance.EnqueueNewTextRequest(new GameplayEventTextController.EventTextRequest
                {
                    eventToken = evtInstance.EventStartToken,
                    eventColor = evtInstance.EventColor,
                    textDuration = args.announcementDuration ?? 6
                });
            }

            evtInstance.doNotAnnounceEnding = args.doNotAnnounceEnd;

            return evtInstance;
        }

        /// <summary>
        /// Struct that represents the spawn arguments to use for a GameplayEvent
        /// </summary>
        public readonly struct GameplayEventSpawnArgs
        {
            /// <summary>
            /// The gameplay event prefab to spawn
            /// </summary>
            public readonly GameObject gameplayEventPrefab;

            /// <summary>
            /// If a value is provided, the event will self destruct after this amount of seconds
            /// </summary>
            public readonly float? expirationTimer;

            /// <summary>
            /// If true, the GameplayEvent will ignore all requirement checks, even if it has a <see cref="GameplayEventRequirement"/> attached
            /// </summary>
            public readonly bool skipEventRequirementChecks;

            /// <summary>
            /// If true, the GameplayEvent will spawn regardless if an instance of itself is already spawned
            /// </summary>
            public readonly bool ignoreDuplicateEvents;

            /// <summary>
            /// If a value is provided, the gameplayEvent can either start automatically or start when it's <see cref="GameplayEvent.StartEvent"/> is called
            /// </summary>
            public readonly bool? beginOnStartOverride;

            /// <summary>
            /// If provided, the GameplayEvent will display its announcement for this amount of seconds
            /// </summary>
            public readonly float? announcementDuration;

            /// <summary>
            /// If true, the GameplayEvent will not announce its start
            /// </summary>
            public readonly bool doNotAnnounceStart;

            /// <summary>
            /// If true, the GameplayEvent will not announce its ending
            /// </summary>
            public readonly bool doNotAnnounceEnd;
        }
    }
}