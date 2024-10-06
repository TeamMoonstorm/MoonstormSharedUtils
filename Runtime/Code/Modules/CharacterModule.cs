using BepInEx;
using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// The CharacterModule is a Module that handles classes that implement <see cref="ICharacterContentPiece"/>, <see cref="ISurvivorContentPiece"/> and <see cref="IMonsterContentPiece"/>.
    /// <para>The CharacterModule's main job is to handle the proper addition of SurvivorDefs, CharacterBodies and CharacterMasters. Alongside addition of monsters to stages using <see cref="MonsterCardProvider"/> and <see cref="DirectorAPI"/></para>
    /// </summary>
    public static class CharacterModule
    {
        /// <summary>
        /// A ReadOnlyDictionary that can be used for finding a CharacterBody's ICharacterContentPiece.
        /// <br>The ReadOnlyDictionary has a special key evaluator which uses <see cref="CharacterBody.bodyIndex"/>, which allows prefab instances of <see cref="CharacterBody"/> to be used in this dictionary.</br>
        /// <br>Subscribe to <see cref="moduleAvailability"/> to ensure the dictionary is not empty.</br>
        /// </summary>
        public static ReadOnlyDictionary<CharacterBody, ICharacterContentPiece> moonstormCharacters { get; private set; }
        private static Dictionary<CharacterBody, ICharacterContentPiece> _moonstormCharacters = new Dictionary<CharacterBody, ICharacterContentPiece>(new CharacterBodyIndexEqualityComparer());

        /// <summary>
        /// Represents the Availability of this Module.
        /// </summary>
        public static ResourceAvailability moduleAvailability;

        private static Dictionary<BaseUnityPlugin, ICharacterContentPiece[]> _pluginToCharacters = new Dictionary<BaseUnityPlugin, ICharacterContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<GameObject>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<GameObject>>();
        private static HashSet<MonsterCardProvider> _monsterCardProviders = new HashSet<MonsterCardProvider>();
        private static HashSet<DirectorCardHolderExtended> _dissonanceCards = new HashSet<DirectorCardHolderExtended>();

        /// <summary>
        /// Adds a new provider to the CharacterModule.
        /// <br>For more info, see <see cref="IContentPieceProvider"/></br>
        /// </summary>
        /// <param name="plugin">The plugin that's adding the new provider</param>
        /// <param name="provider">The provider from the plugin, can be one created using <see cref="ContentUtil.CreateGameObjectGenericContentPieceProvider{T}(BaseUnityPlugin, RoR2.ContentManagement.ContentPack)"/></param>
        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<GameObject> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        /// <summary>
        /// Obtains all the CharacterContentPieces that where added by the specified plugin
        /// </summary>
        /// <param name="plugin">The plugin to obtain it's Characters</param>
        /// <returns>An array of ICharacterContentPieces, if the plugin has not added any characters, it returns an empty Array.</returns>
        public static ICharacterContentPiece[] GetCharacters(BaseUnityPlugin plugin)
        {
            if (_pluginToCharacters.TryGetValue(plugin, out var characters))
            {
                return characters;
            }
#if DEBUG
            MSULog.Info($"{plugin} has no registered characters");
#endif
            return Array.Empty<ICharacterContentPiece>();
        }

        /// <summary>
        /// A Coroutine used to initialize the Characters added by <paramref name="plugin"/>
        /// <br>The coroutine yield breaks if the plugin has not added it's specified provider using <see cref="AddProvider(BaseUnityPlugin, IContentPieceProvider{GameObject})"/></br>
        /// </summary>
        /// <param name="plugin">The plugin to initialize it's Characters</param>
        /// <returns>A Coroutine enumerator that can be Awaited or Yielded</returns>
        public static IEnumerator InitializeCharacters(BaseUnityPlugin plugin)
        {
#if DEBUG
            if (!_pluginToContentProvider.ContainsKey(plugin))
            {
                MSULog.Info($"{plugin} has no IContentPieceProvider registered in the CharacterModule.");
            }
#endif

            if (_pluginToContentProvider.TryGetValue(plugin, out IContentPieceProvider<GameObject> provider))
            {
                var enumerator = InitializeCharactersFromProvider(plugin, provider);
                while (enumerator.MoveNext())
                {
                    yield return null;
                }
            }
            yield break;
        }

        [SystemInitializer(typeof(BodyCatalog))]
        private static IEnumerator SystemInit()
        {
            MSULog.Info("Initializing the Character Module...");

            yield return null;


            moonstormCharacters = new ReadOnlyDictionary<CharacterBody, ICharacterContentPiece>(_moonstormCharacters);
            _moonstormCharacters = null;

            moduleAvailability.MakeAvailable();

            if(moonstormCharacters.Count == 0)
            {
#if DEBUG
                MSULog.Info("Not doing CharacterModule hooks since there are no characters registered.");
#endif
                yield break;
            }
            DirectorAPI.MonsterActions += AddCustomMonsters;
        }

        private static IEnumerator InitializeCharactersFromProvider(BaseUnityPlugin plugin, IContentPieceProvider<GameObject> provider)
        {
            IGameObjectContentPiece<CharacterBody>[] content = provider.GetContents().OfType<IGameObjectContentPiece<CharacterBody>>().ToArray();

            List<IGameObjectContentPiece<CharacterBody>> characters = new List<IGameObjectContentPiece<CharacterBody>>();

            var helper = new ParallelCoroutine();
            foreach (var character in content)
            {
                if (!character.IsAvailable(provider.contentPack))
                    continue;

                characters.Add(character);
                helper.Add(character.LoadContentAsync());
            }

            while (!helper.IsDone())
                yield return null;

            InitializeCharacters(plugin, characters, provider);
        }

        private static void InitializeCharacters(BaseUnityPlugin plugin, List<IGameObjectContentPiece<CharacterBody>> bodies, IContentPieceProvider<GameObject> provider)
        {
            foreach (var body in bodies)
            {
#if DEBUG
                try
                {
#endif
                    body.Initialize();

                    var asset = body.asset;
                    provider.contentPack.bodyPrefabs.AddSingle(asset);

                    if (body is IContentPackModifier packModifier)
                    {
                        packModifier.ModifyContentPack(provider.contentPack);
                    }

                    if (body is ICharacterContentPiece characterContentPiece)
                    {
                        if (!_pluginToCharacters.ContainsKey(plugin))
                        {
                            _pluginToCharacters.Add(plugin, Array.Empty<ICharacterContentPiece>());
                        }
                        var array = _pluginToCharacters[plugin];
                        HG.ArrayUtils.ArrayAppend(ref array, characterContentPiece);
                        _pluginToCharacters[plugin] = array;

                        if (characterContentPiece.masterPrefab)
                        {
                            provider.contentPack.masterPrefabs.AddSingle(characterContentPiece.masterPrefab);
                        }
                        _moonstormCharacters.Add(characterContentPiece.component, characterContentPiece);
                    }

                    if (body is ISurvivorContentPiece survivorContentPiece)
                    {
                        provider.contentPack.survivorDefs.AddSingle(survivorContentPiece.survivorDef);
                    }
                    if (body is IMonsterContentPiece monsterContentPiece)
                    {
                        if (monsterContentPiece.cardProvider)
                            _monsterCardProviders.Add(monsterContentPiece.cardProvider);

                        if (monsterContentPiece.dissonanceCard)
                            _dissonanceCards.Add(monsterContentPiece.dissonanceCard);
                    }

#if DEBUG
                    MSULog.Info($"Body {body.GetType().FullName} initialized.");
#endif

#if DEBUG
                }
                catch (Exception ex)
                {
                    MSULog.Error($"Character {body.GetType().FullName} threw an exception while initializing.\n{ex}");
                }
#endif
            }
        }

        private static void AddCustomMonsters(DccsPool pool, List<DirectorAPI.DirectorCardHolder> cardList, DirectorAPI.StageInfo stageInfo)
        {
            if (RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.mixEnemyArtifactDef))
            {
                foreach (var dissonanceCard in _dissonanceCards)
                {
                    if (!dissonanceCard.IsAvailable())
                        continue;

                    cardList.Add(dissonanceCard);
                }
                return;
            }

            foreach (var monsterCardProvider in _monsterCardProviders)
            {
                AddCustomMonster(monsterCardProvider, pool, stageInfo);
            }
        }

        private static void AddCustomMonster(MonsterCardProvider monsterCardProvider, DccsPool pool, DirectorAPI.StageInfo stageInfo)
        {
            var standardCategory = pool.poolCategories.FirstOrDefault(category => category.name == DirectorAPI.Helpers.MonsterPoolCategories.Standard);

            if (standardCategory == null)
            {
                MSULog.Warning($"Couldn't find standard category for current stage! not adding {monsterCardProvider}.");
                return;
            }

            var dccsCollection = standardCategory.alwaysIncluded.Select(pe => pe.dccs)
                .Concat(standardCategory.includedIfConditionsMet.Select(pe => pe.dccs))
                .Concat(standardCategory.includedIfNoConditionsMet.Select(pe => pe.dccs))
                .ToArray();

            DirectorCardHolderExtended cardHolder = null;
            if (stageInfo.stage == DirectorAPI.Stage.Custom)
            {
                monsterCardProvider.customStageToCards.TryGetValue(stageInfo.CustomStageName, out cardHolder);
            }
            else
            {
                monsterCardProvider.stageToCards.TryGetValue(stageInfo.stage, out cardHolder);
            }

            if (cardHolder == null)
                return;

            if (!cardHolder.IsAvailable())
                return;

            foreach (DirectorCardCategorySelection categorySelection in dccsCollection)
            {
                categorySelection.AddCard(cardHolder);
            }
        }
    }
}
