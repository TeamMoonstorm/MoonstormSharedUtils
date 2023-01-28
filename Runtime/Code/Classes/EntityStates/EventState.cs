using Moonstorm;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Events
{
    /// <summary>
    /// Base class for all event related entity states
    /// </summary>
    public abstract class EventState : EntityState
    {
        [SerializeField]
        public EventCard eventCard;
        [SerializeField]
        public float minDuration = 30f;
        [SerializeField]
        public float maxDuration = 90f;
        [SerializeField]
        public float warningDur = 10f;

        /// <summary>
        /// Wether or not this event should end on a timer
        /// </summary>
        public virtual bool OverrideTimer => false;

        /// <summary>
        /// Wether this event is already past the "Warned" phase
        /// </summary>
        public bool HasWarned { get; protected set; }

        /// <summary>
        /// The actual duration of the event
        /// <para>Duration is taken by remaping the current <see cref="DiffScalingValue"/> capping the in value with min 1 and max 3.5, and keeping the result between min <see cref="minDuration"/> and max <see cref="maxDuration"/></para>
        /// </summary>
        public float DiffScaledDuration { get; protected set; }

        /// <summary>
        /// The current run's difficulty scaling value, taken from the difficultyDef.
        /// </summary>
        public float DiffScalingValue { get; protected set; }

        /// <summary>
        /// The total duration of the event, calculated from the sum of <see cref="DiffScaledDuration"/> and <see cref="warningDur"/>
        /// </summary>
        public float TotalDuration { get; protected set; }

        /// <summary>
        /// When an event starts, this action runs, This action runs for both Clients and Servers
        /// </summary>
        public static event Action<EventCard> onEventStartGlobal;
        
        /// <summary>
        /// When an event starts, this action runs, This action runs only for the Server
        /// </summary>
        public static event Action<EventCard> onEventStartServer;

        /// <summary>
        /// When an event ends, this action runs, this action runs for both Clients and Servers
        /// </summary>
        public static event Action<EventCard> onEventEndGlobal;
        
        /// <summary>
        /// When an event ends, this action runs, this action runs only for the Server.
        /// </summary>
        public static event Action<EventCard> onEventEndServer;

        /// <summary>
        /// Called when the event becomes the main state of the entity state machine.
        /// OnEnter the following things happen:
        /// <para><see cref="DiffScalingValue"/> becomes populated with the DifficultyScalingValue of the run</para>
        /// <para><see cref="DiffScaledDuration"/> gets calculated using the min and max duration of the event</para>
        /// <para><see cref="TotalDuration"/> becomes populated</para>
        /// <para>The event message gets instantiated</para>
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            DiffScalingValue = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue;

            DiffScaledDuration = Util.Remap(DiffScalingValue, 1f, MSUConfig.maxDifficultyScaling.Value, minDuration, maxDuration);

            TotalDuration = DiffScaledDuration + warningDur;
            if (!eventCard.startMessageToken.Equals(string.Empty))
            {
                if(MSUConfig.eventAnnouncementsAsChatMessages.Value)
                {
                    Chat.SimpleChatMessage messageBase = new Chat.SimpleChatMessage()
                    {
                        paramTokens = Array.Empty<string>(),
                        baseToken = Util.GenerateColoredString(Language.GetString(eventCard.startMessageToken), eventCard.messageColor)
                    };
                    Chat.SendBroadcastChat(messageBase);
                }
                else
                    EventHelpers.AnnounceEvent(new EventHelpers.EventAnnounceInfo(eventCard, warningDur, true));
            }
        }

        /// <summary>
        /// Called every FixedUpdate
        /// <para>On FixedUpdate the following things happen:</para>
        /// <para>The timer is checked, if <see cref="OverrideTimer"/> is false and fixedAge is greater than <see cref="TotalDuration"/>, the event ends</para>
        /// <para>If <see cref="HasWarned"/> is false and fixedAge is greater than <see cref="warningDur"/>, <see cref="StartEvent"/> gets called</para>
        /// </summary>
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= TotalDuration && !OverrideTimer)
            {
                outer.SetNextStateToMain();
                return;
            }
            if (!HasWarned && fixedAge >= warningDur)
                StartEvent();
        }

        /// <summary>
        /// Called when the event is no longer the main state of the entity state machine.
        /// OnExit the following things happen:
        /// <para>The Event end message gets instantiated</para>
        /// <para><see cref="onEventEndGlobal"/> and <see cref="onEventEndServer"/> gets invoked</para>
        /// </summary>
        public override void OnExit()
        {
            base.OnExit();
            if(!eventCard.startMessageToken.Equals(string.Empty))
            {
                if(MSUConfig.eventAnnouncementsAsChatMessages.Value)
                {
                    Chat.SimpleChatMessage messageBase = new Chat.SimpleChatMessage()
                    {
                        paramTokens = Array.Empty<string>(),
                        baseToken = Util.GenerateColoredString(Language.GetString(eventCard.endMessageToken), eventCard.messageColor)
                    };
                    Chat.SendBroadcastChat(messageBase);
                }
                else
                {
                    EventHelpers.AnnounceEvent(new EventHelpers.EventAnnounceInfo(eventCard, warningDur, false));
                }
            }

            if (NetworkServer.active)
            {
                onEventEndServer?.Invoke(eventCard);
            }
            onEventEndGlobal?.Invoke(eventCard);
        }

        /// <summary>
        /// Run logic that happens when the event starts here.
        /// <para>See <see cref="FixedUpdate"/> to understand when this method gets called</para>
        /// <para>When StartEvent gets called, the following things happen</para>
        /// <para><see cref="HasWarned"/> gets set to true</para>
        /// <para><see cref="onEventStartGlobal"/> and <see cref="onEventStartServer"/> gets invoked</para>
        /// </summary>
        public virtual void StartEvent()
        {
            HasWarned = true;
            onEventStartGlobal?.Invoke(eventCard);
            if (NetworkServer.active)
                onEventStartServer?.Invoke(eventCard);
        }
    }
}