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

        public static Action<EventCard> onEventStartGlobal;
        public static Action<EventCard> onEventStartServer;

        public static Action<EventCard> onEventEndGlobal;
        public static Action<EventCard> onEventEndServer;
        public override void OnEnter()
        {
            base.OnEnter();
            DiffScalingValue = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue;

            DiffScaledDuration = Util.Remap(DiffScalingValue, 1f, MSUConfig.maxDifficultyScaling.Value, minDuration, maxDuration);

            TotalDuration = DiffScaledDuration + warningDur;
            if (NetworkServer.active)
            {
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
        }
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

        public override void OnExit()
        {
            base.OnExit();
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

            if (NetworkServer.active)
            {
                onEventEndServer?.Invoke(eventCard);
            }
            onEventEndGlobal?.Invoke(eventCard);
        }

        /// <summary>
        /// Run logic that happens when the event starts here.
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