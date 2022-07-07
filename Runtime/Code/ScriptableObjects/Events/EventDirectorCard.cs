using EntityStates;
using RoR2;
using System;
using UnityEngine;

namespace Moonstorm
{
    [Obsolete("EventDirectorCard is obsolete, Upgrade to EventCard")]
    public class EventDirectorCard : ScriptableObject
    {
        public int Cost
        {
            get
            {
                return directorCreditCost;
            }
        }

        public string OncePerRunFlag
        {
            get
            {
                return $"{identifier}:PlayedThisRun";
            }
        }

        public EventIndex EventIndex { get; set; } = EventIndex.None;

        [Header("Internal name")]
        public string identifier;
        [Tooltip("The Entity State to call when the event triggers.")]
        public SerializableEntityStateType activationState;

        [Header("Selection")]
        public int directorCreditCost;
        public float selectionWeight = 1;
        public int minimumStageCompletions;
        [SerializeField]
        [EnumMask(typeof(EventFlags))]
        [Tooltip("To use this you need the Enum Flags Drawer! Ask for it.")]
        public EventFlags eventFlags;
        [Range(0f, 1f)]
        [Tooltip("How easy it is to repeat the event consecutively. Lower values makes it more difficult. 0 makes the event never repeat.")]
        public float repeatedSelectionWeight = 1f;
        public UnlockableDef requiredUnlockableDef;

        [Header("Start/End Messages")]
        public string startMessageToken;
        public string endMessageToken;
        public Color messageColor = Color.white;

        [Flags]
        public enum EventFlags : uint
        {
            None = 0U,
            Weather = 1U,
            AfterVoidFields = 2U,
            AfterLoop = 4U,
            EnemySpawn = 8U,
            OncePerRun = 16U,
            Count = 32U
        }
    }
}
