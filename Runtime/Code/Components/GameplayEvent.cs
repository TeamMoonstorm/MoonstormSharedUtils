using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace MSU
{
    /// <summary>
    /// A GameplayEvent is a <see cref="NetworkBehaviour"/> used for managing the lifetime of a GameplayEvent in the world.
    /// 
    /// <br>GameplayEvents have their own catalog called the <see cref="GameplayEventCatalog"/>. And as such the gameplay events should be added there as well.</br>
    /// 
    /// <br>To aid with the spawning of GameplayEvents, there's the <see cref="GameplayEventManager"/>, which can be used to check if a specific event is active, alive, or for spawning new events.</br>
    /// </summary>
    public sealed class GameplayEvent : NetworkBehaviour
    {
        [Tooltip("When set to true, this gameplay event will call \"StartEvent\" on it's Start method")]
        public bool beginOnStart;

        [Tooltip("When set to true, this gameplay event will ommit creating an announcement that it has ended.")]
        public bool doNotAnnounceEnding;

        /// <summary>
        /// Retrieves the Token that contains the Start message for this event.
        /// </summary>
        public string eventStartToken => _eventStartToken;
        [Header("Gameplay Event Metadata")]
        [Tooltip("When the event starts, this Token is used to inform the players.")]
        [SerializeField] private string _eventStartToken;

        /// <summary>
        /// Retrieves the sound effect that should play when the event starts.
        /// </summary>
        public NetworkSoundEventDef eventStartSfx => _eventStartSfx;
        [Tooltip("When the event starts, this sound effect is played to inform the players.")]
        [SerializeField] private NetworkSoundEventDef _eventStartSfx;

        /// <summary>
        /// Retrieves the Token that contains the End message for this event.
        /// <br>Irrelevant if <see cref="doNotAnnounceEnding"/> is set to true</br>
        /// </summary>
        public string eventEndToken => _eventEndToken;
        [Tooltip("When the event ends, this token is used to inform the players. \nIrrelevant if \"doNotAnnounceEnding\" is set to true.")]
        [SerializeField] private string _eventEndToken;

        /// <summary>
        /// Retrieves the sound effect that should play when the event ends.
        /// <br>Irrelevant if <see cref="doNotAnnounceEnding"/> is set to true</br>
        /// </summary>
        public NetworkSoundEventDef eventEndSfx => _eventEndSfx;

        [Tooltip("When the event ends, this sound effect is played to inform the players. \nIrrelevant if \"doNotAnnounceEnding\" is set to true.")]
        [SerializeField] private NetworkSoundEventDef _eventEndSfx;

        /// <summary>
        /// Retrieves the Color that's used for the messages of this event.
        /// </summary>
        public Color eventColor => _eventColor;
        [Tooltip("This color is used for the messages displayed when the event starts and ends. An outline color is calculated automatically.")]
        [SerializeField] private Color _eventColor;

        /// <summary>
        /// Called when any event begins.
        /// </summary>
        public static event GameplayEventDelegate onEventStartGlobal;

        /// <summary>
        /// Called when this specific event begins
        /// </summary>
        public event GameplayEventDelegate onEventStart;
        [Space, Header("Gameplay Event Hooks")]
        [Tooltip("Called when this specific event begins")]
        [SerializeField] private UnityGameplayEvent _onEventStart;

        /// <summary>
        /// Called when any event ends
        /// </summary>
        public static event GameplayEventDelegate onEventEndGlobal;
        /// <summary>
        /// Called when this specific event ends.
        /// </summary>
        public event GameplayEventDelegate onEventEnd;
        [Tooltip("Called when this specific event ends")]
        [SerializeField] private UnityGameplayEvent _onEventEnd;

        /// <summary>
        /// The GameplayEventIndex assigned to this GameplayEvent. set at runtime when the prefab is added to the <see cref="GameplayEventCatalog"/>
        /// </summary>
        [field:NonSerialized]
        public GameplayEventIndex gameplayEventIndex { get; internal set; }

        /// <summary>
        /// Checks wether this event is playing or not
        /// </summary>
        public bool isPlaying { get; private set; }

        /// <summary>
        /// The GameplayEventRequirement attached to this GameplayEvent, this value can be null.
        /// </summary>
        public NullableRef<GameplayEventRequirement> gameplayEventRequirement { get; private set; }

        /// <summary>
        /// The total duration of the announcement for this event.
        /// </summary>
        [NonSerialized]
        public float announcementDuration = 6f;

        private void Awake()
        {
            InstanceTracker.Add(this);
            gameplayEventRequirement = GetComponent<GameplayEventRequirement>();
        }

        private void Start()
        {
            if(beginOnStart && !isPlaying)
            {
                StartEvent();
            }
        }

        private void OnDestroy()
        {
            if(isPlaying)
            {
                EndEvent();
            }
            InstanceTracker.Remove(this);
        }

        /// <summary>
        /// Starts the event if the event is not playing, as such, it also invokes events related to when the event starts.
        /// </summary>
        public void StartEvent()
        {
            if (isPlaying)
                return;

            isPlaying = true;
            onEventStartGlobal?.Invoke(this);
            onEventStart?.Invoke(this);
            _onEventStart?.Invoke(this);
        }


        /// <summary>
        /// Ends the event if the event is playing, as such, it also invokes events related to when the event ends.
        /// </summary>
        public void EndEvent()
        {
            if (!isPlaying)
                return;

            isPlaying = false;
            onEventEndGlobal?.Invoke(this);
            onEventEnd?.Invoke(this);
            _onEventEnd?.Invoke(this);

            if(!doNotAnnounceEnding && GameplayEventTextController.instance)
            {
                GameplayEventTextController.instance.EnqueueNewTextRequest(new GameplayEventTextController.EventTextRequest
                {
                    eventColor = eventColor,
                    eventToken = eventEndToken,
                    textDuration = announcementDuration
                });
            }
        }
    }

    /// <summary>
    /// Represents a <see cref="UnityEvent"/> that takes <see cref="GameplayEvent"/> as it's first argument.
    /// </summary>
    [Serializable]
    public class UnityGameplayEvent : UnityEvent<GameplayEvent>
    {

    }

    /// <summary>
    /// Represents a C# Delegate that takes <see cref="GameplayEvent"/> as it's first argument and returns void.
    /// </summary>
    /// <param name="gameplayEvent">The GameplayEvent that raised this delegate.</param>

    public delegate void GameplayEventDelegate (GameplayEvent gameplayEvent);
}