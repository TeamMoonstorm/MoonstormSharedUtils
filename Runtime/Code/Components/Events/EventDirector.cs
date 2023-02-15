using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Components
{
    /// <summary>
    /// The <see cref="EventDirector"/> is a Singleton class that's used for managing MSU's Event system.
    /// </summary>
    [RequireComponent(typeof(NetworkStateMachine))]
    public class EventDirector : MonoBehaviour
    {
        /// <summary>
        /// Returns the current instance of the EventDirector
        /// </summary>
        public static EventDirector Instance { get; private set; }
        /// <summary>
        /// The NetworkStateMachine tied to this Eventdirector
        /// </summary>
        public NetworkStateMachine NetworkStateMachine { get; private set; }
        /// <summary>
        /// The EventFunctions tied to this EventDirector
        /// </summary>
        public EventFunctions EventFunctions { get; private set; }
        /// <summary>
        /// The current stage's EventCardSelection
        /// </summary>
        public WeightedSelection<EventCard> EventCardSelection { get; private set; }
        /// <summary>
        /// The CategorySelection for the current stage
        /// </summary>
        public EventDirectorCategorySelection EventDirectorCategorySelection { get; private set; }
        /// <summary>
        /// The EntityStateMachine where the next event will play
        /// </summary>
        public EntityStateMachine TargetedStateMachine { get; private set; }
        /// <summary>
        /// The last event that attempted to play
        /// </summary>
        public EventCard LastAttemptedEventCard { get; private set; }
        /// <summary>
        /// The last event that succesfully played
        /// </summary>
        public EventCard LastSuccesfulEventCard { get; private set; }
        /// <summary>
        /// The total amount of credits spent
        /// </summary>
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

        [Tooltip("The amount of credits gained when the intervalStopWatch reaches 0")]
        public RangeFloat creditGainRange;

        [Tooltip("The current amount of Credits")]
        public float eventCredits;

        [Tooltip("The amount of time that takes between new credits for the director")]
        public RangeFloat intervalResetRange;

        [Tooltip("The stopwatch of the EventDirector")]
        public float intervalStopWatch;

        [Tooltip("The weather parameters when the scene started")]
        public SceneWeatherController.WeatherParams weatherParamsWhenSceneStarted;

        [Tooltip("The RTCP when the scene started")]
        public string weatherRtpcWhenStarted = string.Empty;

        private Xoroshiro128Plus eventRNG;
        private EventCard currentEventCard;
        private Dictionary<EventIndex, int> eventToAmountsPlayed = new Dictionary<EventIndex, int>();


        [SystemInitializer(typeof(EventCatalog))]
        private static void SystemInit()
        {
            SceneDirector.onPrePopulateSceneServer += (director) =>
            {
                if (EventCatalog.RegisteredEventCount <= 0)
                {
                    return;
                }

                if (Run.instance && SceneInfo.instance.countsAsStage && NetworkServer.active)
                {
                    var go = Object.Instantiate(MoonstormSharedUtils.MSUAssetBundle.LoadAsset<GameObject>("MSUEventDirector"));
                    NetworkServer.Spawn(go);
                    var thisComponent = go.GetComponent<EventDirector>();
                    if (!thisComponent.isActiveAndEnabled)
                        thisComponent.enabled = true;
#if DEBUG
                    thisComponent.Log("Spawned");
#endif
                }
            };
        }

        private void Awake()
        {
            if (NetworkServer.active && Run.instance && SceneInfo.instance && SceneInfo.instance.sceneDef)
            {
                NetworkStateMachine = GetComponent<NetworkStateMachine>();
                EventFunctions = GetComponent<EventFunctions>();

                eventRNG = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
                EventDirectorCategorySelection = EventCatalog.GetCategoryFromSceneDef(SceneInfo.instance.sceneDef);
                if (EventDirectorCategorySelection == null)
                {
                    MSULog.Error($"COULD NOT RETRIEVE EVENT CATEGORY FOR SCENE {SceneInfo.instance.sceneDef}!!!");
#if DEBUG
                    Log($"Destroying root");
#endif
                    Destroy(gameObject.transform.root.gameObject);
                    return;
                }
                EventCardSelection = EventDirectorCategorySelection.GenerateWeightedSelection();
                eventToAmountsPlayed = EventCardSelection.GetValues().ToDictionary(x => x.EventIndex, y => 0);
#if DEBUG
                Log("Final Card Selection: " + string.Join("\n", EventCardSelection.choices.Select(x => x.value)));
#endif
                //Log below causes issues, no idea why
                //Log($"Awakened with the following EventCards:\n{string.Join("\n", EventCardSelection.choices.Select(c => c.value.name))}");
            }
        }
        private void OnEnable()
        {
            if (!Instance)
            {
                Instance = this;
                return;
            }
            MSULog.Error($"Duplicate instance of singleton class {GetType().Name}. Only one should exist at a time.");
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active && Run.instance)
            {
                if (!Run.instance.isRunStopwatchPaused)
                {
                    intervalStopWatch -= Time.fixedDeltaTime;
                    if (intervalStopWatch <= 0)
                    {
                        float newStopwatchVal = eventRNG.RangeFloat(intervalResetRange.min, intervalResetRange.max);
#if DEBUG
                        Log($"EventDirector: new stopwatch value: {newStopwatchVal}");
#endif
                        intervalStopWatch = newStopwatchVal;

                        float compensatedDifficultyCoefficient = Run.instance.compensatedDifficultyCoefficient;

                        float eventScaling = compensatedDifficultyCoefficient / Run.instance.participatingPlayerCount;

#if DEBUG
                        Log($"Event Director: compensated difficulty coefficient: {compensatedDifficultyCoefficient}" +
                                $"\nevent scaling: {eventScaling}");
#endif
                        float newCredits = eventRNG.RangeFloat(creditGainRange.min, creditGainRange.max) * eventScaling;
                        eventCredits += newCredits;
#if DEBUG
                        Log($"Event Director: new Credits: {newCredits}" +
                                $"\nTotal credits so far: {eventCredits}");
#endif
                        Simulate();
                    }
                }
            }
        }

        private void Simulate()
        {
            if (AttemptSpawnEvent())
            {
                float amount = eventRNG.RangeFloat(intervalResetRange.min, intervalResetRange.max) * 10;
                intervalStopWatch += amount;
#if DEBUG
                Log($"Added {amount} to interval stopwatch" +
                    $"\n(New value: {intervalStopWatch})");
#endif
                return;
            }
            currentEventCard = null;
        }
        private bool AttemptSpawnEvent()
        {
            bool canSpawn = false;
            if (currentEventCard == null)
            {
#if DEBUG
                Log($"Current event card is null, picking new one");
#endif
                if (EventCardSelection.Count == 0)
                {
#if DEBUG
                    Log($"Cannot pick a card when there's no cards in the EventCardSelection (Count: {EventCardSelection.Count})");
#endif
                    return false;
                }
                canSpawn = PrepareNewEvent(EventCardSelection.Evaluate(eventRNG.nextNormalizedFloat));

                if (!canSpawn)
                    return false;
            }
            float effectiveCost = currentEventCard.GetEffectiveCost(eventToAmountsPlayed[currentEventCard.EventIndex]);
#if DEBUG
            Log($"Playing event {currentEventCard}" +
                $"\n(Event state: {currentEventCard.eventState})");
#endif
            TargetedStateMachine.SetState(EntityStateCatalog.InstantiateState(currentEventCard.eventState));

            if (currentEventCard.eventFlags.HasFlag(EventFlags.OncePerRun))
            {
#if DEBUG
                Log($"Card {currentEventCard} has OncePerRun flag, setting flag.");
#endif
                EventFunctions.RunSetFlag(currentEventCard.OncePerRunFlag);
            }

            eventToAmountsPlayed[currentEventCard.EventIndex]++;

            LastSuccesfulEventCard = currentEventCard;

            eventCredits -= effectiveCost;
            TotalCreditsSpent += effectiveCost;
#if DEBUG
            Log($"Subtracted {effectiveCost} credits" +
                $"\nTotal credits spent: {TotalCreditsSpent}");
#endif

            return true;
        }

        private bool PrepareNewEvent(EventCard card)
        {
#if DEBUG
            Log($"Preparing event {card}");
#endif
            currentEventCard = card;
            if (!card.IsAvailable())
            {
#if DEBUG
                Log($"Event card {card.name} is not available! Aborting.");
#endif
                LastAttemptedEventCard = LastAttemptedEventCard;
                return false;
            }
            float effectiveCost = currentEventCard.GetEffectiveCost(eventToAmountsPlayed[currentEventCard.EventIndex]);
            if (eventCredits < effectiveCost)
            {
#if DEBUG
                Log($"Event card {card.name} is too expensive! (It costs {effectiveCost}, current credits are {eventCredits}), Aborting");
#endif
                LastAttemptedEventCard = LastAttemptedEventCard;
                return false;
            }
            if (IsEventBeingPlayed(card))
            {
#if DEBUG
                Log($"Event card {card.name} is already playing! Aborting");
#endif
                LastAttemptedEventCard = LastAttemptedEventCard;
                return false;
            }
            if(Run.instance.GetEventFlag(currentEventCard.OncePerRunFlag))
            {
#if DEBUG
                Log($"Event card {card.name} already played! Aborting");
#endif
                LastAttemptedEventCard = LastAttemptedEventCard;
                return false;
            }
            FindIdleStateMachine();
            if (card.eventFlags.HasFlag(EventFlags.WeatherRelated) && TargetedStateMachine.customName != "WeatherEvent")
            {
#if DEBUG
                Log($"No empty state machines to play event on! Aborting");
#endif
                return false;
            }

            var teleporterInstance = TeleporterInteraction.instance;
            if (teleporterInstance)
            {
                if (teleporterInstance.isCharged || teleporterInstance.isInFinalSequence)
                {
#if DEBUG
                    Log($"Stage has a teleporter instance and the teleporter is Charged or in it's final sequence, aborting.");
#endif
                    return false;
                }

                if (teleporterInstance.chargePercent > 25)
                {
#if DEBUG
                    Log($"Stage has a teleporter instance and it's charge percent is over 25%, aborting.");
#endif
                    return false;
                }
            }
            return true;
        }

        private bool IsEventBeingPlayed(EventCard card)
        {
            if (NetworkStateMachine)
            {
                foreach (var statemachine in NetworkStateMachine.stateMachines)
                {
                    if (statemachine.state.GetType().Equals(card.eventState.stateType))
                        return true;
                }
            }
            return false;
        }

        private void FindIdleStateMachine()
        {
            if (NetworkStateMachine)
            {
                foreach (var stateMachine in NetworkStateMachine.stateMachines)
                {
                    if (stateMachine.state.GetType().Equals(stateMachine.mainStateType.stateType))
                    {
                        TargetedStateMachine = stateMachine;
                        return;
                    }
                }
            }
        }


        private bool AttemptForceSpawnEvent(EventCard card)
        {
            FindIdleStateMachine();
            if (card && TargetedStateMachine && !IsEventBeingPlayed(card))
            {
                TargetedStateMachine.SetState(EntityStateCatalog.InstantiateState(card.eventState));
                return true;
            }
            return false;
        }

        private int StopAllEvents()
        {
            int eventsStopped = 0;
            foreach (var stateMachine in NetworkStateMachine.stateMachines)
            {
                if (!stateMachine.state.GetType().Equals(stateMachine.mainStateType.stateType))
                {
                    stateMachine.SetNextStateToMain();
                    eventsStopped++;
                }
            }
            return eventsStopped;
        }
#if DEBUG
        private void Log(string msg)
        {
            MSULog.Info($"\n-----o-----\nEvent Director: {msg}\n-----o-----");
        }
        ///Commands
        ///------------------------------------------------------------------------------------------------------------

        [ConCommand(commandName = "force_event", flags = ConVarFlags.ExecuteOnServer, helpText = "Forces a gamewide event to begin. Argument is the event card's name")]
        private static void ForceEvent(ConCommandArgs args)
        {
            if (!Instance)
            {
                Debug.Log($"Event director is unavailable! Cannot start any events.");
                return;
            }

            string evArg = args.TryGetArgString(0).ToLowerInvariant();
            if (string.IsNullOrEmpty(evArg))
            {
                Debug.Log($"Command requires one string argument (Event card name)");
                return;
            }

            EventIndex eventIndex = EventCatalog.FindEventIndex(evArg);
            if (eventIndex == EventIndex.None)
            {
                Debug.Log($"Could not find an EventCard of name {evArg} (FindEventIndex returned EventIndex.None)");
                return;
            }

            EventCard card = EventCatalog.GetEventCard(eventIndex);
            if (card == null)
            {
                Debug.Log($"Could not get eventCard of name {evArg} (EventIndex {eventIndex} isnt tied to any event cards)");
                return;
            }

            if (!Instance.AttemptForceSpawnEvent(card))
            {
                Debug.Log($"Could not start event. too many events are playing.");
                return;
            }
        }

        [ConCommand(commandName = "stop_events", flags = ConVarFlags.ExecuteOnServer, helpText = "Forces all active events to stop")]
        private static void StopEvents(ConCommandArgs args)
        {
            if (!Instance)
            {
                Debug.Log($"Event director is unavailable! Cannot stop any events");
                return;
            }
            int count = Instance.StopAllEvents();
            Debug.Log($"Stopped {count} events");
        }
#endif
    }
}