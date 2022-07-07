using RoR2.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Moonstorm.Components
{
    /// <summary>
    /// The controller for an Event's Announcement text
    /// </summary>
    public class EventTextController : MonoBehaviour
    {
        /// <summary>
        /// The state of the event
        /// </summary>
        public enum EventFadeState
        {
            FadeIn,
            Wait,
            FadeOut,
        }

        /// <summary>
        /// UIJuice used for fading
        /// </summary>
        public UIJuice uiJuice;
        /// <summary>
        /// Wether the fade begins as soon as the event gets entered
        /// </summary>
        public bool fadeOnStart;
        /// <summary>
        /// How long the warning lasts
        /// </summary>
        public float warningDuration;

        private bool fading = false;
        private EventFadeState fadeState;
        private float internalStopwatch;
        private float actualWarningDuration;
        private void Start()
        {
            actualWarningDuration = warningDuration / 3;
            uiJuice.transitionDuration = actualWarningDuration;
            fadeState = EventFadeState.FadeIn;
            if (fadeOnStart)
                BeginFade();
        }
        
        /// <summary>
        /// Begins the EventTextController, only call this is <see cref="fadeOnStart"/> is false
        /// </summary>
        public void BeginFade()
        {
            switch(fadeState)
            {
                case EventFadeState.FadeIn:
                    uiJuice.destroyOnEndOfTransition = false;
                    fading = true;
                    uiJuice.TransitionAlphaFadeIn();
                    break;
                case EventFadeState.Wait:
                    fading = true;
                    break;
                case EventFadeState.FadeOut:
                    uiJuice.destroyOnEndOfTransition = true;
                    fading = true;
                    uiJuice.TransitionAlphaFadeOut();
                    break;
            }
        }

        private void Update()
        {
            if(fading)
            {
                internalStopwatch += Time.unscaledDeltaTime;
                if(internalStopwatch > warningDuration)
                {
                    FadeEnd();
                }
            }
        }

        private void FadeEnd()
        {
            fading = false;
            internalStopwatch = 0;

            if(fadeState == EventFadeState.FadeIn)
            {
                fadeState = EventFadeState.Wait;
                BeginFade();
                return;
            }
            else if(fadeState == EventFadeState.Wait)
            {
                fadeState = EventFadeState.FadeOut;
                BeginFade();
                return;
            }
            Destroy(gameObject);
        }
        /*public enum EventFadeState
        {
            FadeIn,
            Wait,
            FadeOut,
            DestroyASAP
        }

        [SerializeField]
        private HGTextMeshProUGUI textToFade;
        public bool fadeOnStart;

        [Space(5)]
        public float warningDuration;
        public float targetAlphaValue;
        public bool ignoreTimeScale;

        private bool fading = false;
        private EventFadeState fadeState;
        private float internalStopwatch;
        private float actualWarningDuration;
        private void Start()
        {
            actualWarningDuration = warningDuration / 3;
            fadeState = EventFadeState.FadeIn;
            if (fadeOnStart)
                BeginFade();
        }

        public void BeginFade()
        {
            fading = true;
            textToFade.alpha = Mathf.Lerp(textToFade.alpha, targetAlphaValue, actualWarningDuration);
            /*Color color = textToFade.color;
            color.a = targetAlphaValue;
            Debug.Log($"Fading {textToFade.color} to {color}");
            textToFade.CrossFadeColor(color, actualWarningDuration, false, true, false);
        }

        private void Update()
        {
            if (fadeState == EventFadeState.DestroyASAP)
                Destroy(gameObject);

            if(fading)
            {
                internalStopwatch += Time.deltaTime;
                if(internalStopwatch >= actualWarningDuration)
                {
                    FadeEnd();
                }
            }
        }

        private void FadeEnd()
        {
            fading = false;
            internalStopwatch = 0;

            switch(fadeState)
            {
                case EventFadeState.FadeIn:
                    {
                        fadeState = EventFadeState.Wait;
                        BeginFade();
                        break;
                    }
                case EventFadeState.Wait:
                    {
                        targetAlphaValue = 0;
                        fadeState = EventFadeState.FadeOut;
                        BeginFade();
                        break;
                    }
                case EventFadeState.FadeOut:
                    {
                        fadeState = EventFadeState.DestroyASAP;
                        Destroy(gameObject);
                        break;
                    }
            }
        }*/
    }
}