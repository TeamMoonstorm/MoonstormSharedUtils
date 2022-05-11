using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Moonstorm.Components
{
    //This is different from the Post Process Duration component because it grabs it from the current pp's weight
    public class LerpPostProcessingWeight : MonoBehaviour
    {
        public PostProcessVolume postProcessVolume;
        public float endWeight;
        public float duration;
        [HideInInspector]
        public PostProcessProfile postProcessProfile;

        private float stopwatch;
        private float startWeight;

        private void Start()
        {
            startWeight = postProcessVolume.weight;
            //We only do this so it creates an instance if it hasn't
            postProcessProfile = postProcessVolume.profile;
            if (!postProcessVolume.HasInstantiatedProfile())
                MSULog.Error($"{postProcessVolume} has no instantiated profile. returning.");
        }

        void FixedUpdate()
        {
            postProcessVolume.weight = Mathf.Lerp(startWeight, endWeight, stopwatch / duration);
            if (stopwatch > duration)
                this.enabled = false;
            stopwatch += Time.fixedDeltaTime;
        }
    }
}