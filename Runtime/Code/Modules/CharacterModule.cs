using BepInEx;
using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static RoR2.RoR2Content;

namespace MSU
{
    public static class CharacterModule
    {
        public static ReadOnlyDictionary<CharacterBody, ICharacterContentPiece> MoonstormCharacters { get; private set; }
        private static Dictionary<CharacterBody, ICharacterContentPiece> _moonstormCharacters = new Dictionary<CharacterBody, ICharacterContentPiece>();

        public static ResourceAvailability moduleAvailability;

        private static Dictionary<BaseUnityPlugin, ICharacterContentPiece[]> _pluginToCharacters = new Dictionary<BaseUnityPlugin, ICharacterContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<GameObject>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<GameObject>>();
        private static HashSet<MonsterCardProvider> _monsterCardProviders = new HashSet<MonsterCardProvider>();
        private static HashSet<DirectorAPI.DirectorCardHolder> _dissonanceCards = new HashSet<DirectorAPI.DirectorCardHolder>();

        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<GameObject> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        public static ICharacterContentPiece[] GetCharacters(BaseUnityPlugin plugin)
        {
            if(_pluginToCharacters.TryGetValue(plugin, out var characters))
            {
                return characters;
            }
            return null;
        }

        public static IEnumerator InitializeCharacters(BaseUnityPlugin plugin)
        {
            if(_pluginToContentProvider.TryGetValue(plugin, out IContentPieceProvider<GameObject> provider))
            {
                yield return InitializeCharactersFromProvider(plugin, provider);
            }
            yield break;
        }

        [SystemInitializer(typeof(BodyCatalog))]
        private static void SystemInit()
        {
            MSULog.Info("Initializing Character Module...");
            DirectorAPI.MonsterActions += AddCustomMonsters;

            MoonstormCharacters = new ReadOnlyDictionary<CharacterBody, ICharacterContentPiece>(_moonstormCharacters);
            _moonstormCharacters = null;

            moduleAvailability.MakeAvailable();
        }

        private static IEnumerator InitializeCharactersFromProvider(BaseUnityPlugin plugin, IContentPieceProvider<GameObject> provider)
        {
            IGameObjectContentPiece<CharacterBody>[] content = provider.GetContents().OfType<IGameObjectContentPiece<CharacterBody>>().ToArray();

            List<IGameObjectContentPiece<CharacterBody>> characters = new List<IGameObjectContentPiece<CharacterBody>>();

            var helper = new ParallelCoroutineHelper();
            foreach(var character in content)
            {
                if (!character.IsAvailable(provider.ContentPack))
                    continue;

                characters.Add(character);
                helper.Add(character.LoadContentAsync);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            InitializeCharacters(plugin, characters, provider);
        }

        private static void InitializeCharacters(BaseUnityPlugin plugin, List<IGameObjectContentPiece<CharacterBody>> bodies, IContentPieceProvider<GameObject> provider)
        {
            foreach (var body in bodies)
            {
                body.Initialize();

                var asset = body.Asset;
                provider.ContentPack.bodyPrefabs.AddSingle(asset);

                if (body is IContentPackModifier packModifier)
                {
                    packModifier.ModifyContentPack(provider.ContentPack);
                }

                if (body is ICharacterContentPiece characterContentPiece)
                {
                    if (!_pluginToCharacters.ContainsKey(plugin))
                    {
                        _pluginToCharacters.Add(plugin, Array.Empty<ICharacterContentPiece>());
                    }
                    var array = _pluginToCharacters[plugin];
                    HG.ArrayUtils.ArrayAppend(ref array, characterContentPiece);

                    if (characterContentPiece.MasterPrefab)
                    {
                        provider.ContentPack.masterPrefabs.AddSingle(characterContentPiece.MasterPrefab);
                    }
                    _moonstormCharacters.Add(characterContentPiece.Component, characterContentPiece);
                }

                if (body is ISurvivorContentPiece survivorContentPiece)
                {
                    provider.ContentPack.survivorDefs.AddSingle(survivorContentPiece.SurvivorDef);
                }
                if (body is IMonsterContentPiece monsterContentPiece)
                {
                    if (monsterContentPiece.CardProvider)
                        _monsterCardProviders.Add(monsterContentPiece.CardProvider);

                    if (monsterContentPiece.DissonanceCard)
                        _dissonanceCards.Add(monsterContentPiece.DissonanceCard);
                }

                if (body is IUnlockableContent unlockableContent)
                {
                    UnlockableDef[] unlockableDefs = unlockableContent.TiedUnlockables;
                    if (unlockableDefs.Length > 0)
                    {
                        UnlockableManager.AddUnlockables(unlockableDefs);
                        provider.ContentPack.unlockableDefs.Add(unlockableDefs);
                    }
                }
            }
        }

        private static void AddCustomMonsters(DccsPool pool, List<DirectorAPI.DirectorCardHolder> cardList, DirectorAPI.StageInfo stageInfo)
        {
            if (RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.mixEnemyArtifactDef))
            {
                foreach (var dissonanceCard in _dissonanceCards)
                {
                    if (!dissonanceCard.Card.IsAvailable())
                        continue;

                    cardList.Add(dissonanceCard);
                }
                return;
            }

            foreach (var monsterCardProvider in _monsterCardProviders)
            {
                AddCustomMonster(monsterCardProvider, pool, cardList, stageInfo);
            }
        }

        private static void AddCustomMonster(MonsterCardProvider monsterCardProvider, DccsPool pool, List<DirectorAPI.DirectorCardHolder> cardList, DirectorAPI.StageInfo stageInfo)
        {
            var standardCategory = pool.poolCategories.FirstOrDefault(category => category.name == DirectorAPI.Helpers.MonsterPoolCategories.Standard);
            
            if(standardCategory == null)
            {
                MSULog.Warning($"Couldn't find standard category for current stage! not adding {monsterCardProvider}.");
                return;
            }

            var dccsCollection = standardCategory.alwaysIncluded.Select(pe => pe.dccs)
                .Concat(standardCategory.includedIfConditionsMet.Select(pe => pe.dccs))
                .Concat(standardCategory.includedIfNoConditionsMet.Select(pe => pe.dccs))
                .ToArray();

            DirectorAPI.DirectorCardHolder cardHolder = null;
            if(stageInfo.stage == DirectorAPI.Stage.Custom)
            {
                monsterCardProvider.CustomStageToCards.TryGetValue(stageInfo.CustomStageName, out cardHolder);
            }
            else
            {
                monsterCardProvider.StageToCards.TryGetValue(stageInfo.stage, out cardHolder );
            }

            if (cardHolder == null)
                return;

            if (!cardHolder.Card.IsAvailable())
                return;

            foreach(DirectorCardCategorySelection categorySelection in dccsCollection)
            {
                categorySelection.AddCard(cardHolder);
            }
        }
    }
}
