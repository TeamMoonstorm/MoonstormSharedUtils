using Moonstorm;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Events
{
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

        public bool HasWarned { get; protected set; }

        public float DiffScaledDuration { get; protected set; }

        public float DiffScalingValue { get; protected set; }

        public float TotalDuration { get; protected set; }

        public static event Action<EventCard> onEventStartGlobal;

        public static event Action<EventCard> onEventStartServer;

        public static event Action<EventCard> onEventEndGlobal;

        public static event Action<EventCard> onEventEndServer;

        public override void OnEnter()
        {
            base.OnEnter();
            DiffScalingValue = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue;

            DiffScaledDuration = Util.Remap(DiffScalingValue, 1f, MSUConfig.maxDifficultyScaling, minDuration, maxDuration);

            TotalDuration = DiffScaledDuration + warningDur;
            if (!eventCard.startMessageToken.Equals(string.Empty))
            {
                if (MSUConfig.eventAnnouncementsAsChatMessages)
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
            if (!eventCard.startMessageToken.Equals(string.Empty))
            {
                if (MSUConfig.eventAnnouncementsAsChatMessages)
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

        public virtual void StartEvent()
        {
            HasWarned = true;
            onEventStartGlobal?.Invoke(eventCard);
            if (NetworkServer.active)
                onEventStartServer?.Invoke(eventCard);
        }
    }
}