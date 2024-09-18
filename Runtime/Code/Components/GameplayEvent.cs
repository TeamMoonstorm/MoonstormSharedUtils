using EntityStates;
using RoR2;
using System;
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
        [SyncVar]
        public bool beginOnStart;

        public float announcementDuration { get => _announcementDuration; set => _announcementDuration = value; }
        [Header("Gameplay Event Metadata")]
        [SyncVar]
        [SerializeField] private float _announcementDuration = 6;

        public float eventDuration { get => _eventDuration; set => _eventDuration = value; }
        [SerializeField, SyncVar]
        private float _eventDuration = -1;
        /// <summary>
        /// Retrieves the Token that contains the Start message for this event.
        /// </summary>
        public string eventStartToken => _eventStartToken;
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
        /// </summary>
        public string eventEndToken => _eventEndToken;
        [Tooltip("When the event ends, this token is used to inform the players.")]
        [SerializeField] private string _eventEndToken;

        /// <summary>
        /// Retrieves the sound effect that should play when the event ends.
        /// </summary>
        public NetworkSoundEventDef eventEndSfx => _eventEndSfx;

        [Tooltip("When the event ends, this sound effect is played to inform the players.")]
        [SerializeField] private NetworkSoundEventDef _eventEndSfx;

        /// <summary>
        /// Retrieves the Color that's used for the messages of this event.
        /// </summary>
        public Color eventColor => _eventColor;
        [Tooltip("This color is used for the messages displayed when the event starts and ends. An outline color is calculated automatically.")]
        [SerializeField] private Color _eventColor;

        /// <summary>
        /// Called on the Server when any event begins
        /// </summary>
        public static event GameplayEventDelegate onEventStartServer;

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
        /// Called on the Server when any event ends
        /// </summary>
        public static event GameplayEventDelegate onEventEndServer;

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
        [field: NonSerialized]
        public GameplayEventIndex gameplayEventIndex { get; internal set; }

        /// <summary>
        /// Checks wether this event is playing or not
        /// </summary>
        public bool isPlaying { get => _isPlaying; private set => _isPlaying = value; }
        [SyncVar(hook = nameof(Client_OnIsPlayingChanged))]
        private bool _isPlaying;

        /// <summary>
        /// If set to true, this event will not announce its start.
        /// </summary>
        public bool doNotAnnounceStart { get => _doNotAnnounceStart; set => _doNotAnnounceStart = value; }
        [SyncVar]
        private bool _doNotAnnounceStart;

        /// <summary>
        /// If set to true, this event will not announce its ending.
        /// </summary>
        public bool doNotAnnounceEnd { get; set; }
        [SyncVar]
        private bool _doNotAnnounceEnd;

        public EntityStateIndex? customTextStateIndex { get; set; }

        public GenericObjectIndex? customTMPFontAssetIndex { get; set; }

        /// <summary>
        /// The GameplayEventRequirement attached to this GameplayEvent, this value can be null.
        /// </summary>
        public NullableRef<GameplayEventRequirement> gameplayEventRequirement { get; private set; }

        private float _eventDurationStopwatch;

        private void Awake()
        {
            InstanceTracker.Add(this);
            gameplayEventRequirement = GetComponent<GameplayEventRequirement>();
        }

        private void Start()
        {
            if (beginOnStart && !isPlaying && NetworkServer.active)
            {
                StartEvent();
            }
        }

        private void OnDestroy()
        {
            if (isPlaying && NetworkServer.active)
            {
                EndEvent();
            }
            InstanceTracker.Remove(this);
        }

        /// <summary>
        /// Server Only Method
        /// <br></br>
        /// Starts the event if the event is not playing, as such, it also invokes events related to when the event starts.
        /// </summary>
        [Server]
        public void StartEvent()
        {
            if (isPlaying)
                return;

            _eventDurationStopwatch = 0;
            isPlaying = true;

            onEventStartServer?.Invoke(this);

            onEventStartGlobal?.Invoke(this);
            onEventStart?.Invoke(this);
            _onEventStart?.Invoke(this);

            AnnounceEvent();
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active)
                return;

            if (eventDuration < 0 || !isPlaying)
                return;

            _eventDurationStopwatch += Time.fixedDeltaTime;
            if(_eventDurationStopwatch > eventDuration)
            {
                EndEvent();
            }
        }


        /// <summary>
        /// Server Only Method
        /// <br></br>
        /// 
        /// Ends the event if the event is playing, as such, it also invokes events related to when the event ends.
        /// </summary>
        [Server]
        public void EndEvent()
        {
            if (!isPlaying)
                return;

            _eventDurationStopwatch = 0;
            isPlaying = false;

            onEventEndServer?.Invoke(this);

            onEventEndGlobal?.Invoke(this);
            onEventEnd?.Invoke(this);
            _onEventEnd?.Invoke(this);

            AnnounceEvent();
        }

        private void AnnounceEvent()
        {
            if (isPlaying && doNotAnnounceStart)
                return;

            if (!isPlaying && doNotAnnounceEnd)
                return;

            if (!GameplayEventTextController.instance)
                return;

            GameplayEventTextController.EventTextRequest startRequest = default;

            if (customTextStateIndex.HasValue)
                startRequest.customTextState = new SerializableEntityStateType(EntityStateCatalog.GetStateType(customTextStateIndex.Value));

            if (customTMPFontAssetIndex.HasValue)
                startRequest.genericObjectIndexThatPointsToTMP_FontAsset = customTMPFontAssetIndex.Value;

            startRequest.textDuration = announcementDuration;
            startRequest.eventToken = eventStartToken;
            startRequest.eventColor = eventColor;

            GameplayEventTextController.instance.EnqueueNewTextRequest(startRequest, false);
        }

        [Client]
        private void Client_OnIsPlayingChanged(bool newVal)
        {
            if(newVal)
            {
                onEventStartGlobal?.Invoke(this);
                onEventStart?.Invoke(this);
                _onEventStart.Invoke(this);
            }
            else
            {
                onEventEndGlobal?.Invoke(this);
                onEventEnd?.Invoke(this);
                _onEventEnd?.Invoke(this);
            }
            AnnounceEvent();
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (!initialState)
                return false;

            writer.Write(customTextStateIndex.HasValue);
            if (customTextStateIndex.HasValue)
                writer.Write(customTextStateIndex.Value);

            writer.Write(customTMPFontAssetIndex.HasValue);
            if (customTMPFontAssetIndex.HasValue)
                writer.Write(customTMPFontAssetIndex.Value);

            return true;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if(initialState)
            {
                if(reader.ReadBoolean())
                    customTextStateIndex = reader.ReadEntityStateIndex();
    
                if(reader.ReadBoolean())
                    customTMPFontAssetIndex = reader.ReadGenericObjectIndex();
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

    public delegate void GameplayEventDelegate(GameplayEvent gameplayEvent);
}