using BepInEx;
using RoR2;
using RoR2.ConVar;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Components
{
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
                    MSULog.Fatal($"COULD NOT RETRIEVE EVENT CATEGORY FOR SCENE {SceneInfo.instance.sceneDef}!!!");
#if DEBUG
                    Log($"Destroying root");
                    Log(null);
#endif
                    Destroy(gameObject.transform.root.gameObject);
                    return;
                }
                EventCardSelection = EventDirectorCategorySelection.GenerateWeightedSelection();
                eventToAmountsPlayed = EventCardSelection.GetValues().ToDictionary(x => x.EventIndex, y => 0);
#if DEBUG
                Log("Final Card Selection: " + string.Join("\n", EventCardSelection.choices.Where(x => x.value).Select(x => x.value)));
                Log(null);
#endif
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
#if DEBUG
            if (!msEnableEvents.value)
            {
                return;
            }
#endif
            if (NetworkServer.active && Run.instance)
            {
                if (!Run.instance.isRunStopwatchPaused)
                {
                    intervalStopWatch -= Time.fixedDeltaTime;
                    if (intervalStopWatch <= 0)
                    {
#if DEBUG
                        Log($"Stopwatch Ended, beggin Logging.\n-------o-Event Director Log-o-------");
#endif
                        float newStopwatchVal = eventRNG.RangeFloat(intervalResetRange.min, intervalResetRange.max);
#if DEBUG
                        Log($">new stopwatch value: {newStopwatchVal}");
#endif
                        intervalStopWatch = newStopwatchVal;

                        float eventScaling = GetEventScaling();

                        float newCredits = eventRNG.RangeFloat(creditGainRange.min, creditGainRange.max) * eventScaling;
                        eventCredits += newCredits;

#if DEBUG
                        Log($">new Credits: {newCredits}" +
                                $"\nTotal credits so far: {eventCredits}");
#endif
                        Simulate();
#if DEBUG
                        Log(null);
#endif
                    }
                }
            }
        }

        private float GetEventScaling()
        {
            float currentStage = Run.instance.stageClearCount + 1;
            float stageModifier = currentStage / Run.stagesPerLoop;
            float playerTeamLevel = TeamManager.instance.GetTeamLevel(TeamIndex.Player);
            float scaling = playerTeamLevel * stageModifier * GetDifficultyScalingValue;
#if DEBUG
            Log($">Selected Difficulty Scaling Coefficient: {GetDifficultyScalingValue}");
            Log($">Current Stage: {currentStage}");
            Log($">PlayerTeamLevel: {playerTeamLevel}");
            Log($">Drizzle Scaling: {playerTeamLevel * stageModifier * 1}");
            Log($">Rainstorm Scaling: {playerTeamLevel * stageModifier * 2}");
            Log($">Monsoom Scaling: {playerTeamLevel * stageModifier * 3}");
            Log($">Selected Difficulty Scaling: {scaling}");
#endif
            return scaling;
        }

        private void Simulate()
        {
            if (AttemptSpawnEvent())
            {
#if DEBUG
                Log($">Spawn succesful, setting currentEventCard to Null");
#endif
                currentEventCard = null;
                return;
            }
        }
        private bool AttemptSpawnEvent()
        {
            bool canSpawn = false;
            if (currentEventCard == null || currentEventCard == LastAttemptedEventCard)
            {
#if DEBUG
                Log($">Current event card is null, picking new one");
#endif
                if (EventCardSelection.Count == 0)
                {
#if DEBUG
                    Log($">Cannot pick a card when there's no cards in the EventCardSelection (Count: {EventCardSelection.Count})");
#endif
                    return false;
                }
                canSpawn = PrepareNewEvent(EventCardSelection.Evaluate(eventRNG.nextNormalizedFloat));

                if (!canSpawn)
                    return false;
            }
            float effectiveCost = currentEventCard.GetEffectiveCost(eventToAmountsPlayed[currentEventCard.EventIndex]);
#if DEBUG
            Log($">Playing event {currentEventCard}" +
                $"\n(Event state: {currentEventCard.eventState})");
#endif
            TargetedStateMachine.SetState(EntityStateCatalog.InstantiateState(ref currentEventCard.eventState));

            if (currentEventCard.eventFlags.HasFlag(EventFlags.OncePerRun))
            {
#if DEBUG
                Log($">Card {currentEventCard} has OncePerRun flag, setting flag.");
#endif
                EventFunctions.RunSetFlag(currentEventCard.OncePerRunFlag);
            }

            eventToAmountsPlayed[currentEventCard.EventIndex]++;

            LastSuccesfulEventCard = currentEventCard;


            eventCredits -= effectiveCost;
            TotalCreditsSpent += effectiveCost;
#if DEBUG
            Log($">Subtracted {effectiveCost} credits" +
                $"\nTotal credits spent: {TotalCreditsSpent}");
#endif

            return true;
        }

        private bool PrepareNewEvent(EventCard card)
        {
#if DEBUG
            Log($">Preparing event {card}");
#endif
            currentEventCard = card;
            if (!card.IsAvailable())
            {
#if DEBUG
                Log($">Event card {card.name} is not available! Aborting.");
#endif
                LastAttemptedEventCard = currentEventCard;
                return false;
            }
            float effectiveCost = currentEventCard.GetEffectiveCost(eventToAmountsPlayed[currentEventCard.EventIndex]);
            if (eventCredits < effectiveCost)
            {
#if DEBUG
                Log($">Event card {card.name} is too expensive! (It costs {effectiveCost}, current credits are {eventCredits}), Aborting");
#endif
                LastAttemptedEventCard = currentEventCard;
                return false;
            }
            if (IsEventBeingPlayed(card))
            {
#if DEBUG
                Log($">Event card {card.name} is already playing! Aborting");
#endif
                LastAttemptedEventCard = currentEventCard;
                return false;
            }
            if (Run.instance.GetEventFlag(currentEventCard.OncePerRunFlag))
            {
#if DEBUG
                Log($">Event card {card.name} already played! Aborting");
#endif
                LastAttemptedEventCard = currentEventCard;
                return false;
            }
            FindIdleStateMachine(currentEventCard);
            if (!TargetedStateMachine)
            {
#if DEBUG
                Log($">No empty state machines to play event on! Aborting");
#endif
                LastAttemptedEventCard = currentEventCard;
                return false;
            }

            var teleporterInstance = TeleporterInteraction.instance;
            if (teleporterInstance)
            {
                if (teleporterInstance.isCharged || teleporterInstance.isInFinalSequence)
                {
#if DEBUG
                    Log($">Stage has a teleporter instance and the teleporter is Charged or in it's final sequence, aborting.");
#endif
                    LastAttemptedEventCard = currentEventCard;
                    return false;
                }

                if (teleporterInstance.chargePercent > 25)
                {
#if DEBUG
                    Log($">Stage has a teleporter instance and it's charge percent is over 25%, aborting.");
#endif
                    LastAttemptedEventCard = currentEventCard;
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

        private void FindIdleStateMachine(EventCard card)
        {
            if (!card.requiredStateMachine.IsNullOrWhiteSpace())
            {
                EntityStateMachine requiredStateMachine = EntityStateMachine.FindByCustomName(gameObject, card.requiredStateMachine);
                if (!requiredStateMachine)
                {
#if DEBUG
                    MSULog.Error($"The card {card} requires a state machine with the name {card.requiredStateMachine}, but no such machine exists in the event director!");
#endif
                    TargetedStateMachine = null;
                    return;
                }
                TargetedStateMachine = requiredStateMachine;
                return;
            }

            foreach (var stateMachine in NetworkStateMachine.stateMachines)
            {
                if (stateMachine.customName.StartsWith("Generic") && stateMachine.state.GetType().Equals(stateMachine.mainStateType.stateType))
                {
                    TargetedStateMachine = stateMachine;
                    return;
                }
            }
        }

        public static bool AddNewEntityStateMachine(string stateMachineName)
        {
            GameObject prefab = MoonstormSharedUtils.MSUAssetBundle.LoadAsset<GameObject>("MSUEventDirector");
            bool machineWithNameExists = EntityStateMachine.FindByCustomName(prefab, stateMachineName);
            if (machineWithNameExists)
            {
#if DEBUG
                MSULog.Warning($"An entity state machine with name {stateMachineName} already exists in the EventDirector.");
#endif
                return false;
            }

            NetworkStateMachine networker = prefab.GetComponent<NetworkStateMachine>();
            EntityStateMachine newMachine = prefab.AddComponent<EntityStateMachine>();
            newMachine.customName = stateMachineName;
            var idleState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle));
            newMachine.initialStateType = idleState;
            newMachine.mainStateType = idleState;
            HG.ArrayUtils.ArrayAppend(ref networker.stateMachines, newMachine);
#if DEBUG
            MSULog.Info($"Succesfully added custom state machine named {stateMachineName} to the EventDirector");
#endif
            return true;
        }
#if DEBUG
        private void Log(string msg)
        {
            if (!msEnableEventLogging.value)
            {
                return;
            }
            if (string.IsNullOrEmpty(msg))
            {
                MSULog.Info(log.ToString());
                log.Clear();
            }
            log.AppendLine(msg);
        }
        private bool AttemptForceSpawnEvent(EventCard card)
        {
            FindIdleStateMachine(card);
            if (card && TargetedStateMachine && !IsEventBeingPlayed(card))
            {
                TargetedStateMachine.SetState(EntityStateCatalog.InstantiateState(ref card.eventState));
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

        private bool PlayEventDebug(EventCard card)
        {
            bool canSpawn = false;
            canSpawn = PrepareNewEventDebug(card);
            if (!canSpawn)
                return false;

            float effectiveCost = card.GetEffectiveCost(eventToAmountsPlayed[card.EventIndex]);

            Log($">Playing event {card}\n(Event state: {card.eventState})");

            TargetedStateMachine.SetState(EntityStateCatalog.InstantiateState(ref card.eventState));

            if (card.eventFlags.HasFlag(EventFlags.OncePerRun))
            {
#if DEBUG
                Log($">Card {card} has OncePerRun flag, setting flag.");
#endif
                EventFunctions.RunSetFlag(card.OncePerRunFlag);
            }

            eventToAmountsPlayed[card.EventIndex]++;

            eventCredits -= effectiveCost;
            TotalCreditsSpent += effectiveCost;

#if DEBUG
            Log($">Subtracted {effectiveCost} credits" +
                $"\nTotal credits spent: {TotalCreditsSpent}");
#endif

            return true;
        }

        private bool PrepareNewEventDebug(EventCard card)
        {
#if DEBUG
            Log($">Preparing event {card}");
#endif
            if (!card.IsAvailable())
            {
#if DEBUG
                Log($">Event card {card.name} is not available! Aborting.");
#endif
                return false;
            }
            float effectiveCost = card.GetEffectiveCost(eventToAmountsPlayed[card.EventIndex]);
            if (eventCredits < effectiveCost)
            {
#if DEBUG
                Log($">Event card {card.name} is too expensive! (It costs {effectiveCost}, current credits are {eventCredits}), Aborting");
#endif
                return false;
            }
            if (IsEventBeingPlayed(card))
            {
#if DEBUG
                Log($">Event card {card.name} is already playing! Aborting");
#endif
                return false;
            }
            if (Run.instance.GetEventFlag(card.OncePerRunFlag))
            {
#if DEBUG
                Log($">Event card {card.name} already played! Aborting");
#endif
                return false;
            }
            FindIdleStateMachine(card);
            if (!TargetedStateMachine)
            {
#if DEBUG
                Log($">No empty state machines to play event on! Aborting");
#endif
                return false;
            }

            var teleporterInstance = TeleporterInteraction.instance;
            if (teleporterInstance)
            {
                if (teleporterInstance.isCharged || teleporterInstance.isInFinalSequence)
                {
#if DEBUG
                    Log($">Stage has a teleporter instance and the teleporter is Charged or in it's final sequence, aborting.");
#endif
                    return false;
                }

                if (teleporterInstance.chargePercent > 25)
                {
#if DEBUG
                    Log($">Stage has a teleporter instance and it's charge percent is over 25%, aborting.");
#endif
                    return false;
                }
            }
            return true;
        }
        ///Commands
        ///------------------------------------------------------------------------------------------------------------

        [ConCommand(commandName = "ms_add_credits", flags = ConVarFlags.ExecuteOnServer, helpText = "Adds the desired amount of credits to the Event Director. Argument is a float value, can be negative.")]
        private static void AddCredits(ConCommandArgs args)
        {
            if (!Instance)
            {
                Debug.Log($"Event director is unavailable! Cannot start any events.");
                return;
            }

            string num = args.TryGetArgString(0);
            if (float.TryParse(num, NumberStyles.Float, CultureInfo.InvariantCulture, out float creditsToAdd))
            {
                Instance.eventCredits += creditsToAdd;
                Debug.Log($"Added {creditsToAdd} to Event Director's Credits.");
                return;
            }

            Debug.Log($"Float parse error, could not parse {num}");
        }
        [ConCommand(commandName = "ms_play_event", flags = ConVarFlags.ExecuteOnServer, helpText = "Tries to start an event, following the regular checks before it starts. Argument is the event card's name")]
        private static void PlayEvent(ConCommandArgs args)
        {
            if (!Instance)
            {
                Debug.Log($"Event director is unavailable! Cannot start any events.");
                return;
            }

            string cardName = args.TryGetArgString(0)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(cardName))
            {
                Debug.Log($"Command requires one string argument (Event card name)");
                return;
            }

            EventIndex eventIndex = EventCatalog.FindEventIndex(cardName);
            if (eventIndex == EventIndex.None)
            {
                Debug.Log($"Could not find an EventCard of name {cardName} (FindEventIndex returned EventIndex.None)");
                return;
            }

            EventCard card = EventCatalog.GetEventCard(eventIndex);
            if (!Instance.PlayEventDebug(card))
            {
                Debug.Log($"Could not start event.");
            }
            Instance.Log(null);
        }
        [ConCommand(commandName = "ms_force_event", flags = ConVarFlags.ExecuteOnServer, helpText = "Forces a gamewide event to begin. Argument is the event card's name")]
        private static void ForceEvent(ConCommandArgs args)
        {
            if (!Instance)
            {
                Debug.Log($"Event director is unavailable! Cannot start any events.");
                return;
            }

            string evArg = args.TryGetArgString(0)?.ToLowerInvariant();
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

        [ConCommand(commandName = "ms_stop_events", flags = ConVarFlags.ExecuteOnServer, helpText = "Forces all active events to stop")]
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

        private static BoolConVar msEnableEvents = new BoolConVar("ms_enable_events", ConVarFlags.ExecuteOnServer | ConVarFlags.SenderMustBeServer, "1", "Enable or Disable Events");

        private static BoolConVar msEnableEventLogging = new BoolConVar("ms_enable_event_logging", ConVarFlags.ExecuteOnServer, "1", "Enable or Disable verbose logging of the Event Director");
#endif
    }
}