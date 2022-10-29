
using BepInEx.Logging;
using RoR2;
using RoR2.ConVar;
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
        /// <summary>
        /// Returns the cost of the most expensive event
        /// </summary>
        private int MostExpensiveEventInDeck
        {
            get
            {
                int cost = 0;
                for(int i = 0; i < EventCardSelection.Count; i++)
                {
                    EventCard card = EventCardSelection.GetChoice(i).value;
                    int cardCost = card.cost;

                    cost = Mathf.Max(cost, cardCost);
                }
                return cost;
            }
        }
        /// <summary>
        /// Returns the current <see cref="DifficultyDef.scalingValue"/> of the run in progress
        /// </summary>
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


        [SystemInitializer(typeof(EventCatalog))]
        private static void SystemInit()
        {
            SceneDirector.onPrePopulateSceneServer += (director) =>
            {
                if(EventCatalog.RegisteredEventCount <= 0)
                {
                    return;
                }

                if(Run.instance && SceneInfo.instance.countsAsStage && NetworkServer.active)
                {
                    var go = Object.Instantiate(MoonstormSharedUtils.MSUAssetBundle.LoadAsset<GameObject>("MSUEventDirector"));
                    NetworkServer.Spawn(go);
                    var thisComponent = go.GetComponent<EventDirector>();
                    if (!thisComponent.isActiveAndEnabled)
                        thisComponent.enabled = true;
                    thisComponent.Log("Spawned");
                }
            };
        }

        private void Awake()
        {
            if(NetworkServer.active && Run.instance && SceneInfo.instance && SceneInfo.instance.sceneDef)
            {
                NetworkStateMachine = GetComponent<NetworkStateMachine>();
                EventFunctions = GetComponent<EventFunctions>();

                eventRNG = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
                EventDirectorCategorySelection = EventCatalog.GetCategoryFromSceneDef(SceneInfo.instance.sceneDef);
                if(EventDirectorCategorySelection == null)
                {
                    MSULog.Error($"COULD NOT RETRIEVE EVENT CATEGORY FOR SCENE {SceneInfo.instance.sceneDef}!!!");
                    Log($"Destroying root");
                    Destroy(gameObject.transform.root.gameObject);
                    return;
                }
                EventCardSelection = EventDirectorCategorySelection.GenerateWeightedSelection();
                //Log below causes issues, no idea why
                //Log($"Awakened with the following EventCards:\n{string.Join("\n", EventCardSelection.choices.Select(c => c.value.name))}");
            }
        }
        private void OnEnable()
        {
            if(!Instance)
            {
                Instance = this;
                return;
            }
            MSULog.Error($"Duplicate instance of singleton class {GetType().Name}. Only one should exist at a time.");
        }

        private void OnDisable()
        {
            if(Instance == this)
            {
                Instance = null;
            }
        }

        private void FixedUpdate()
        {
            if (!cvDisableEventDirector.value && NetworkServer.active && Run.instance)
            {
                if(!Run.instance.isRunStopwatchPaused)
                {
                    intervalStopWatch -= Time.fixedDeltaTime;
                    if (intervalStopWatch <= 0)
                    {
                        float newStopwatchVal = eventRNG.RangeFloat(intervalResetRange.min, intervalResetRange.max);
                        Log($"EventDirector: new stopwatch value: {newStopwatchVal}");
                        intervalStopWatch = newStopwatchVal;

                        float compensatedDifficultyCoefficient = Run.instance.compensatedDifficultyCoefficient;

                        float eventScaling = compensatedDifficultyCoefficient / Run.instance.participatingPlayerCount;

                        Log($"Event Director: compensated difficulty coefficient: {compensatedDifficultyCoefficient}" +
                                $"\nevent scaling: {eventScaling}");

                        float newCredits = eventRNG.RangeFloat(creditGainRange.min, creditGainRange.max) * eventScaling;
                        eventCredits += newCredits;

                        Log($"Event Director: new Credits: {newCredits}" +
                                $"\nTotal credits so far: {eventCredits}");

                        Simulate();
                    }
                }
            }
        }

        private void Simulate()
        {
            if(AttemptSpawnEvent())
            {
                float amount = eventRNG.RangeFloat(intervalResetRange.min, intervalResetRange.max) * 10;
                intervalStopWatch += amount;
                Log($"Added {amount} to interval stopwatch" +
                    $"\n(New value: {intervalStopWatch})");
                return;
            }
            currentEventCard = null;
        }
        private bool AttemptSpawnEvent()
        {
            bool canSpawn = false;
            if(currentEventCard == null)
            {
                Log($"Current event card is null, picking new one");
                if(EventCardSelection.Count == 0)
                {
                    Log($"Cannot pick a card when there's no cards in the EventCardSelection (Count: {EventCardSelection.Count})");
                    return false;
                }
                canSpawn = PrepareNewEvent(EventCardSelection.Evaluate(eventRNG.nextNormalizedFloat));

                if(!canSpawn)
                    return false;
            }

            Log($"Playing event {currentEventCard}" +
                $"\n(Event state: {currentEventCard.eventState})");
            TargetedStateMachine.SetState(EntityStateCatalog.InstantiateState(currentEventCard.eventState));

            if (currentEventCard.eventFlags.HasFlag(EventFlags.OncePerRun))
            {
                Log($"Card {currentEventCard} has OncePerRun flag, setting flag.");
                EventFunctions.RunSetFlag(currentEventCard.OncePerRunFlag);
            }

            LastSuccesfulEventCard = currentEventCard;

            eventCredits -= currentEventCard.cost;
            TotalCreditsSpent += currentEventCard.cost;
            Log($"Subtracted {currentEventCard.cost} credits" +
                $"\nTotal credits spent: {TotalCreditsSpent}");

            return true;
        }

        private bool PrepareNewEvent(EventCard card)
        {
            Log($"Preparing event {card}");
            currentEventCard = card;
            if(!card.IsAvailable())
            {
                Log($"Event card {card.name} is not available! Aborting.");
                LastAttemptedEventCard = LastAttemptedEventCard;
                return false;
            }
            if(eventCredits < currentEventCard.cost)
            {
                Log($"Event card {card.name} is too expensive! Aborting");
                LastAttemptedEventCard = LastAttemptedEventCard;
                return false;
            }
            if(IsEventBeingPlayed(card))
            {
                Log($"Event card {card.name} is already playing! Aborting");
                LastAttemptedEventCard = LastAttemptedEventCard;
                return false;
            }
            FindIdleStateMachine();
            if (card.eventFlags.HasFlag(EventFlags.WeatherRelated) && TargetedStateMachine.customName != "WeatherEvent")
            {
                Log($"No empty state machines to play event on! Aborting");
                return false;
            }
            return true;
        }

        private bool IsEventBeingPlayed(EventCard card)
        {
            if(NetworkStateMachine)
            {
                foreach(var statemachine in NetworkStateMachine.stateMachines)
                {
                    if (statemachine.state.GetType().Equals(card.eventState.stateType))
                        return true;
                }
            }
            return false;
        }

        private void FindIdleStateMachine()
        {
            if(NetworkStateMachine)
            {
                foreach(var stateMachine in NetworkStateMachine.stateMachines)
                {
                    if(stateMachine.state.GetType().Equals(stateMachine.mainStateType.stateType))
                    {
                        TargetedStateMachine = stateMachine;
                        return;
                    }
                }
            }
        }

        private void Log(string msg)
        {
            if (EnableInternalLogging)
                MSULog.Info($"\n-----o-----\nEvent Director: {msg}\n-----o-----");
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
        ///Commands
        ///------------------------------------------------------------------------------------------------------------

        [ConCommand(commandName = "force_event", flags = ConVarFlags.ExecuteOnServer, helpText = "Forces a gamewide event to begin. Argument is the event card's name")]
        private static void ForceEvent(ConCommandArgs args)
        {
            if(!Instance)
            {
                Debug.Log($"Event director is unavailable! Cannot start any events.");
                return;
            }

            string evArg = args.TryGetArgString(0).ToLowerInvariant();
            if(string.IsNullOrEmpty(evArg))
            {
                Debug.Log($"Command requires one string argument (Event card name)");
                return;
            }

            EventIndex eventIndex = EventCatalog.FindEventIndex(evArg);
            if(eventIndex == EventIndex.None)
            {
                Debug.Log($"Could not find an EventCard of name {evArg} (FindEventIndex returned EventIndex.None)");
                return;
            }

            EventCard card = EventCatalog.GetEventCard(eventIndex);
            if(card == null)
            {
                Debug.Log($"Could not get eventCard of name {evArg} (EventIndex {eventIndex} isnt tied to any event cards)");
                return;
            }

            if(!Instance.AttemptForceSpawnEvent(card))
            {
                Debug.Log($"Could not start event. too many events are playing.");
                return;
            }
        }

        [ConCommand(commandName = "stop_events", flags = ConVarFlags.ExecuteOnServer, helpText = "Forces all active events to stop")]
        private static void StopEvents(ConCommandArgs args)
        {
            if(!Instance)
            {
                Debug.Log($"Event director is unavailable! Cannot stop any events");
                return;
            }
            int count = Instance.StopAllEvents();
            Debug.Log($"Stopped {count} events");
        }

        /// <summary>
        /// Wether the event director is disabled or enabled
        /// </summary>
        public static bool DisableEventDirector => cvDisableEventDirector.value;

        /// <summary>
        /// Wether the event director logs information
        /// </summary>
        public static bool EnableInternalLogging => cvEnableInternalEventDirectorLogging.value;
        private static readonly BoolConVar cvDisableEventDirector = new BoolConVar("disable_events", ConVarFlags.SenderMustBeServer | ConVarFlags.Cheat, "0", "Disable the Event Director");
        private static readonly BoolConVar cvEnableInternalEventDirectorLogging = new BoolConVar("enable_event_logging", ConVarFlags.None, "0", "Enables the event director to print internal logging.");
    }
}
/*using Moonstorm.ScriptableObjects;
using RoR2;
using RoR2.ConVar;
using UnityEngine;
using UnityEngine.Networking;


namespace Moonstorm.Components
{
    public class EventDirector : MonoBehaviour
    {
        public static EventDirector instance;
        public static float eventCredit;

        public NetworkStateMachine networkStateMachine;
        public EventFunctions eventFunctions;

        public WeightedSelection<EventDirectorCard> eventCardsSelection;
        public EventDirectorCard currentEventCard;
        public SceneWeatherController.WeatherParams weatherParamsWhenSceneStarted;
        public string weatherRtpcWhenStarted = "";

        public EventDirectorCard LastAttemptedEventCard { get; set; }
        public EventDirectorCard LastSuccessfulEventCard { get; set; }

        public float GetCurrentDifficultyScalingValue { get => DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue; }

        private EventCardDeck eventCards;

        public static Vector2 random = new Vector2(15, 55);
        public static float randomMin = 0.5f;

        public void OnEnable()
        {
            instance = this;
        }
        public void OnDisable()
        {
            instance = null;
        }

        public void Awake()
        {
            if (NetworkServer.active && Run.instance && SceneInfo.instance && SceneInfo.instance.sceneDef)
            {
                networkStateMachine = GetComponent<NetworkStateMachine>();
                eventFunctions = GetComponent<EventFunctions>();

                rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);
                eventCards = EventCatalog.GetCurrentStageEvents();
                //eventCards.ValidateCards();
                eventCardsSelection = eventCards.GenerateDirectorCardWeightedSelection();
            }
        }

        private void FixedUpdate()
        {
            if (cvDirectorEventDisable.value)
                return;

            if (NetworkServer.active && Run.instance)
            {
                if (!Run.instance.isRunStopwatchPaused)
                {
                    interval -= Time.fixedDeltaTime;
                    if (interval <= 0)
                    {
                        interval = rng.RangeFloat(random.x, random.y);
                        float runDifficulty = Run.instance.difficultyCoefficient;
                        float runDifficultyCap = GetCurrentDifficultyScalingValue * 5f;
                        float add = rng.RangeFloat(runDifficulty * randomMin, runDifficultyCap);
                        //LogCore.LogM(add);
                        eventCredit += add;
                        if (eventCardsSelection != null)
                            AttemptSpawnEvent();
                    }
                }
            }
        }

        private bool AttemptSpawnEvent()
        {
            currentEventCard = eventCardsSelection.Evaluate(rng.nextNormalizedFloat);
            if (!currentEventCard.CardIsValid())
            {
                if (cvDirectorEventEnableInternalLogs.value)
                    Debug.Log($"Event card {currentEventCard.identifier} is invalid. Aborting.");
                return false;
            }
            if (eventCredit < currentEventCard.Cost)
            {
                if (cvDirectorEventEnableInternalLogs.value)
                    Debug.Log($"Spawn card {currentEventCard.identifier} is too expensive. Aborting.");
                return false;
            }
            float repeatWeight = Mathf.Pow(currentEventCard.repeatedSelectionWeight, consecutiveEventSpawn);
            if (eventCredit < currentEventCard.Cost / repeatWeight)
            {
                if (cvDirectorEventEnableInternalLogs.value)
                    Debug.Log($"Event card {currentEventCard.identifier} has played consecutively {consecutiveEventSpawn} times and is now {Mathf.Round(currentEventCard.Cost / repeatWeight * 100f) / 100f} credits. It is no longer affordable. Aborting.");
                return false;
            }
            if (IsEventBeingPlayed(currentEventCard))
            {
                if (cvDirectorEventEnableInternalLogs.value)
                    Debug.Log($"Event card {currentEventCard.identifier} is already playing. Aborting.");
                return false;
            }
            EntityStateMachine targetStateMachine = FindIdleStateMachine();
            if (!targetStateMachine)
            {
                if (cvDirectorEventEnableInternalLogs.value)
                    Debug.Log("No empty state machines to play event on. Aborting.");
                return false;
            }
            if (currentEventCard.CheckFlag(EventDirectorCard.EventFlags.Weather) && targetStateMachine.customName != "WeatherEvent")
            {
                if (cvDirectorEventEnableInternalLogs.value)
                    Debug.Log("Weather event already playing. Aborting.");
                return false;
            }
            targetStateMachine.SetState(currentEventCard.InstantiateNextState());

            //This is what makes runtime events one-time
            if (currentEventCard.CheckFlag(EventDirectorCard.EventFlags.OncePerRun))
                eventFunctions.RunSetFlag(currentEventCard.OncePerRunFlag);

            if (currentEventCard == LastSuccessfulEventCard)
                consecutiveEventSpawn++;
            else
                consecutiveEventSpawn = 0;

            LastSuccessfulEventCard = currentEventCard;

            eventCredit -= currentEventCard.directorCreditCost;
            return true;
        }

        private void PrepareNewEvent(EventDirectorCard eventCard)
        {
        }

        private bool AttemptForceSpawnEvent(EntityStateMachine stateMachine, EventDirectorCard card)
        {
            if (card && stateMachine && !IsEventBeingPlayed(card))
            {
                stateMachine.SetState(card.InstantiateNextState());
                return true;
            }
            return false;
        }

        /*private EventDirectorCard GenerateNextEvent()
        {
            int selection = 8;
            WeightedSelection<EventDirectorCard> weightedSelection = new WeightedSelection<EventDirectorCard>(selection);
            int count = eventCards.GenerateDirectorCardWeightedSelection(selection).Count;
            for (int i = 0; i < count; i++)
            {
                LogCore.LogM("Oi there bruv");
                var choice = eventCardsSelection.GetChoice(i);
                var card = choice.value;

                if (!IsEventBeingPlayed(card))
                {
                    LogCore.LogM("Adding choice");
                    weightedSelection.AddChoice(choice);
                }
            }
            if (weightedSelection.Count > 0)
                return weightedSelection.Evaluate(rng.nextNormalizedFloat);
            return null;
        }

        public EntityStateMachine FindIdleStateMachine()
        {
            if (networkStateMachine)
                foreach (var stateMachine in networkStateMachine.stateMachines)
                    //If it's in its idle state
                    if (stateMachine.state.GetType().Equals(stateMachine.mainStateType.stateType))
                        return stateMachine;
            return null;
        }

        public bool IsEventBeingPlayed(EventDirectorCard card)
        {
            if (networkStateMachine)
                foreach (var stateMachine in networkStateMachine.stateMachines)
                    if (stateMachine.state.GetType().Equals(card.activationState.stateType))
                        return true;
            return false;
        }

        public int StopAllEvents()
        {
            int eventsStopped = 0;
            foreach (var stateMachine in networkStateMachine.stateMachines)
            {
                if (!stateMachine.state.GetType().Equals(stateMachine.mainStateType.stateType))
                {
                    stateMachine.SetNextStateToMain();
                    eventsStopped++;
                }
            }
            return eventsStopped;
        }

        public void OnDestroy()
        {
        }

        private float interval;
        /// <summary>How many times the same event has spawned.</summary>
        private int consecutiveEventSpawn;
        private Xoroshiro128Plus rng;
        [SerializeField]
        private readonly EventSceneDeck _eventCards;


        /// Commands
        ///-------------------------------------------------------------------------------------

        [ConCommand(commandName = "force_event", flags = ConVarFlags.ExecuteOnServer, helpText = "Forces a gamewide event to begin. Argument is director card name.")]
        private static void ForceEvent(ConCommandArgs args)
        {
            if (instance)
            {
                string evArg = args.TryGetArgString(0);
                if (!string.IsNullOrEmpty(evArg))
                {
                    EventCatalog.TryFindDirectorCard(evArg, out var card);
                    if (!card)
                    {
                        Debug.Log("Could not start event. Event was not found.");
                        return;
                    }
                    if (!instance.AttemptForceSpawnEvent(instance.FindIdleStateMachine(), card))
                    {
                        Debug.Log("Could not start event. Too many events are playing.");
                        return;
                    }
                }
                else
                    Debug.Log("Command requires one string argument (director card identifier).");
            }
            else
                Debug.Log("Event director is unavailable! Cannot start any events.");

        }

        [ConCommand(commandName = "stop_events", flags = ConVarFlags.ExecuteOnServer, helpText = "Forces all active events to stop.")]
        private static void StopEvents(ConCommandArgs args)
        {
            if (instance)
            {
                int ct = instance.StopAllEvents();
                Debug.Log($"Stopped {ct} events.");
            }
            else
                Debug.Log("Event director is unavailable! Cannot start any events.");
        }

        public static readonly BoolConVar cvDirectorEventDisable = new BoolConVar("ss2_director_event_disable", ConVarFlags.SenderMustBeServer | ConVarFlags.Cheat, "0", "Disables all event directors.");

        private static readonly BoolConVar cvDirectorEventEnableInternalLogs = new BoolConVar("director_event_enable_internal_logs", ConVarFlags.None, "0", "Enables event director to print internal logging.");
    }
}*/