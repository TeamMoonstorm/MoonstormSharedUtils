using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    public static class GameplayEventManager
    {

        public static bool GameplayEventExists(GameplayEventIndex index)
        {
            foreach (GameplayEvent gameplayEvent in InstanceTracker.GetInstancesList<GameplayEvent>())
            {
                if (gameplayEvent.GameplayEventIndex == index)
                    return true;
            }

            return false;
        }

        public static bool GameplayEventIsPlaying(GameplayEventIndex index)
        {
            foreach(GameplayEvent gameplayEvent in InstanceTracker.GetInstancesList<GameplayEvent>())
            {
                if(gameplayEvent.GameplayEventIndex == index)
                    return gameplayEvent.IsPlaying;
            }

            return false;
        }

        public static GameplayEvent SpawnGameplayEvent(GameplayEventSpawnArgs args)
        {
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

        public readonly struct GameplayEventSpawnArgs
        {
            public readonly GameObject gameplayEventPrefab;
            public readonly float? expirationTimer;
            public readonly bool skipEventRequirementChecks;
            public readonly bool ignoreDuplicateEvents;
            public readonly bool? beginOnStartOverride;

            public readonly float? announcementDuration;
            public readonly bool doNotAnnounceStart;
            public readonly bool doNotAnnounceEnd;
        }
    }
}