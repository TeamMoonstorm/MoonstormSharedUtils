using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// The <see cref="CharacterModuleBase"/> is a <see cref="ContentModule{T}"/> that handles the <see cref="CharacterBase"/> class
    /// <para>The <see cref="CharacterModuleBase"/>'s main job is to handle the proper addition of CharacterBodies and CharacterMasters from <see cref="CharacterBase"/> inheriting classes</para>
    /// <para>For more specific uses there's the <see cref="MonsterBase"/> class, used for adding a Monster to the game and have it spawned via the Combat Director</para>
    /// <para>There's also the <see cref="SurvivorBase"/> class, used for adding a survivor to the game via a SurvivorDef</para>
    /// <para>Inherit from this module if you want to load and manage Characters with <see cref="CharacterBase"/> systems</para>
    /// </summary>
    public abstract class CharacterModuleBase : ContentModule<CharacterBase>
    {
        #region Properties and Fields
        /// <summary>
        /// A ReadOnlyDictionary that can be used for loading a specific CharacterBase by giving it's tied CharacterBody prefab
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionaryCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<GameObject, CharacterBase> MoonstormCharacters { get; private set; }
        internal static Dictionary<GameObject, CharacterBase> characters = new Dictionary<GameObject, CharacterBase>();

        /// <summary>
        /// Returns all the MonsterBases from the <see cref="MoonstormCharacters"/>
        /// </summary>
        public static MonsterBase[] MoonstormMonsters { get => MoonstormCharacters.Values.Where(cb => cb.GetType().IsSubclassOf(typeof(MonsterBase))).Cast<MonsterBase>().ToArray(); }
        /// <summary>
        /// Returns all the SurvivorBases from the <see cref="MoonstormCharacters"/>
        /// </summary>
        public static SurvivorBase[] MoonstormSurvivors { get => MoonstormCharacters.Values.Where(cb => cb.GetType().IsSubclassOf(typeof(SurvivorBase))).Cast<SurvivorBase>().ToArray(); }
        /// <summary>
        /// Returns all the CharacterBody prefabs from the <see cref="MoonstormCharacters"/>
        /// </summary>
        public static GameObject[] LoadedCharacterBodies { get => MoonstormCharacters.Values.Select(cb => cb.BodyPrefab).ToArray(); }
        /// <summary>
        /// Returns all the CharacterMaster prefabs from the <see cref="MoonstormCharacters"/>
        /// </summary>
        public static GameObject[] LoadedCharacterMasters { get => MoonstormCharacters.Values.Select(cb => cb.MasterPrefab).ToArray(); }
        /// <summary>
        /// An action that gets invoked when the <see cref="MoonstormCharacters"/> dictionary has been populated
        /// </summary>
        public static Action<ReadOnlyDictionary<GameObject, CharacterBase>> OnDictionaryCreated;
        #endregion

        [SystemInitializer(new Type[] { typeof(BodyCatalog), typeof(MasterCatalog) })]
        private static void SystemInit()
        {
            MSULog.Info("Initializing Character Module...");
            DirectorAPI.MonsterActions += ModifyMonsters;

            MoonstormCharacters = new ReadOnlyDictionary<GameObject, CharacterBase>(characters);
            characters = null;

            OnDictionaryCreated?.Invoke(MoonstormCharacters);
        }

        #region Characters
        /// <summary>
        /// <inheritdoc cref="ModuleBase{T}.GetContentClasses{T}(Type)"/>
        /// <para>T in this case is <see cref="CharacterBase"/></para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's <see cref="CharacterBase"/></returns>
        protected virtual IEnumerable<CharacterBase> GetCharacterBases()
        {
            MSULog.Debug($"Getting the Characters found inside {GetType().Assembly}...");
            return GetContentClasses<CharacterBase>();
        }

        /// <summary>
        /// Adds a CharacterBase's CharacterBody and CharacterMaster prefabs to the game and to the ContentPack
        /// <para>If the character is a <see cref="MonsterBase"/> and it inherits <see cref="MonsterBase.MonsterDirectorCard"/>, then it'll appear ingame using the CombatDirector</para>
        /// <para>If the character is a <see cref="SurvivorBase"/>, then it's SurvivorDef will be added to the ContentPack</para>
        /// </summary>
        /// <param name="character">The CharacterBase to add</param>
        /// <param name="characterDictionary">Optional, a dictionary to add your initialized CharacterBase and CharacterBody game objects</param>
        protected void AddCharacter(CharacterBase character, Dictionary<GameObject, CharacterBase> characterDictionary = null)
        {
            InitializeContent(character);
            characterDictionary?.Add(character.BodyPrefab, character);
        }

        /// <summary>
        /// <inheritdoc cref="AddCharacter(CharacterBase, Dictionary{GameObject, CharacterBase})"/>
        /// </summary>
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
