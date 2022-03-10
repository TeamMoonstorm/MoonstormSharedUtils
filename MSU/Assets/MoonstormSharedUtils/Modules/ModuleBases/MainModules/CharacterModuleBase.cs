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
        public static ReadOnlyDictionary<GameObject, CharacterBase> MoonstormCharacters
        {
            get
            {
                if (!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve dictionary {nameof(MoonstormCharacters)}", typeof(CharacterModuleBase));
                    return null;
                }
                return MoonstormCharacters;
            }
            private set
            {
                MoonstormCharacters = value;
            }
        }
        private static Dictionary<GameObject, CharacterBase> characters = new Dictionary<GameObject, CharacterBase>();
        public static Action<ReadOnlyDictionary<GameObject, CharacterBase>> OnDictionariesCreated;

        public static MonsterBase[] MoonstormMonsters { get => MoonstormCharacters.Values.Where(cb => cb.GetType().IsSubclassOf(typeof(MonsterBase))).Cast<MonsterBase>().ToArray(); }
        public static SurvivorBase[] MoonstormSurvivors { get => MoonstormCharacters.Values.Where(cb => cb.GetType().IsSubclassOf(typeof(SurvivorBase))).Cast<SurvivorBase>().ToArray(); }
        public static GameObject[] LoadedCharacterBodies { get => MoonstormCharacters.Values.Select(cb => cb.BodyPrefab).ToArray(); }
        public static GameObject[] LoadedCharacterMasters { get => MoonstormCharacters.Values.Select(cb => cb.MasterPrefab).ToArray(); }

        public static bool Initialized { get; private set; } = false;
        #endregion

        [SystemInitializer(new Type[] { typeof(BodyCatalog), typeof(MasterCatalog) })]
        private static void SystemInit()
        {
            Initialized = true;
            MSULog.Info("Subscribing to delegates related to survivors & monsters.");
            DirectorAPI.MonsterActions += ModifyMonsters;

            MoonstormCharacters = new ReadOnlyDictionary<GameObject, CharacterBase>(characters);
            characters.Clear();
            characters = null;

            OnDictionariesCreated?.Invoke(MoonstormCharacters);
        }

        #region Characters
        protected virtual IEnumerable<CharacterBase> GetCharacterBases()
        {
            if (Initialized)
            {
                ThrowModuleInitialized($"Retrieve CharacterBase list", typeof(CharacterModuleBase));
                return null;
            }

            MSULog.Debug($"Getting the Characters found inside {GetType().Assembly}...");
            return GetContentClasses<CharacterBase>();
        }

        protected void AddCharacter(CharacterBase character, Dictionary<GameObject, CharacterBase> characterDictionary = null)
        {
            if (Initialized)
            {
                ThrowModuleInitialized($"Add CharacterBase to ContentPack", typeof(CharacterModuleBase));
                return;
            }

            if (InitializeContent(character) && characterDictionary != null)
                AddSafelyToDict(ref characterDictionary, character.BodyPrefab, character);

            MSULog.Debug($"Character {character} added");
        }

        protected override bool InitializeContent(CharacterBase contentClass)
        {
            if(AddSafely(ref SerializableContentPack.bodyPrefabs, contentClass.BodyPrefab))
            {
                contentClass.Initialize();

                switch(contentClass)
                {
                    case MonsterBase monster:
                        AddSafely(ref SerializableContentPack.masterPrefabs, monster.MasterPrefab);
                        break;
                    case SurvivorBase survivor:
                        AddSafely(ref SerializableContentPack.survivorDefs, survivor.SurvivorDef);
                        AddSafely(ref SerializableContentPack.masterPrefabs, survivor.MasterPrefab);
                        break;
                }
                return true;
            }
            return false;
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
