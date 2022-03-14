using Moonstorm.Components;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm
{
    /*internal static class Events
    {
        internal static void Init()
        {
            SceneDirector.onPrePopulateSceneServer += AddEventDirector;
            Run.onRunStartGlobal += ResetEventCredits;
        }

        private static void AddEventDirector(SceneDirector obj)
        {
            if(!EventCatalog.HasAnyEventRegistered)
            {
                MSULog.Info($"No events are in the event catalog, aborting spawning the event director.");
                return;
            }
            
            if (Run.instance && SceneInfo.instance.countsAsStage && NetworkServer.active)
            {
                NetworkServer.Spawn(Object.Instantiate(MoonstormSharedUtils.MSUAssetBundle.LoadAsset<GameObject>("MSUEventDirector")));
            }
        }

        private static void ResetEventCredits(Run obj)
        {
            EventDirector.eventCredit = 0;
        }
    }*/
}
