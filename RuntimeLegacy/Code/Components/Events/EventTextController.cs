using RoR2.UI;
using UnityEngine;

namespace Moonstorm.Components
{
    public class EventTextController : MonoBehaviour
    {
        public enum EventFadeState
        {
            FadeIn,
            Wait,
            FadeOut,
        }

        public UIJuice uiJuice;

        public bool fadeOnStart;

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

        public void BeginFade()
        {
            switch (fadeState)
            {
                case EventFadeState.FadeIn:
                    uiJuice.destroyOnEndOfTransition = false;
                    fading = true;
                    uiJuice.originalAlpha = MSUConfig.maxOpacityForEventMessage;
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
            if (fading)
            {
                internalStopwatch += Time.unscaledDeltaTime;
                if (internalStopwatch > warningDuration)
                {
                    FadeEnd();
                }
            }
        }

        private void FadeEnd()
        {
            fading = false;
            internalStopwatch = 0;

            if (fadeState == EventFadeState.FadeIn)
            {
                fadeState = EventFadeState.Wait;
                BeginFade();
                return;
            }
            else if (fadeState == EventFadeState.Wait)
            {
                fadeState = EventFadeState.FadeOut;
                BeginFade();
                return;
            }
            Destroy(gameObject);
        }
    }
}