using EntityStates;
using RoR2;
using System;
using UnityEngine;

namespace Moonstorm.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Moonstorm/Events/Event Director Card")]
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

        public bool CardIsValid()
        {
            if (!Run.instance)
                return false;
            Run run = Run.instance;

            bool flag = !requiredUnlockableDef || run.IsUnlockableUnlocked(requiredUnlockableDef);
            //If it doesnt have the flag or it does and the loop is greater than 0
            bool flag1 = !CheckFlag(EventFlags.AfterLoop) || run.loopClearCount > 0;
            //If it doesnt have the flag or it does and the void fields have been visited
            bool flag2 = !CheckFlag(EventFlags.AfterVoidFields) || run.GetEventFlag("ArenaPortalTaken");
            //If it isnt one-time or the the flag isnt registered for the run
            bool flag3 = !CheckFlag(EventFlags.OncePerRun) || !run.GetEventFlag(OncePerRunFlag);


            return flag && (flag1 || flag2) && flag3;
        }

        public bool CheckFlag(Enum flag)
        {
            return eventFlags.HasFlag(flag);
        }

        public virtual EntityState InstantiateNextState()
        {
            EntityState entityState = EntityStateCatalog.InstantiateState(activationState);
            return entityState;
        }

        [ContextMenu("Log Flags")]
        private void LogFlags()
        {
            foreach (var flag in typeof(EventFlags).GetEnumValues())
            {
                if (CheckFlag((EventFlags)flag))
                    Debug.Log(Enum.GetName(typeof(EventFlags), flag));
            }
        }
    }
}
