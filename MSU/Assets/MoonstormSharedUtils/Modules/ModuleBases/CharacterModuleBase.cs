using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A Module Base for managing Characterbodies
    /// <para>Automatically adds Monsters to the CombatDirector according to the settings on the Monster's MSMonsterDirectorCard</para>
    /// </summary>
    public abstract class CharacterModuleBase : ModuleBase
    {
        /// <summary>
        /// Dictionary of all the CharacterBodies loaded by Moonstorm Shared Utils
        /// </summary>
        public static Dictionary<GameObject, CharacterBase> MoonstormCharacters = new Dictionary<GameObject, CharacterBase>();

        public static List<MSMonsterFamily> MoonstormFamilies = new List<MSMonsterFamily>();

        /// <summary>
        /// Returns all the Monsters loaded by Moonstorm Shared Utils
        /// </summary>
        public static MonsterBase[] MoonstormMonsters { get => MoonstormCharacters.Values.Where(cb => cb.GetType().IsSubclassOf(typeof(MonsterBase))).Cast<MonsterBase>().ToArray(); }
        /// <summary>
        /// Returns all the Survivors loaded by Moonstorm Shared Utils
        /// </summary>
        public static SurvivorBase[] MoonstormSurvivors { get => MoonstormCharacters.Values.Where(cb => cb.GetType().IsSubclassOf(typeof(SurvivorBase))).Cast<SurvivorBase>().ToArray(); }
        /// <summary>
        /// Returns all the CharacterBody prefabs loaded by Moonstorm Shared Utils
        /// </summary>
        public static GameObject[] LoadedCharacterBodies { get => MoonstormCharacters.Values.Select(cb => cb.BodyPrefab).ToArray(); }
        /// <summary>
        /// Returns all the CharacterMaster prefabs loaded by MoonstormSharedUtils
        /// </summary>
        public static GameObject[] LoadedCharacterMasters { get => MoonstormCharacters.Values.Select(cb => cb.MasterPrefab).ToArray(); }
        
        [SystemInitializer(new Type[] { typeof(BodyCatalog), typeof(MasterCatalog) })]
        private static void HookInit()
        {
            MSULog.LogI("Subscribing to delegates related to survivors & monsters.");
            //DirectorAPI.FamilyActions += ModifyFamilies;
            DirectorAPI.MonsterActions += ModifyMonsters;
        }

        #region Find Methods
        public static T FindCharacterBase<T>(string bodyName) where T : CharacterBase
        {
            MoonstormCharacters.TryGetValue(BodyCatalog.FindBodyPrefab(bodyName), out CharacterBase charBase);
            return charBase as T;
        }
        public static T FindCharacterBase<T>(BodyIndex bodyIndex) where T : CharacterBase
        {
            MoonstormCharacters.TryGetValue(BodyCatalog.GetBodyPrefab(bodyIndex), out CharacterBase charBase);
            return charBase as T;
        }
        public static T FindCharacterBase<T>(GameObject masterPrefab) where T : CharacterBase
        {
            MoonstormCharacters.TryGetValue(masterPrefab.GetComponent<CharacterMaster>().bodyPrefab, out CharacterBase charBase);
            return charBase as T;
        }
        public static T FindCharacterBase<T>(MasterCatalog.MasterIndex masterIndex) where T : CharacterBase
        {
            MoonstormCharacters.TryGetValue(MasterCatalog.GetMasterPrefab(masterIndex).GetComponent<CharacterMaster>().bodyPrefab, out CharacterBase charBase);
            return charBase as T;
        }
        public static T FindCharacterBase<T>(CharacterMaster charMaster) where T : CharacterBase
        {
            MoonstormCharacters.TryGetValue(charMaster.bodyPrefab, out CharacterBase charBase);
            return charBase as T;
        }
        public static MonsterBase FindMonsterBase(MSMonsterDirectorCard monsterDirectorCard)
        {
            return MoonstormCharacters.Values.Where(cb => cb.GetType().IsSubclassOf(typeof(MonsterBase))).Cast<MonsterBase>().Where(mb => mb.MonsterDirectorCard = monsterDirectorCard).FirstOrDefault();
        }
        #endregion

        #region Characters
        /// <summary>
        /// Finds all the CharacterBase inheriting classes in your assembly and creates instances for each found.
        /// <para>Ignores all classes with the "DisabledContent" attribute</para>
        /// </summary>
        /// <returns>An IEnumerable of all your Assembly's CharacterBases</returns>
        public virtual IEnumerable<CharacterBase> InitializeCharacters()
        {
            MSULog.LogD($"Getting the Characters found inside {GetType().Assembly}...");
            return GetContentClasses<CharacterBase>();
        }

        /// <summary>
        /// Initializes a Character
        /// </summary>
        /// <param name="character">The CharacterBase class</param>
        /// <param name="characterList">Optinal, a List for storing the CharacterBases.</param>
        public void AddCharacter(CharacterBase character, Dictionary<GameObject, CharacterBase> characterDictionary = null)
        {
            character.Initialize();

            MoonstormCharacters.Add(character.BodyPrefab, character);
            if (characterDictionary != null)
                characterDictionary.Add(character.BodyPrefab, character);

            /*TryAddingBodyPrefabToContentPack(character.BodyPrefab);
            TryAddingMasterPrefabToContentPack(character.MasterPrefab);*/

            MSULog.LogD($"Character {character} added");
        }

        internal void TryAddingBodyPrefabToContentPack(GameObject bodyPrefab)
        {
            if (ContentPack.bodyPrefabs.Contains(bodyPrefab))
                MSULog.LogD($"Content pack already has {bodyPrefab} in it's  \"BodyPrefabs\" array.");
            else
                HG.ArrayUtils.ArrayAppend(ref ContentPack.bodyPrefabs, bodyPrefab);
        }
        internal void TryAddingMasterPrefabToContentPack(GameObject masterPrefab)
        {
            if (ContentPack.masterPrefabs.Contains(masterPrefab))
                MSULog.LogD($"Content pack already has {masterPrefab} in it's \"MasterPrefabs\" array.");
            else
                HG.ArrayUtils.ArrayAppend(ref ContentPack.masterPrefabs, masterPrefab);
        }
        #endregion

        #region Monster Families
        public void AddMonsterFamily(MSMonsterFamily[] monsterFamilies)
        {
            MoonstormFamilies.AddRange(monsterFamilies);
        }
        public void AddMonsterFamily(MSMonsterFamily monsterFamily)
        {
            MoonstormFamilies.Add(monsterFamily);
        }
        #endregion

        #region Hooks
        private static void ModifyMonsters(List<DirectorAPI.DirectorCardHolder> cardList, DirectorAPI.StageInfo stageInfo)
        {
            int num = 0;
            foreach(var monster in MoonstormMonsters)
            {
                var card = monster.MonsterDirectorCard;
                if(card.stages.HasFlag(stageInfo.stage))
                {
                    if(stageInfo.stage == DirectorAPI.Stage.Custom)
                    {
                        if (card.customStages.Contains(stageInfo.CustomStageName.ToLowerInvariant()))
                        {
                            num++;
                            cardList.Add(card.DirectorCardHolder);
                            MSULog.LogD($"Added {card} Monster");
                            continue;
                        }
                    }
                    else if(stageInfo.CheckStage(card.stages))
                    {
                        num++;
                        cardList.Add(card.DirectorCardHolder);
                        MSULog.LogD($"Added {card} Monster");
                    }
                }
            }
            if(num > 0)
            {
                MSULog.LogD($"Added a total of {num} Monsters");
            }
        }

        private static void ModifyFamilies(List<DirectorAPI.MonsterFamilyHolder> familyList, DirectorAPI.StageInfo stageInfo)
        {
            int num = 0;
            foreach(var family in MoonstormFamilies)
            {
                if (family.stages.HasFlag(stageInfo.stage))
                {
                    if (stageInfo.stage == DirectorAPI.Stage.Custom)
                    {
                        if (family.customStages.Contains(stageInfo.CustomStageName.ToLowerInvariant()))
                        {
                            num++;
                            familyList.Add(family.MonsterFamilyHolder);
                            MSULog.LogD($"Added {family} to the Families list");
                            continue;
                        }
                    }
                    else if (stageInfo.CheckStage(family.stages))
                    {
                        num++;
                        familyList.Add(family.MonsterFamilyHolder);
                        MSULog.LogD($"Added {family} to the Families list");
                    }
                }
            }
            if (num > 0)
            {
                MSULog.LogD($"Added a total of {num} Monsters");
            }
        }
        #endregion
    }
}
