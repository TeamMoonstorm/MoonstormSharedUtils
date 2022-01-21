using Moonstorm.ScriptableObjects;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Events
{
    /// <summary>
    /// A generic EventState where all events inherit from
    /// </summary>
    public abstract class EventState : EntityState
    {
        /// <summary>
        /// The EventDirectorCard tied to this Event
        /// </summary>
        [SerializeField]
        public EventDirectorCard eventCard;
        /// <summary>How long this event lasts in Drizzle</summary>
        [SerializeField]
        public float drizzleDuration = 30f;
        /// <summary>
        /// The max duration of this event
        /// </summary>
        [SerializeField]
        public float typhoonDuration = 90f;
        /// <summary>
        /// The amount of time between the warning message and when the event starts
        /// </summary>
        [SerializeField]
        public float warningDuration = 15f;
        /// <summary>
        /// Max scaling used for the events
        /// </summary>
        public static float typhoonScaling = 3.5f;

        public virtual bool overrideTimer => false;

        public bool hasWarned { get; protected set; }
        public float difficultyScaledDuration { get; protected set; }
        public float difficultyScalingValue { get; protected set; }
        public float totalDuration { get; protected set; }
        public override void OnEnter()
        {
            base.OnEnter();
            difficultyScalingValue = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue;

            difficultyScaledDuration = Util.Remap(difficultyScalingValue, 1f, typhoonScaling, drizzleDuration, typhoonDuration);

            totalDuration = difficultyScaledDuration + warningDuration;
            if (NetworkServer.active)
            {
                if (!eventCard.startMessageToken.Equals(string.Empty))
                {
                    Chat.SimpleChatMessage messageBase = new Chat.SimpleChatMessage()
                    {
                        paramTokens = Array.Empty<string>(),
                        baseToken = Util.GenerateColoredString(Language.GetString(eventCard.startMessageToken), eventCard.messageColor)
                    };
                    Chat.SendBroadcastChat(messageBase);
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= totalDuration && !overrideTimer)
            {
                outer.SetNextStateToMain();
                return;
            }
            if (!hasWarned && fixedAge >= warningDuration)
                StartEvent();
        }

        public override void OnExit()
        {
            base.OnExit();
            if (NetworkServer.active)
            {
                Chat.SimpleChatMessage messageBase = new Chat.SimpleChatMessage()
                {
                    paramTokens = Array.Empty<string>(),
                    baseToken = Util.GenerateColoredString(Language.GetString(eventCard.endMessageToken), eventCard.messageColor)
                };
                Chat.SendBroadcastChat(messageBase);
            }
        }

        public virtual void StartEvent()
        {
            hasWarned = true;
        }
    }

}
