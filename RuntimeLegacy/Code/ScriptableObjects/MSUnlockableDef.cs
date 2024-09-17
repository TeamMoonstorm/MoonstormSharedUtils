using HG;
using RoR2;
using RoR2.Achievements;
using System;

namespace Moonstorm
{
    public class MSUnlockableDef : UnlockableDef
    {
        [Serializable]
        public struct AchievementStringAssetRef
        {
            public string AchievementIdentifier;

            public MSUnlockableDef UnlockableDef;
        }

        [SerializableSystemType.RequiredBaseType(typeof(BaseAchievement))]
        public SerializableSystemType achievementCondition;

        public bool serverTracked;

        [SerializableSystemType.RequiredBaseType(typeof(BaseServerAchievement))]
        public SerializableSystemType baseServerAchievement;

        public string achievementNameToken;

        public string achievementDescToken;

        public AchievementStringAssetRef prerequisiteAchievement;

        public AchievementDef AchievementDef { get; private set; }

        internal AchievementDef GetOrCreateAchievementDef()
        {
            var achievementIdentifier = cachedName + ".Achievement";

            if (AchievementDef != null)
                return AchievementDef;
            else
            {
                AchievementDef achievementDef = new AchievementDef
                {
                    identifier = achievementIdentifier,
                    unlockableRewardIdentifier = cachedName,
                    prerequisiteAchievementIdentifier = GetPrerequisiteAchievementIdentifier(),
                    nameToken = achievementNameToken,
                    descriptionToken = achievementDescToken,
                    type = (Type)achievementCondition,
                    achievedIcon = achievementIcon,
                    unachievedIcon = null,
                };
                if (serverTracked)
                {
                    var serverAchievementType = (Type)baseServerAchievement;
                    if (serverAchievementType != null)
                    {
                        achievementDef.serverTrackerType = serverAchievementType;
                    }
                }

                AchievementDef = achievementDef;
                return AchievementDef;
            }
        }

        private string GetPrerequisiteAchievementIdentifier()
        {
            AchievementStringAssetRef stringAssetRef = prerequisiteAchievement;
            if (!string.IsNullOrEmpty(stringAssetRef.AchievementIdentifier))
            {
                return stringAssetRef.AchievementIdentifier;
            }
            else if (stringAssetRef.UnlockableDef != null)
            {
                return stringAssetRef.UnlockableDef.cachedName + ".Achievement";
            }
            return null;
        }
    }
}
