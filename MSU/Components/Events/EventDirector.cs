using Moonstorm.ScriptableObjects;
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
                        float runDifficultyCap = Run.instance.selectedDifficultyInternal * 5f;
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
        }*/

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
}