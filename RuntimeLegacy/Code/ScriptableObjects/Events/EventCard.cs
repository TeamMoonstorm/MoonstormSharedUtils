using EntityStates;
using R2API;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm
{
    public class EventCard : ScriptableObject
    {
        public EventIndex EventIndex { get; internal set; }
        public string OncePerRunFlag { get => $"{name}:PlayedThisRun"; }

        [EnumMask(typeof(EventFlags))]
        public EventFlags eventFlags;

        public SerializableEntityStateType eventState;

        public string requiredStateMachine;

        public string startMessageToken;

        public string endMessageToken;

        public Color messageColor = Color.white;

        public NetworkSoundEventDef startSound;


        [EnumMask(typeof(DirectorAPI.Stage))]
        public DirectorAPI.Stage availableStages;

        public List<string> customStageNames = new List<string>();

        public string category;

        [Range(0, 100)]
        public float selectionWeight;

        public int cost;

        public float repeatedSelectionCostCoefficient = 1;

        public int minimumStageCompletions = 0;

        public AddressReferencedUnlockableDef requiredUnlock;

        public AddressReferencedUnlockableDef forbiddenUnlock;

        public List<AddressReferencedExpansionDef> requiredExpansionDefs = new List<AddressReferencedExpansionDef>();

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
            foreach (AddressReferencedExpansionDef ed in requiredExpansionDefs)
            {
                expansionsEnabled = Run.instance.IsExpansionEnabled(ed);
            }
            if (!expansionsEnabled)
            {
                return false;
            }

            bool requiredUnlockableUnlocked = !requiredUnlock || Run.instance.IsUnlockableUnlocked(requiredUnlock);
            bool forbiddenUnlockableUnlocked = forbiddenUnlock && Run.instance.DoesEveryoneHaveThisUnlockableUnlocked(forbiddenUnlock);
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

        public float GetEffectiveCost(int totalRepetitions)
        {
            if (totalRepetitions <= 0)
                return cost;

            return cost + (cost * repeatedSelectionCostCoefficient * totalRepetitions);
        }
    }
}
