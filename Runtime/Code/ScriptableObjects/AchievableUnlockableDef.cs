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
using RoR2BepInExPack;

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

        /// <summary>
        /// Returns the PreRequisite Achievement required for this Achievement to be unlocked.
        /// <para>See <see cref="PreRequisiteAchievementIdentifier"/> for more info</para>
        /// </summary>
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
        [Tooltip("The pre-requisite achievement that needs to be unlocked for this Achievement to be unlocked.\nGenerally used on Survivors where their Skill/Skin related unlocks have pre-requisites to their character unlocks.")]
        private PreRequisiteAchievementIdentifier _preRequisiteAchievement;

        /// <summary>
        /// Returns the generated AchievementDef that's tied to this Unlockable
        /// </summary>
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
        private static void SystemInit()
        {
            RoR2BepInExPack.VanillaFixes.SaferAchievementManager.OnCollectAchievementDefs += AddInstances;
        }

        private static void AddInstances(List<string> arg1, Dictionary<string, AchievementDef> arg2, List<AchievementDef> arg3)
        {
            foreach (AchievableUnlockableDef def in _instances)
            {
                if (def.index == UnlockableIndex.None)
                    continue;

                var tiedAchievement = def.TiedAchievementDef;
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

            [Tooltip("The identifier for another achievement, usually these can be obtained by concatenating the Unlockable's name and \".Achievement\" (ie: using \"Characters.Bandit2.Achievement\" will make unlocking Bandit2 a pre-requisite for this unlockable to be unlocked.)")]
            public string achievementIdentifier;
        }

    }
}