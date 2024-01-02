using HG;
using RoR2;
using RoR2.Achievements;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;

namespace MSU
{
    public class AchievableUnlockableDef : UnlockableDef
    {
        [SerializableSystemType.RequiredBaseType(typeof(BaseAchievement))]
        public SerializableSystemType achievementCondition;
        public SerializableSystemType serverAchievementCondition;
        public string achievementNameToken;
        public string achievementDescriptionToken;
        public string PreRequisiteAchievement
        {
            get
            {
                if(!_preRequisiteAchievement.achievementIdentifier.IsNullOrWhiteSpace())
                {
                    return _preRequisiteAchievement.achievementIdentifier;
                }
                else if(_preRequisiteAchievement.unlockableDef)
                {
                    return _preRequisiteAchievement.unlockableDef.cachedName + ".Achievement";
                }
                return null;
            }
        }
        [SerializeField]
        private PreRequisiteAchievementIdentifier _preRequisiteAchievement;

        public AchievementDef TiedAchievementDef
        {
            get
            {
                if(_tiedAchievementDef == null)
                {
                    var identifier = cachedName + ".Achievement";
                    _tiedAchievementDef = new AchievementDef
                    {
                        identifier = identifier,
                        unlockableRewardIdentifier = cachedName,
                        prerequisiteAchievementIdentifier = PreRequisiteAchievement,
                        nameToken = achievementNameToken,
                        descriptionToken = achievementDescriptionToken,
                        type = (Type)achievementCondition,
                        achievedIcon = achievementIcon,
                        unachievedIcon = null,
                    };
                    var serverAchievementType = (Type)serverAchievementCondition;
                    if(serverAchievementType != null)
                    {
                        _tiedAchievementDef.serverTrackerType = serverAchievementType;
                    }
                }

                return _tiedAchievementDef;
            }
        }
        private AchievementDef _tiedAchievementDef;

        [Serializable]
        public struct PreRequisiteAchievementIdentifier
        {
            public AchievableUnlockableDef unlockableDef;
            public string achievementIdentifier;
        }
    }
}