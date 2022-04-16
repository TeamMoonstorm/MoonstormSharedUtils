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

        [EnumMask(typeof(EventFlags))]
        public EventFlags eventFlags;
        public SerializableEntityStateType eventState;
        public string startMessageToken;
        public string endMessageToken;
        public Color messageColor = Color.white;
        public NetworkSoundEventDef startSound;

        [EnumMask(typeof(DirectorAPI.Stage))]
        public DirectorAPI.Stage availableStages;
        public List<string> availableCustomStages = new List<string>();
        public int selectionWeight;
        public int cost;
        public int minimumStageCompletions;
        public AddressableUnlockableDef requiredUnlockableDef;
        public AddressableUnlockableDef forbiddenUnlockableDef;
        public AddressableExpansionDef requiredExpansionDef;

        public bool IsAvailable()
        {
            bool flag = !requiredUnlockableDef.Asset || Run.instance.IsUnlockableUnlocked(requiredUnlockableDef.Asset);
            bool flag2 = forbiddenUnlockableDef.Asset && Run.instance.DoesEveryoneHaveThisUnlockableUnlocked(forbiddenUnlockableDef.Asset);
            if(Run.instance && Run.instance.stageClearCount >= minimumStageCompletions && flag && !flag2 && !string.IsNullOrEmpty(eventState.typeName))
            {
                if(!requiredExpansionDef.Asset)
                {
                    return Run.instance.IsExpansionEnabled(requiredExpansionDef.Asset);
                }
                return true;
            }
            return false;
        }
    }
}
