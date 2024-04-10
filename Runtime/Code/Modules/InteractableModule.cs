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
using UnityEngine.Networking;

namespace MSU
{
    public static class InteractableModule
    {
        public static ReadOnlyDictionary<IInteractable, IInteractableContentPiece> MoonstormInteractables { get; private set; }
        private static Dictionary<IInteractable, IInteractableContentPiece> _moonstormInteractables = new Dictionary<IInteractable, IInteractableContentPiece>();

        public static ResourceAvailability moduleAvailability;

        private static Dictionary<BaseUnityPlugin, IInteractableContentPiece[]> _pluginToInteractables = new Dictionary<BaseUnityPlugin, IInteractableContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<GameObject>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<GameObject>>();

        private static HashSet<InteractableCardProvider> _interactableCardProviders = new HashSet<InteractableCardProvider>();

        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<GameObject> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        public static IInteractableContentPiece[] GetInteractables(BaseUnityPlugin plugin)
        {
            if(_pluginToInteractables.TryGetValue(plugin, out var interactableContentPieces))
            {
                return interactableContentPieces;
            }

            return null;
        }

        public static IEnumerator InitializeInteractables(BaseUnityPlugin plugin)
        {
            if(_pluginToContentProvider.TryGetValue(plugin, out IContentPieceProvider<GameObject> provider))
            {
                var enumerator = InitializeInteractablesFromProvider(plugin, provider);
                while (enumerator.MoveNext())
                {
                    yield return null;
                }
            }
            yield break;
        }

        private static void SystemInit()
        {
            MSULog.Info("Initializing Interactable Module...");
            DirectorAPI.InteractableActions += AddCustomInteractables;

            MoonstormInteractables = new ReadOnlyDictionary<IInteractable, IInteractableContentPiece>(_moonstormInteractables);
            _moonstormInteractables = null;

            moduleAvailability.MakeAvailable();
        }

        private static IEnumerator InitializeInteractablesFromProvider(BaseUnityPlugin plugin, IContentPieceProvider<GameObject> provider)
        {
            IGameObjectContentPiece<IInteractable>[] content = provider.GetContents().OfType<IGameObjectContentPiece<IInteractable>>().ToArray();
            List<IGameObjectContentPiece<IInteractable>> interactables = new List<IGameObjectContentPiece<IInteractable>>();

            var helper = new ParallelCoroutineHelper();
            foreach (var interactable in content)
            {
                if (!interactable.IsAvailable(provider.ContentPack))
                    continue;

                interactables.Add(interactable);
                helper.Add(interactable.LoadContentAsync);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            InitializeInteractables(plugin, interactables, provider);
        }

        private static void InitializeInteractables(BaseUnityPlugin plugin, List<IGameObjectContentPiece<IInteractable>> interactables, IContentPieceProvider<GameObject> provider)
        {
            foreach(var interactable in interactables)
            {
                interactable.Initialize();

                var asset = interactable.Asset;
                if (asset.TryGetComponent<NetworkIdentity>(out _))
                {
                    provider.ContentPack.networkedObjectPrefabs.AddSingle(asset);
                }

                if (interactable is IContentPackModifier packModifier)
                {
                    packModifier.ModifyContentPack(provider.ContentPack);
                }

                if (interactable is IInteractableContentPiece interactableContentPiece)
                {
                    if (!_pluginToInteractables.ContainsKey(plugin))
                    {
                        _pluginToInteractables.Add(plugin, Array.Empty<IInteractableContentPiece>());
                    }
                    var array = _pluginToInteractables[plugin];
                    HG.ArrayUtils.ArrayAppend(ref array, interactableContentPiece);

                    if (interactableContentPiece.CardProvider)
                    {
                        _interactableCardProviders.Add(interactableContentPiece.CardProvider);
                    }
                    _moonstormInteractables.Add(interactableContentPiece.Component, interactableContentPiece);
                }

                if (interactable is IUnlockableContent unlockableContent)
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

        private static void AddCustomInteractables(DccsPool pool, DirectorAPI.StageInfo stageInfo)
        {
            foreach(var interactableCardProvider in _interactableCardProviders)
            {
                AddCustomInteractable(interactableCardProvider, pool, stageInfo);
            }
        }

        private static void AddCustomInteractable(InteractableCardProvider interactableCardProvider, DccsPool pool, DirectorAPI.StageInfo stageInfo)
        {
            var alwaysIncluded = pool.poolCategories.SelectMany(pc => pc.alwaysIncluded.Select(pe => pe.dccs));
            var includedIfConditionsMet = pool.poolCategories.SelectMany(pc => pc.includedIfConditionsMet.Select(pe => pe.dccs));
            var includedIfNoConditionsMet = pool.poolCategories.SelectMany(pc => pc.includedIfNoConditionsMet.Select(pe => pe.dccs));

            DirectorCardCategorySelection[] selections = alwaysIncluded.Union(includedIfConditionsMet).Union(includedIfNoConditionsMet).ToArray();

            DirectorAPI.DirectorCardHolder cardHolder = null;

            if(stageInfo.stage == DirectorAPI.Stage.Custom)
            {
                interactableCardProvider.CustomStageToCards.TryGetValue(stageInfo.CustomStageName, out  cardHolder);
            }
            else
            {
                interactableCardProvider.StageToCards.TryGetValue(stageInfo.stage, out cardHolder);
            }

            if (cardHolder == null)
                return;

            if (!cardHolder.Card.IsAvailable())
                return;

            foreach(DirectorCardCategorySelection selection in selections)
            {
                selection.AddCard(cardHolder);
            }
        }
    }
}
