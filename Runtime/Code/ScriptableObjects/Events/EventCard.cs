using EntityStates;
using Moonstorm.AddressableAssets;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

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

        [Tooltip("The flags for this event")]
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
        [Tooltip("If this has already played in the current stage, the cost will be multiplied by this amount.")]
        public float repeatedSelectionCostCoefficient = 1;
        [Tooltip("How many stages need to pass before this event can be played")]
        public int minimumStageCompletions = 0;
        [Tooltip("If supplied, this event can only play if this unlockableDef has been unlocked")]
        public AddressableUnlockableDef requiredUnlockableDef;
        [Tooltip("If supplied, this event CANNOT play if this unlockableDef has been unlocked")]
        public AddressableUnlockableDef forbiddenUnlockableDef;
        [Tooltip("If supplied, this event can only play if ALL expansion defs are enabled")]
        public List<AddressableExpansionDef> requiredExpansions = new List<AddressableExpansionDef>();

        /// <summary>
        /// Checks if this card can be used currently.
        /// Override this to implement more checks on a custom, subclassed version of EventCard
        /// </summary>
        /// <returns>True if this event is available, false otherwise</returns>
        public virtual bool IsAvailable()
        {
            if (!Run.instance)
            {
                return false;
            }

            if (Run.instance.stageClearCount <= minimumStageCompletions)
            {
                return false;
            }

            var expansionsEnabled = true;
            foreach (AddressableExpansionDef ed in requiredExpansions)
            {
                expansionsEnabled = Run.instance.IsExpansionEnabled(ed.Asset);
            }
            if (!expansionsEnabled)
            {
                return false;
            }

            bool requiredUnlockableUnlocked = !requiredUnlockableDef || Run.instance.IsUnlockableUnlocked(requiredUnlockableDef);
            bool forbiddenUnlockableUnlocked = forbiddenUnlockableDef && Run.instance.DoesEveryoneHaveThisUnlockableUnlocked(forbiddenUnlockableDef);
            if (!(requiredUnlockableUnlocked && !forbiddenUnlockableUnlocked))
            {
                return false;
            }

            if (eventFlags.HasFlag(EventFlags.OncePerRun))
            {
                if (Run.instance.GetEventFlag(OncePerRunFlag))
                {
                    return false;
                }
            }



            //If it doesnt have the flag or it does and the loop is greater than 0
            bool flag2 = !eventFlags.HasFlag(EventFlags.AfterLoop) || Run.instance.loopClearCount > 0;
            //If it doesnt have the flag or it does and the void fields have been visited
            bool flag3 = !eventFlags.HasFlag(EventFlags.AfterVoidFields) || Run.instance.GetEventFlag("ArenaPortalTaken");

            if (!(flag2 || flag3))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// When an event is played, internally the director adds it to a dictionary to keep track how many times it has played.
        /// <para>Depending on how many times it has played, the card will increase in cost, use this method to get the Effective cost of this card.</para>
        /// </summary>
        /// <param name="totalRepetitions">The total amount of times this event has repeated.</param>
        /// <returns>The actual cost of the card</returns>
        public float GetEffectiveCost(int totalRepetitions)
        {
            if (totalRepetitions <= 0)
                return cost;

            return cost + (cost * repeatedSelectionCostCoefficient * totalRepetitions);
        }
    }
}
