﻿using BepInEx;
using HG;
using RoR2;
using RoR2.Achievements;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// An AchievableUnlockableDef is an extension of <see cref="UnlockableDef"/> which allows for easy creation of Unlockables that are unlocked via Achievements, unlike unlocks that are unlocked via other methods (IE: Obtaining a LogBook for a monster)
    /// </summary>
    [CreateAssetMenu(fileName = "New AchievableUnlockableDef", menuName = "MSU/AchievableUnlockableDef")]
    public class AchievableUnlockableDef : UnlockableDef
    {
        private static HashSet<AchievableUnlockableDef> _instances = new HashSet<AchievableUnlockableDef>();
        [Header("Achievement Metadata")]

        [Tooltip("The condition required for this Achievement")]
        [SerializableSystemType.RequiredBaseType(typeof(BaseAchievement))]
        public SerializableSystemType achievementCondition;

        [Tooltip("The server achievement condition for this Achievement, can be none if your achievement does not require server tracking.")]
        [SerializableSystemType.RequiredBaseType(typeof(BaseServerAchievement))]
        public SerializableSystemType serverAchievementCondition;

        [Tooltip("The name token for this Achievement")]
        public string achievementNameToken;

        [Tooltip("The description token for this Achievement")]
        public string achievementDescriptionToken;

        [Tooltip("This amount of lunar coins are rewarded to the player for obtaining the Achievement")]
        public uint coinReward;

        /// <summary>
        /// Returns the PreRequisite Achievement required for this Achievement to be unlocked.
        /// <para>See <see cref="PreRequisiteAchievementIdentifier"/> for more info</para>
        /// </summary>
        public string preRequisiteAchievement
        {
            get
            {
                if (!_preRequisiteAchievement.achievementIdentifier.IsNullOrWhiteSpace())
                {
                    return _preRequisiteAchievement.achievementIdentifier;
                }
                else if (_preRequisiteAchievement.unlockableDef)
                {
                    return _preRequisiteAchievement.unlockableDef.cachedName + ".Achievement";
                }
                return null;
            }
        }
        [SerializeField]
        [Tooltip("The pre-requisite achievement that needs to be unlocked for this Achievement to be unlocked.\nGenerally used on Survivors where their Skill/Skin related unlocks have pre-requisites to their character unlocks.")]
        private PreRequisiteAchievementIdentifier _preRequisiteAchievement;

        /// <summary>
        /// Returns the generated AchievementDef that's tied to this Unlockable
        /// </summary>
        public AchievementDef tiedAchievementDef
        {
            get
            {
                if (_tiedAchievementDef == null)
                {
                    var identifier = cachedName + ".Achievement";
                    _tiedAchievementDef = new AchievementDef
                    {
                        identifier = identifier,
                        unlockableRewardIdentifier = cachedName,
                        prerequisiteAchievementIdentifier = preRequisiteAchievement,
                        nameToken = achievementNameToken,
                        descriptionToken = achievementDescriptionToken,
                        type = (Type)achievementCondition,
                        achievedIcon = achievementIcon,
                        unachievedIcon = null,
                        lunarCoinReward = coinReward,
                    };
                    var serverAchievementType = (Type)serverAchievementCondition;
                    if (serverAchievementType != null)
                    {
                        _tiedAchievementDef.serverTrackerType = serverAchievementType;
                    }
                }

                return _tiedAchievementDef;
            }
        }
        private AchievementDef _tiedAchievementDef;

        private new void Awake()
        {
            base.Awake();
            _instances.Add(this);
        }

        private void OnDestroy()
        {
            _instances.Remove(this);
        }

        [SystemInitializer]
        private static IEnumerator SystemInit()
        {
            yield return null;

            RoR2BepInExPack.VanillaFixes.SaferAchievementManager.OnCollectAchievementDefs += AddInstances;
        }

        private static void AddInstances(List<string> arg1, Dictionary<string, AchievementDef> arg2, List<AchievementDef> arg3)
        {
            MSULog.Info("Adding Achievements from AchievableUnlockableDefs...");
            foreach (AchievableUnlockableDef def in _instances)
            {
                if (def.index == UnlockableIndex.None)
                    continue;

                var tiedAchievement = def.tiedAchievementDef;
                if (!tiedAchievement.achievedIcon)
                {
#if DEBUG
                    MSULog.Warning($"Not adding {def} as during the AchievementDef creation it returned a null achievedIcon Sprite. (achievementIcon={def.achievementIcon})");
#endif
                    continue;
                }

                if (tiedAchievement.type == null)
                {
#if DEBUG
                    MSULog.Warning($"Not adding {def} as during the AchievementDef creation it returned a null achievementCondition Type. (achievementCondition={def.achievementCondition})");
#endif
                    continue;
                }
                def.getHowToUnlockString = () =>
                {
                    return Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", Language.GetString(tiedAchievement.nameToken), Language.GetString(tiedAchievement.descriptionToken));
                };
                def.getUnlockedString = () =>
                {
                    return Language.GetStringFormatted("UNLOCKED_FORMAT", Language.GetString(tiedAchievement.nameToken), Language.GetString(tiedAchievement.descriptionToken));
                };

                arg1.Add(tiedAchievement.identifier);
                arg2.Add(tiedAchievement.identifier, tiedAchievement);
                arg3.Add(tiedAchievement);
            }
        }

        /// <summary>
        /// A Struct to represent a pre-requisite achievement
        /// </summary>
        [Serializable]
        public struct PreRequisiteAchievementIdentifier
        {
            [Tooltip("The AchievableUnlockableDef that's required to be achieved before this one can be achieved.")]
            public AchievableUnlockableDef unlockableDef;

            [Tooltip("The identifier for another achievement, this is usually used for VANILLA achievements. Say for example, you want to make this achievement require unlocking bandit first, you would set the identifier to \"CompleteThreeStages\", which is bandit's achievement unlock identifier.")]
            public string achievementIdentifier;
        }

    }
}