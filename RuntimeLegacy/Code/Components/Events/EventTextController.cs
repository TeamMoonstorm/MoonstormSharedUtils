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
            throw new System.NotImplementedException();
        }

        public void BeginFade()
        {
            throw new System.NotImplementedException();
        }

        private void Update()
        {
            throw new System.NotImplementedException();
        }

        private void FadeEnd()
        {
            throw new System.NotImplementedException();
        }
    }
}