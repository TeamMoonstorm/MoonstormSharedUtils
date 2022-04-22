using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using R2API;
using RoR2;
using EntityStates;
using RoR2.ExpansionManagement;
using Moonstorm.AddressableAssets;

namespace Moonstorm
{
    [CreateAssetMenu(menuName = "Moonstorm/Events/EventCard")]
    public class EventCard : ScriptableObject
    {
        public EventIndex EventIndex { get; internal set; }
        public string OncePerRunFlag { get => $"{name}:PlayedThisRun"; }

        [EnumMask(typeof(EventFlags))]
        public EventFlags eventFlags;
        public SerializableEntityStateType eventState;
        public string startMessageToken;
        public string endMessageToken;
        public Color messageColor = Color.white;
        public NetworkSoundEventDef startSound;

        [EnumMask(typeof(DirectorAPI.Stage))]
        public DirectorAPI.Stage availableStages;
        public List<string> customStageNames = new List<string>();
        public string category;
        public int selectionWeight;
        public int cost;
        public int minimumStageCompletions;
        public AddressableUnlockableDef requiredUnlockableDef;
        public AddressableUnlockableDef forbiddenUnlockableDef;
        public AddressableExpansionDef requiredExpansionDef;

        public bool IsAvailable()
        {
            if (!Run.instance)
                return false;

            bool flag0 = !requiredUnlockableDef.Asset || Run.instance.IsUnlockableUnlocked(requiredUnlockableDef.Asset);
            bool flag1 = forbiddenUnlockableDef.Asset && Run.instance.DoesEveryoneHaveThisUnlockableUnlocked(forbiddenUnlockableDef.Asset);
            //if enough stages are cleared, and unlock requirements are met
            if(Run.instance.stageClearCount >= minimumStageCompletions && flag0 && !flag1)
            {
                //If it doesnt have the flag or it does and the loop is greater than 0
                bool flag2 = !eventFlags.HasFlag(EventFlags.AfterLoop) || Run.instance.loopClearCount > 0;
                //If it doesnt have the flag or it does and the void fields have been visited
                bool flag3 = !eventFlags.HasFlag(EventFlags.AfterVoidFields) || Run.instance.GetEventFlag("ArenaPortalTaken");
                //If it isnt one-time or the the flag isnt registered for the run
                bool flag4 = !eventFlags.HasFlag(EventFlags.OncePerRun) || !Run.instance.GetEventFlag(OncePerRunFlag);

                if (!((flag2 || flag3) && flag4))
                    return false;

                //If event requires expansionDef
                if (requiredExpansionDef.Asset)
                {
                    return Run.instance.IsExpansionEnabled(requiredExpansionDef.Asset);
                }
                return true;
            }
            return false;
        }
    }
}
