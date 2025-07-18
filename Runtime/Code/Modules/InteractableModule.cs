﻿using BepInEx;
using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public static ReadOnlyDictionary<IInteractable, IInteractableContentPiece> moonstormInteractables { get; private set; }
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
        /// <param name="provider">The provider from the plugin, can be one created using <see cref="ContentUtil.CreateGameObjectGenericContentPieceProvider{T}(BaseUnityPlugin, RoR2.ContentManagement.ContentPack)"/></param>
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
            if (_pluginToInteractables.TryGetValue(plugin, out var interactableContentPieces))
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
        private static IEnumerator SystemInit()
        {
            MSULog.Info("Initializing the Interactable Module...");

            yield return null;

            moonstormInteractables = new ReadOnlyDictionary<IInteractable, IInteractableContentPiece>(_moonstormInteractables);
            _moonstormInteractables = null;
            moduleAvailability.MakeAvailable();

            if(moonstormInteractables.Count == 0)
            {
#if DEBUG
                MSULog.Info("Not doing InteractableModule hooks since no interactables are registered.");
#endif
                yield break;
            }
            DirectorAPI.InteractableActions += AddCustomInteractables;
        }

        private static IEnumerator InitializeInteractablesFromProvider(BaseUnityPlugin plugin, IContentPieceProvider<GameObject> provider)
        {
            IGameObjectContentPiece<IInteractable>[] content = provider.GetContents().OfType<IGameObjectContentPiece<IInteractable>>().ToArray();
            List<IGameObjectContentPiece<IInteractable>> interactables = new List<IGameObjectContentPiece<IInteractable>>();

            var helper = new ParallelCoroutine();
            foreach (var interactable in content)
            {
                if (!interactable.IsAvailable(provider.contentPack))
                    continue;

                interactables.Add(interactable);
                helper.Add(interactable.LoadContentAsync());
            }

            while (!helper.IsDone())
                yield return null;

            InitializeInteractables(plugin, interactables, provider);
        }

        private static void InitializeInteractables(BaseUnityPlugin plugin, List<IGameObjectContentPiece<IInteractable>> interactables, IContentPieceProvider<GameObject> provider)
        {
            foreach (var interactable in interactables)
            {
#if DEBUG
                try
                {
#endif
                    interactable.Initialize();

                    var asset = interactable.asset;
                    if (asset.TryGetComponent<NetworkIdentity>(out _))
                    {
                        provider.contentPack.networkedObjectPrefabs.AddSingle(asset);
                    }

                    if (interactable is IContentPackModifier packModifier)
                    {
                        packModifier.ModifyContentPack(provider.contentPack);
                    }

                    if (interactable is IInteractableContentPiece interactableContentPiece)
                    {
                        if (!_pluginToInteractables.ContainsKey(plugin))
                        {
                            _pluginToInteractables.Add(plugin, Array.Empty<IInteractableContentPiece>());
                        }
                        var array = _pluginToInteractables[plugin];
                        HG.ArrayUtils.ArrayAppend(ref array, interactableContentPiece);
                        _pluginToInteractables[plugin] = array;

                        if (interactableContentPiece.cardProvider)
                        {
                            _interactableCardProviders.Add(interactableContentPiece.cardProvider);
                        }
                        _moonstormInteractables.Add(interactableContentPiece.component, interactableContentPiece);
                    }

#if DEBUG
                    MSULog.Info($"Interactable {interactable.GetType().FullName} initialized.");
#endif

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
            DirectorCardCategorySelection firstCategorySelection = null;
            foreach(var poolCategory in pool.poolCategories)
            {
                foreach(var alwaysIncluded in poolCategory.alwaysIncluded)
                {
                    if(alwaysIncluded.dccs)
                    {
                        firstCategorySelection = alwaysIncluded.dccs;
                        break;
                    }
                }

                if (firstCategorySelection)
                    break;
            }

            foreach (var interactableCardProvider in _interactableCardProviders)
            {
                AddCustomInteractable(interactableCardProvider, firstCategorySelection, stageInfo);
            }
        }

        private static void AddCustomInteractable(InteractableCardProvider interactableCardProvider, DirectorCardCategorySelection dccs, DirectorAPI.StageInfo stageInfo)
        {
            DirectorCardHolderExtended cardHolder = null;

            if (stageInfo.stage == DirectorAPI.Stage.Custom)
            {
                interactableCardProvider.customStageToCards.TryGetValue(stageInfo.CustomStageName, out cardHolder);
            }
            else
            {
                interactableCardProvider.stageToCards.TryGetValue(stageInfo.stage, out cardHolder);
            }

            if (cardHolder == null)
                return;

            if (!cardHolder.IsAvailable())
                return;

            dccs.AddCard(cardHolder);
        }
    }
}
