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
            throw new System.NotImplementedException();
        }

        public float GetEffectiveCost(int totalRepetitions)
        {
            throw new System.NotImplementedException();
        }
    }
}
