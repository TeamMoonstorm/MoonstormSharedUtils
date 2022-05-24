using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    public abstract class CharacterModuleBase : ContentModule<CharacterBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<GameObject, CharacterBase> MoonstormCharacters { get; private set; }
        internal static Dictionary<GameObject, CharacterBase> characters = new Dictionary<GameObject, CharacterBase>();

        public static MonsterBase[] MoonstormMonsters { get => MoonstormCharacters.Values.Where(cb => cb.GetType().IsSubclassOf(typeof(MonsterBase))).Cast<MonsterBase>().ToArray(); }
        public static SurvivorBase[] MoonstormSurvivors { get => MoonstormCharacters.Values.Where(cb => cb.GetType().IsSubclassOf(typeof(SurvivorBase))).Cast<SurvivorBase>().ToArray(); }
        public static GameObject[] LoadedCharacterBodies { get => MoonstormCharacters.Values.Select(cb => cb.BodyPrefab).ToArray(); }
        public static GameObject[] LoadedCharacterMasters { get => MoonstormCharacters.Values.Select(cb => cb.MasterPrefab).ToArray(); }
        public static Action<ReadOnlyDictionary<GameObject, CharacterBase>> OnDictionariesCreated;
        #endregion

        [SystemInitializer(new Type[] { typeof(BodyCatalog), typeof(MasterCatalog) })]
        private static void SystemInit()
        {
            MSULog.Info("Initializing Character Module...");
            DirectorAPI.MonsterActions += ModifyMonsters;

            MoonstormCharacters = new ReadOnlyDictionary<GameObject, CharacterBase>(characters);
            characters = null;

            OnDictionariesCreated?.Invoke(MoonstormCharacters);
        }

        #region Characters
        protected virtual IEnumerable<CharacterBase> GetCharacterBases()
        {
            MSULog.Debug($"Getting the Characters found inside {GetType().Assembly}...");
            return GetContentClasses<CharacterBase>();
        }

        protected void AddCharacter(CharacterBase character, Dictionary<GameObject, CharacterBase> characterDictionary = null)
        {
            InitializeContent(character);
            characterDictionary?.Add(character.BodyPrefab, character);
        }

        protected override void InitializeContent(CharacterBase contentClass)
        {
            AddSafely(ref SerializableContentPack.bodyPrefabs, contentClass.BodyPrefab, "BodyPrefabs");

            contentClass.Initialize();

            switch(contentClass)
            {
                case MonsterBase monster:
                    AddSafely(ref SerializableContentPack.masterPrefabs, monster.MasterPrefab, "MasterPrefabs");
                    MSULog.Debug($"Character {monster} Initialized and ensured it's body and master prefabs in {SerializableContentPack.name}");
                    break;
                case SurvivorBase survivor:
                    AddSafely(ref SerializableContentPack.survivorDefs, survivor.SurvivorDef);
                    AddSafely(ref SerializableContentPack.masterPrefabs, survivor.MasterPrefab, "MasterPrefabs");
                    MSULog.Debug($"Character {survivor} Initialized and ensured it's body, master prefabs & survivor def in {SerializableContentPack.name}");
                    break;
            }
        }
        #endregion

        #region Hooks
        private static void ModifyMonsters(DccsPool pool, List<DirectorAPI.DirectorCardHolder> cardList, DirectorAPI.StageInfo stageInfo)
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
                            MSULog.Debug($"Added {card} Monster");
                            continue;
                        }
                    }
                    else if(stageInfo.CheckStage(card.stages))
                    {
                        num++;
                        cardList.Add(card.DirectorCardHolder);
                        MSULog.Debug($"Added {card} Monster");
                    }
                }
            }
            if(num > 0)
            {
                MSULog.Debug($"Added a total of {num} Monsters");
            }
        }
        #endregion
    }
}
