using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm.Components
{
    // Chances are this entire system will get shafted once they start using the weather controller
    public class SetupWeatherController : MonoBehaviour
    {
        //It could potentially be worth it to reverse this list and add only scenes that are guaranteed to work.
        /// <summary>
        /// If a scene in the game, modded or unmodded, should not have weather changes, add it to this list.
        /// </summary>
        public static List<string> blacklistedScenes = new List<string>()
        {
            "arena",
            "bazaar",
            "artifactworld"
        };

        private SceneWeatherController weatherController;

        void Awake()
        {

            if (SceneWeatherController.instance)
            {
                weatherController = SceneWeatherController.instance;
                EventDirector.Instance.weatherParamsWhenSceneStarted = weatherController.initialWeatherParams;
                EventDirector.Instance.weatherRtpcWhenStarted = (string.IsNullOrEmpty(weatherController.rtpcWeather)) ? string.Empty : weatherController.rtpcWeather;
            }
            else if (!blacklistedScenes.Contains(SceneInfo.instance.sceneDef.baseSceneName))
            {
                weatherController = SceneInfo.instance.gameObject.AddComponent<SceneWeatherController>();
                weatherController.sun = FindSun();
                weatherController.fogMaterial = GetFogMaterial();
                weatherController.initialWeatherParams = GetInitialParams();
                weatherController.weatherLerpOverChargeTime = MoonstormSharedUtils.MSUAssetBundle.LoadAsset<AnimationCurveAsset>("curveLinear").value;
                EventDirector.Instance.weatherParamsWhenSceneStarted = weatherController.initialWeatherParams;
                weatherController.weatherLerp = 0f;
                EventDirector.Instance.weatherRtpcWhenStarted = (weatherController.rtpcWeather is null) ? "" : weatherController.rtpcWeather;
                weatherController.rtpcWeather = "";
            }
            Destroy(this);
        }


        /* Unfortunately, the only way to really find a sun is by finding the component that is attached to most of them.
         * In all current vanilla cases there is only one NGSS_Directional per scene, and it's always attached to the sun.
         * That doesn't necessarily mean that this will be the case in the future, as either of these cases failing will mess this up,
         * not to mention modding.
         */
        private Light FindSun()
        {
            var sunLight = FindObjectOfType<NGSS_Directional>()?.GetComponent<Light>(); ;
            if (!sunLight)
                MSULog.Warning("Could not find sun object.");
            return sunLight;
        }

        //TODO: set up fog shit
        private Material GetFogMaterial()
        {
            return null;
        }

        private SceneWeatherController.WeatherParams GetInitialParams()
        {
            return new SceneWeatherController.WeatherParams
            {
                sunColor = weatherController.sun ? weatherController.sun.color : Color.clear,
                sunIntensity = weatherController.sun ? weatherController.sun.intensity : 0
                //Add fog
            };
        }

    }
}