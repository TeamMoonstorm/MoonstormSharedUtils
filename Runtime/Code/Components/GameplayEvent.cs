using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace MSU
{
    public class GameplayEvent : NetworkBehaviour
    {
        public bool beginOnStart;
        public bool doNotAnnounceEnding;
        public string EventStartToken => _eventStartToken;
        [Header("Gameplay Event Metadata")]
        [SerializeField] private string _eventStartToken;

        public NetworkSoundEventDef EventStartSfx => _eventStartSfx;
        [SerializeField] private NetworkSoundEventDef _eventStartSfx;

        public string EventEndToken => _eventEndToken;
        [SerializeField] private string _eventEndToken;

        public NetworkSoundEventDef EventEndSfx => _eventEndSfx;
        [SerializeField] private NetworkSoundEventDef _eventEndSfx;

        public Color EventColor => _eventColor;
        [SerializeField] private Color _eventColor;

        public static event GameplayEventDelegate OnEventStartGlobal;
        public event GameplayEventDelegate OnEventStart;
        [Space, Header("Gameplay Event Hooks")]
        [SerializeField] private UnityGameplayEvent _onEventStart;

        public static event GameplayEventDelegate OnEventEndGlobal;
        public event GameplayEventDelegate OnEventEnd;
        [SerializeField] private UnityGameplayEvent _onEventEnd;
        public GameplayEventIndex GameplayEventIndex { get; internal set; }

        public bool IsPlaying { get; private set; }

        [NonSerialized]
        public float announcementDuration = 6f;

        private void Awake()
        {
            InstanceTracker.Add(this);
        }
        private void Start()
        {
            if(beginOnStart && !IsPlaying)
            {
                StartEvent();
            }
        }

        private void OnDestroy()
        {
            if(IsPlaying)
            {
                EndEvent();
            }
            InstanceTracker.Remove(this);
        }

        public void StartEvent()
        {
            if (IsPlaying)
                return;

            IsPlaying = true;
            OnEventStartGlobal?.Invoke(this);
            OnEventStart?.Invoke(this);
            _onEventStart?.Invoke(this);
        }

        public void EndEvent()
        {
            if (!IsPlaying)
                return;

            IsPlaying = false;
            OnEventEndGlobal?.Invoke(this);
            OnEventEnd?.Invoke(this);
            _onEventEnd?.Invoke(this);

            if(!doNotAnnounceEnding && GameplayEventTextController.Instance)
            {
                GameplayEventTextController.Instance.EnqueueNewTextRequest(new GameplayEventTextController.EventTextRequest
                {
                    eventColor = EventColor,
                    eventToken = EventEndToken,
                    textDuration = announcementDuration
                });
            }
        }
    }

    [Serializable]
    public class UnityGameplayEvent : UnityEvent<GameplayEvent>
    {

    }

    public delegate void GameplayEventDelegate (GameplayEvent gameplayEvent);
}