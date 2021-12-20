using Moonstorm.Components;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm
{
    internal static class Events
    {
        internal static void Init()
        {
            SceneDirector.onPrePopulateSceneServer += AddEventDirector;
            Run.onRunStartGlobal += ResetEventCredits;
        }

        private static void AddEventDirector(SceneDirector obj)
        {
            if (EventCatalog.HasAnyEventRegistered)
            {
                if (Run.instance && SceneInfo.instance.countsAsStage && NetworkServer.active)
                {
                    NetworkServer.Spawn(Object.Instantiate(MoonstormSharedUtils.mainAssetBundle.LoadAsset<GameObject>("MSUEventDirector")));
                }
            }
            else
            {
                MSULog.LogI($"No events are in the event catalog, aborting spawning the event director.");
            }
        }

        private static void ResetEventCredits(Run obj)
        {
            EventDirector.eventCredit = 0;
        }
    }
}
