using BepInEx;
using RoR2;
using RoR2.ConVar;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Components
{
    [Obsolete]
    [RequireComponent(typeof(NetworkStateMachine))]
    public class EventDirector : MonoBehaviour
    {
        public static EventDirector Instance { get; private set; }

        public NetworkStateMachine NetworkStateMachine { get; private set; }

        public EventFunctions EventFunctions { get; private set; }

        public WeightedSelection<EventCard> EventCardSelection { get; private set; }

        public EventDirectorCategorySelection EventDirectorCategorySelection { get; private set; }

        public EntityStateMachine TargetedStateMachine { get; private set; }

        public EventCard LastAttemptedEventCard { get; private set; }

        public EventCard LastSuccesfulEventCard { get; private set; }

        public float TotalCreditsSpent { get; private set; }
        private int MostExpensiveEventInDeck
        {
            get
            {
                int cost = 0;
                for (int i = 0; i < EventCardSelection.Count; i++)
                {
                    EventCard card = EventCardSelection.GetChoice(i).value;
                    int cardCost = card.cost;

                    cost = Mathf.Max(cost, cardCost);
                }
                return cost;
            }
        }
        private float GetDifficultyScalingValue
        {
            get
            {
                if (!Run.instance)
                    return 0;

                return DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue;
            }
        }

        public RangeFloat creditGainRange;

        public float eventCredits;

        public RangeFloat intervalResetRange;

        public float intervalStopWatch;

        public SceneWeatherController.WeatherParams weatherParamsWhenSceneStarted;

        public string weatherRtpcWhenStarted = string.Empty;

        private Xoroshiro128Plus eventRNG;
        private EventCard currentEventCard;
        private Dictionary<EventIndex, int> eventToAmountsPlayed = new Dictionary<EventIndex, int>();

#if DEBUG
        private StringBuilder log = new StringBuilder();
#endif

        private static void SystemInit()
        {
            throw new System.NotImplementedException();
        }

        private void Awake()
        {
            throw new System.NotImplementedException();
        }
        private void OnEnable()
        {
            throw new System.NotImplementedException();
        }

        private void OnDisable()
        {
            throw new System.NotImplementedException();
        }

        private void FixedUpdate()
        {
            throw new System.NotImplementedException();
        }

        private float GetEventScaling()
        {
            throw new System.NotImplementedException();
        }

        private void Simulate()
        {
            throw new System.NotImplementedException();
        }
        private bool AttemptSpawnEvent()
        {
            throw new System.NotImplementedException();
        }

        private bool PrepareNewEvent(EventCard card)
        {
            throw new System.NotImplementedException();
        }

        private bool IsEventBeingPlayed(EventCard card)
        {
            throw new System.NotImplementedException();
        }

        private void FindIdleStateMachine(EventCard card)
        {
            throw new System.NotImplementedException();
        }

        public static bool AddNewEntityStateMachine(string stateMachineName)
        {
            throw new System.NotImplementedException();
        }

        private void Log(string msg)
        {
            throw new System.NotImplementedException();
        }
        private bool AttemptForceSpawnEvent(EventCard card)
        {
            throw new System.NotImplementedException();
        }

        private int StopAllEvents()
        {
            throw new System.NotImplementedException();
        }

        private bool PlayEventDebug(EventCard card)
        {
            throw new System.NotImplementedException();
        }

        private bool PrepareNewEventDebug(EventCard card)
        {
            throw new System.NotImplementedException();
        }
    }
}