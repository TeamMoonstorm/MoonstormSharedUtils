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
    public class GameplayEventTextController : MonoBehaviour
    {
        public static GameplayEventTextController Instance { get; private set; }
        private static GameObject _prefab;

        public EntityStateMachine TextStateMachine { get; private set; }
        public HGTextMeshProUGUI TextMeshProUGUI { get; private set; }
        public HUD HUDInstance { get; private set; }
        private Queue<EventTextRequest> _textRequests = new Queue<EventTextRequest>();
        public EventTextRequest? CurrentTextRequest { get; private set; }

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
                TextStateMachine.SetNextState(new FadeInState
                {
                    duration = CurrentTextRequest.Value.textDuration / 3
                });
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

        public struct EventTextRequest
        {
            public string eventToken;
            public Color eventColor;
            public float textDuration;

            public string TokenValue => Language.GetString(eventToken);
            public Color GetBestOutlineColor()
            {
                Color.RGBToHSV(eventColor, out float hue, out float saturation, out float light);

                float modifier = light > 0.5f ? (-0.5f) : 0.5f;
                float newSaturation = Mathf.Clamp01(saturation + modifier);
                float newLight = Mathf.Clamp01(light + modifier);
                return Color.HSVToRGB(hue, newSaturation, newLight);
            }
        }

        public abstract class EventTextState : EntityState
        {
            public GameplayEventTextController TextController { get; private set; }
            public UIJuice Juice { get; private set; }

            public float duration;

            public override void OnEnter()
            {
                base.OnEnter();
                TextController = GetComponent<GameplayEventTextController>();
                Juice = GetComponent<UIJuice>();
            }
        }

        public class FadeInState : EventTextState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                Juice.destroyOnEndOfTransition = false;
                Juice.originalAlpha = 1;
                Juice.transitionDuration = duration;
                Juice.TransitionAlphaFadeIn();
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