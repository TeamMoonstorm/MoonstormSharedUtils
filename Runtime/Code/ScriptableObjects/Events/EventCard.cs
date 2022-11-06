using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using R2API;
using RoR2;
using EntityStates;
using RoR2.ExpansionManagement;
using Moonstorm.AddressableAssets;

namespace Moonstorm
{
    /// <summary>
    /// Represents an EventCard for the <see cref="Moonstorm.Components.EventDirector"/>
    /// </summary>
    [CreateAssetMenu(menuName = "Moonstorm/Events/EventCard")]
    public class EventCard : ScriptableObject
    {
        /// <summary>
        /// The index of this EventCard, set by the <see cref="EventCatalog"/>
        /// </summary>
        public EventIndex EventIndex { get; internal set; }
        /// <summary>
        /// A flag that's used for events that only play once in a run
        /// </summary>
        public string OncePerRunFlag { get => $"{name}:PlayedThisRun"; }

        [Tooltip("The flags for this even")]
        [EnumMask(typeof(EventFlags))]
        public EventFlags eventFlags;
        [Tooltip("The event's EntityState, MUST inherit from EventState")]
        public SerializableEntityStateType eventState;
        [Tooltip("The token that gets displayed at the start of the event")]
        public string startMessageToken;
        [Tooltip("The token that gets displayed at the end of the event")]
        public string endMessageToken;
        [Tooltip("The color to use for the messages")]
        public Color messageColor = Color.white;
        [Tooltip("A sound to play at the start of the event")]
        public NetworkSoundEventDef startSound;

        [Tooltip("The stages where this event can play")]
        [EnumMask(typeof(DirectorAPI.Stage))]
        public DirectorAPI.Stage availableStages;
        [Tooltip("A list of custom stages where this event can play")]
        public List<string> customStageNames = new List<string>();
        [Tooltip("The category of event, category must exist within the EventDirectorCategorySelection")]
        public string category;
        [Tooltip("The selection weight for this event")]
        [Range(0, 100)]
        public float selectionWeight;
        [Tooltip("The credit cost for this event")]
        public int cost;
        [Tooltip("How many stages need to pass before this event can be played")]
        public int minimumStageCompletions;
        [Tooltip("If supplied, this event can only play if this unlockableDef has been unlocked")]
        public AddressableUnlockableDef requiredUnlockableDef;
        [Tooltip("If supplied, this event CANNOT play if this unlockableDef has been unlocked")]
        public AddressableUnlockableDef forbiddenUnlockableDef;
        [Tooltip("If supplied, this event can only play if ALL expansion defs are enabled")]
        public List<AddressableExpansionDef> requiredExpansions = new List<AddressableExpansionDef>();

        /// <summary>
        /// Checks if this card can be used currently
        /// </summary>
        /// <returns>True if this event is available, false otherwise</returns>
        public bool IsAvailable()
        {
            if (!Run.instance)
                return false;

            bool flag0 = !requiredUnlockableDef.Asset || Run.instance.IsUnlockableUnlocked(requiredUnlockableDef.Asset);
            bool flag1 = forbiddenUnlockableDef.Asset && Run.instance.DoesEveryoneHaveThisUnlockableUnlocked(forbiddenUnlockableDef.Asset);
            //if enough stages are cleared, and unlock requirements are met
            if(Run.instance.stageClearCount >= minimumStageCompletions && flag0 && !flag1)
            {
                //If it doesnt have the flag or it does and the loop is greater than 0
                bool flag2 = !eventFlags.HasFlag(EventFlags.AfterLoop) || Run.instance.loopClearCount > 0;
                //If it doesnt have the flag or it does and the void fields have been visited
                bool flag3 = !eventFlags.HasFlag(EventFlags.AfterVoidFields) || Run.instance.GetEventFlag("ArenaPortalTaken");
                //If it isnt one-time or the the flag isnt registered for the run
                bool flag4 = !eventFlags.HasFlag(EventFlags.OncePerRun) || !Run.instance.GetEventFlag(OncePerRunFlag);

                if (!((flag2 || flag3) && flag4))
                    return false;

                var expansionsEnabled = true;
                foreach(AddressableExpansionDef expansionDef in requiredExpansions)
                {
                    expansionsEnabled = Run.instance.IsExpansionEnabled(expansionDef.Asset);
                }
                return expansionsEnabled;
            }
            return false;
        }
    }
}
