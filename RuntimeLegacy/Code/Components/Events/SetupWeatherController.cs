using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm.Components
{
    public class SetupWeatherController : MonoBehaviour
    {
        //It could potentially be worth it to reverse this list and add only scenes that are guaranteed to work.
        public static List<string> blacklistedScenes = new List<string>()
        {
            "arena",
            "bazaar",
            "artifactworld"
        };

        private SceneWeatherController weatherController;

        void Awake()
        {
            throw new System.NotImplementedException();
        }


        /* Unfortunately, the only way to really find a sun is by finding the component that is attached to most of them.
         * In all current vanilla cases there is only one NGSS_Directional per scene, and it's always attached to the sun.
         * That doesn't necessarily mean that this will be the case in the future, as either of these cases failing will mess this up,
         * not to mention modding.
         */
        private Light FindSun()
        {
            throw new System.NotImplementedException();
        }

        //TODO: set up fog shit
        private Material GetFogMaterial()
        {
            throw new System.NotImplementedException();
        }

        private SceneWeatherController.WeatherParams GetInitialParams()
        {
            throw new System.NotImplementedException();
        }

    }
}