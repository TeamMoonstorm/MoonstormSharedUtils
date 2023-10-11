using HG;
using RoR2;
using RoR2.Achievements;
using System;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="MSUnlockableDef"/> is an extension of <see cref="UnlockableDef"/> that allows for easy creation of an Unlockable that gets unlocked via an Achievement
    /// </summary>
    [CreateAssetMenu(fileName = "New ExtendedUnlockableDef", menuName = "Moonstorm/MSUnlockableDef")]
    public class MSUnlockableDef : UnlockableDef
    {
        /// <summary>
        /// Represents an addressable version of a prerequisite achievement
        /// </summary>
        [Serializable]
        public struct AchievementStringAssetRef
        {
            [Tooltip("The prerequisite AchievementDef's identifier")]
            public string AchievementIdentifier;
            [Tooltip("The prerequisite MSUnlockableDef")]
            public MSUnlockableDef UnlockableDef;
        }

        [Tooltip("The BaseAchievement class that manages how this achievement is obtained")]
        [SerializableSystemType.RequiredBaseType(typeof(BaseAchievement))]
        public SerializableSystemType achievementCondition;

        [Tooltip("Wether this achievement is server tracked")]
        public bool serverTracked;

        [Tooltip("The BaseServerAchievement class that manages the networking of this achievment")]
        [SerializableSystemType.RequiredBaseType(typeof(BaseServerAchievement))]
        public SerializableSystemType baseServerAchievement;

        [Tooltip("The name of this achievement")]
        public string achievementNameToken;

        [Tooltip("The description of this achievement")]
        public string achievementDescToken;

        [Tooltip("The prerequisite achievement for this achievement to be unlocked")]
        public AchievementStringAssetRef prerequisiteAchievement;

        /// <summary>
        /// This is the <see cref="AchievementDef"/> that's tied to this UnlockableDef
        /// </summary>
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
