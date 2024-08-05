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
    /// <summary>
    /// The InteractableModule is a Module that handles classes that implement <see cref="IInteractableContentPiece"/>.
    /// <para>The InteractableModule's main job is to handle the proper additon of the Interactable to the NetworkedObjects array of your ContentPack. Alongside addition of Interactables to stages using <see cref="InteractableCardProvider"/> and <see cref="DirectorAPI"/></para>
    /// </summary>
    public static class InteractableModule
    {
        /// <summary>
        /// A ReadOnlyDictionary that can be used for finding an Interactable's IInteractableContentPiece.
        /// <br>The ReadOnlyDictionary has a special Key Evaluator that Evaluates keys based off the IInteractable's <see cref="NetworkIdentity.assetId"/> to check if two interactables are the same. This allows instances of the interactable to be used as keys.</br>
        /// <br>Subscribe to <see cref="moduleAvailability"/> to ensure the dictionary is not empty.</br>
        /// </summary>
        public static ReadOnlyDictionary<IInteractable, IInteractableContentPiece> MoonstormInteractables { get; private set; }
        private static Dictionary<IInteractable, IInteractableContentPiece> _moonstormInteractables = new Dictionary<IInteractable, IInteractableContentPiece>(new IInteractableNetworkIdentityAssetIDComparer());

        /// <summary>
        /// Represents the Availability of this Module.
        /// </summary>
        public static ResourceAvailability moduleAvailability;

        private static Dictionary<BaseUnityPlugin, IInteractableContentPiece[]> _pluginToInteractables = new Dictionary<BaseUnityPlugin, IInteractableContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<GameObject>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<GameObject>>();

        private static HashSet<InteractableCardProvider> _interactableCardProviders = new HashSet<InteractableCardProvider>();

        /// <summary>
        /// Adds a new provider to the InteractableModule
        /// <br>For more info, see <see cref="IContentPieceProvider"/></br>
        /// </summary>
        /// <param name="plugin">The plugin that's adding the new provider</param>
        /// <param name="provider">The provider from the plugin, can be one created using <see cref="ContentUtil.CreateGameObjectContentPieceProvider{T}(BaseUnityPlugin, RoR2.ContentManagement.ContentPack)"/></param>
        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<GameObject> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        /// <summary>
        /// Obtains all the InteractableContentPieces that where added by the specified plugin
        /// </summary>
        /// <param name="plugin">The plugin to obtain it's Interactables</param>
        /// <returns>An array of IInteractableContentPieces, if the plugin has not added any Interactables, it returns an empty Array.</returns>
        public static IInteractableContentPiece[] GetInteractables(BaseUnityPlugin plugin)
        {
            if(_pluginToInteractables.TryGetValue(plugin, out var interactableContentPieces))
            {
                return interactableContentPieces;
            }
#if DEBUG
            MSULog.Info($"{plugin} has no registered interactables");
#endif
            return Array.Empty<IInteractableContentPiece>();
        }

        /// <summary>
        /// A Coroutine used to initialize the Characters added by <paramref name="plugin"/>
        /// <br>The coroutine yeild breaks if the plugin has not added a provider using <see cref="AddProvider(BaseUnityPlugin, IContentPieceProvider{GameObject})"/></br>
        /// </summary>
        /// <param name="plugin">The plugin to initialize it's Interactables</param>
        /// <returns>A Coroutine enumerator that can be Awaited or Yielded</returns>
        public static IEnumerator InitializeInteractables(BaseUnityPlugin plugin)
        {
#if DEBUG
            if (!_pluginToContentProvider.ContainsKey(plugin))
            {
                MSULog.Info($"{plugin} has no IContentPieceProvider registered in the InteractableModule.");
            }
#endif
            if (_pluginToContentProvider.TryGetValue(plugin, out IContentPieceProvider<GameObject> provider))
            {
                var enumerator = InitializeInteractablesFromProvider(plugin, provider);
                while (enumerator.MoveNext())
                {
                    yield return null;
                }
            }
            yield break;
        }

        [SystemInitializer]
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

            var helper = new ParallelMultiStartCoroutine();
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
#if DEBUG
                try
                {
#endif
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
#if DEBUG
                }
                catch (Exception ex) 
                {
                    MSULog.Error($"Interactable {interactable.GetType().FullName} threw an exception while initializing.\n{ex}");
                }
#endif
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
