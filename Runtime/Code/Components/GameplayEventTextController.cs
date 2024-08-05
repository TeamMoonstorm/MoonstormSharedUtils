using EntityStates;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    //TODO: Make it so family events are displayed with this too.
    /// <summary>
    /// The <see cref="GameplayEventTextController"/> is a Singleton component that manages the EventText system of <see cref="GameplayEvent"/>s.
    /// <br>The EventText are large, bold texts that appear when an event starts and when an event ends.</br>
    /// <pr>This text is automatically enqueued when spawning an Event via the <see cref="GameplayEventManager"/>, otherwise no text is enqueued when spawning <see cref="GameplayEvent"/>s manually.</pr>
    /// </summary>
    public sealed class GameplayEventTextController : MonoBehaviour
    {
        /// <summary>
        /// Returns the current instance of the GameplayEventTextController.
        /// </summary>
        public static GameplayEventTextController Instance { get; private set; }
        private static GameObject _prefab;

        /// <summary>
        /// The EntityStateMachine that's running the required fade in/out states of the text.
        /// </summary>
        public EntityStateMachine TextStateMachine { get; private set; }

        /// <summary>
        /// The <see cref="HGTextMeshProUGUI"/> component that's displaying the event text.
        /// </summary>
        public HGTextMeshProUGUI TextMeshProUGUI { get; private set; }

        /// <summary>
        /// The HUD instance that this GameplayEventTextController is a child of.
        /// </summary>
        public HUD HUDInstance { get; private set; }
        private Queue<EventTextRequest> _textRequests = new Queue<EventTextRequest>();

        /// <summary>
        /// Returns the current event text request that's being processed.
        /// </summary>
        public EventTextRequest? CurrentTextRequest { get; private set; }

        private TMPro.TMP_FontAsset _bombadierFontAsset;

        [SystemInitializer]
        private static void SystemInit()
        {
            _prefab = MSUMain.MSUAssetBundle.LoadAsset<GameObject>("GameplayEventText");
            On.RoR2.UI.HUD.Awake += SpawnAndGetInstance;
        }

        private static void SpawnAndGetInstance(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            GameObject.Instantiate(_prefab, self.mainContainer.transform);
            Instance.HUDInstance = self;
        }

        [ConCommand(commandName = "test_event_text", flags = ConVarFlags.None, helpText = "Tests the GameplayEventTextController with a generic EventTextRequest")]
        private static void CC_TestEventText(ConCommandArgs args)
        {
            if (!Instance)
                return;

            Instance.EnqueueNewTextRequest(new EventTextRequest
            {
                eventToken = "Event Text Test",
                eventColor = Color.cyan,
                textDuration = 15
            });
        }

        /// <summary>
        /// Enqueues a new <see cref="EventTextRequest"/> to be displayed
        /// </summary>
        /// <param name="request">The EventText to display</param>
        public void EnqueueNewTextRequest(EventTextRequest request)
        {
            _textRequests.Enqueue(request);
        }

        private void Update()
        {
            //no request being processed, and there's a pending request, dequeue and initialize.
            if(!CurrentTextRequest.HasValue && _textRequests.Count > 0)
            {
                DequeueAndInitializeRequest();
                return;
            }

            //If there is a request, and its not being proceessed, process it.
            if(CurrentTextRequest.HasValue && TextStateMachine.state is Idle)
            {
                var value = CurrentTextRequest.Value;

                var hasOverrideState = value.customTextState.HasValue;
                EventTextState state = hasOverrideState ? (EventTextState)EntityStateCatalog.InstantiateState(value.customTextState.Value) : new FadeInState();
                state.duration = hasOverrideState ? value.textDuration : value.textDuration / 3;
                TextStateMachine.SetNextState(state);
            }
        }

        internal void NullCurrentRequest()
        {
            CurrentTextRequest = null;
        }

        private void DequeueAndInitializeRequest()
        {
            CurrentTextRequest = _textRequests.Dequeue();
            string tokenValue = CurrentTextRequest.Value.TokenValue;
            Color messageColor = CurrentTextRequest.Value.eventColor;
            Color outlineColor = CurrentTextRequest.Value.GetBestOutlineColor();

            TextMeshProUGUI.text = tokenValue;
            TextMeshProUGUI.color = messageColor;
            TextMeshProUGUI.outlineColor = outlineColor;
            TextMeshProUGUI.font = CurrentTextRequest.Value.customFontAsset.HasValue ? CurrentTextRequest.Value.customFontAsset : _bombadierFontAsset;
        }

        private void Start()
        {
            _bombadierFontAsset = TextMeshProUGUI.font;
        }

        private void Awake()
        {
            TextStateMachine = GetComponent<EntityStateMachine>();
            TextMeshProUGUI = GetComponent<HGTextMeshProUGUI>();
        }

        private void OnEnable()
        {
            Instance = this;
            TextMeshProUGUI.text = string.Empty;
        }
        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Struct that represents a request to display an EventText.
        /// </summary>
        public struct EventTextRequest
        {
            /// <summary>
            /// The token to use for the text, the <see cref="GameplayEventTextController"/> uses <see cref="TokenValue"/> to obtain the correct string in the current language.
            /// </summary>
            public string eventToken;
            /// <summary>
            /// The color for this event text, the outline for this event text is calculated using <see cref="GetBestOutlineColor"/>
            /// </summary>
            public Color eventColor;

            /// <summary>
            /// The total duration that this event text should last. If <see cref="customTextState"/>'s value is null, this value will be passed to <see cref="FadeInState"/> with the duration value of <see cref="textDuration"/> / 3 (IE: A text duration of 6 will cause the FadeIn state to last 2 seconds, The wait state 2 seconds, and the FadeOut state to last 2 seconds)
            /// <para>In case <see cref="customTextState"/> is not null, this value will be passed raw to the state specified in <see cref="customTextState"/></para>
            /// </summary>
            public float textDuration;

            /// <summary>
            /// If supplied, the <see cref="GameplayEventTextController.TextStateMachine"/>'s state will be set to this state, The state needs to inherit from <see cref="EventTextState"/>.
            /// <br>remember to eventually set the state machine's state back to main, otherwise no more text requests will be processed.</br>
            /// <br>The final state (the one that sets the state machine's state back to main) should also call <see cref="EventTextState.NullRequest"/> for proper disposal of the request.</br>
            /// </summary>
            public SerializableEntityStateType? customTextState;

            /// <summary>
            /// If supplied, this font asset is used for the lifetime of this Request, if no FontAsset is supplied, the base game's "Bombardier" font is used.
            /// </summary>
            public NullableRef<TMPro.TMP_FontAsset> customFontAsset;

            /// <summary>
            /// The value of <see cref="eventToken"/> using the currently loaded language
            /// </summary>
            public string TokenValue => Language.GetString(eventToken);

            /// <summary>
            /// Obtains the best outline color to use with <see cref="eventColor"/>
            /// <br>This color is calculated depending on the <see cref="eventColor"/>'s light value</br>.
            /// </summary>
            /// <returns>The best outline color</returns>
            public Color GetBestOutlineColor()
            {
                Color.RGBToHSV(eventColor, out float hue, out float saturation, out float light);

                float modifier = light > 0.5f ? (-0.5f) : 0.5f;
                float newSaturation = Mathf.Clamp01(saturation + modifier);
                float newLight = Mathf.Clamp01(light + modifier);
                return Color.HSVToRGB(hue, newSaturation, newLight);
            }
        }

        /// <summary>
        /// The base class for all EventText related entity states
        /// </summary>
        public abstract class EventTextState : EntityState
        {
            /// <summary>
            /// Returns the GameplayEventTextcontroller that created this state.
            /// </summary>
            public GameplayEventTextController TextController { get; private set; }

            /// <summary>
            /// The UIJuice component attached to the <see cref="GameplayEventTextController"/>
            /// </summary>
            public UIJuice Juice { get; private set; }

            /// <summary>
            /// The total duration this state should last.
            /// </summary>
            public float duration;

            public override void OnEnter()
            {
                base.OnEnter();
                TextController = GetComponent<GameplayEventTextController>();
                Juice = GetComponent<UIJuice>();
            }

            /// <summary>
            /// Method used for nulling the current request in the TExtController, usually the request that instantiated this state.
            /// </summary>
            protected virtual void NullRequest()
            {
                TextController.NullCurrentRequest();
            }
        }

        /// <summary>
        /// State used for EventTexts where the Text fades in
        /// </summary>
        public class FadeInState : EventTextState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                Juice.destroyOnEndOfTransition = false;
                Juice.transitionDuration = duration;
                Juice.TransitionAlphaFadeIn();
                Juice.originalAlpha = 1;
                Juice.transitionEndAlpha = 1;
            }

            public override void Update()
            {
                base.Update();
                if(age > duration)
                {
                    outer.SetNextState(new WaitState
                    {
                        duration = duration
                    });
                }
            }
        }

        /// <summary>
        /// State used for EventTexts where the text has faded in completely and is now waiting
        /// </summary>
        public class WaitState : EventTextState
        {
            public override void Update()
            {
                base.Update();
                if(age > duration)
                {
                    outer.SetNextState(new FadeOutState
                    {
                        duration = duration
                    });
                }
            }
        }

        /// <summary>
        /// State used for EventTexts where the text has finished waiting and is now fading out
        /// </summary>
        public class FadeOutState : EventTextState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                Juice.TransitionAlphaFadeOut();
            }

            public override void Update()
            {
                base.Update();
                if(age > duration)
                {
                    TextController.NullCurrentRequest();
                    outer.SetNextStateToMain();
                }
            }
        }
    }
}