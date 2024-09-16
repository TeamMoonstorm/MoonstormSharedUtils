using BepInEx;
using MSU;
using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static RoR2.RoR2Content;

namespace MSU
{
    /// <summary>
    /// The ArtifactModule is a Module that handles classes that implement <see cref="IArtifactContentPiece"/>.
    /// <para>The ArtifactModule's main job is to handle the proper addition of ArtifactDefs and proper hooking usage using the game's <see cref="RunArtifactManager"/></para>
    /// <br>The ArtifactModule also implements <see cref="R2API.ScriptableObjects.ArtifactCode"/> using <see cref="ArtifactCodeAPI"/></br>
    /// </summary>
    public static class ArtifactModule
    {
        /// <summary>
        /// A ReadOnlyDictionary that can be used for finding an ArtifactDef's IArtifactContentPiece.
        /// <br>Subscribe to <see cref="moduleAvailability"/> to ensure the dictionary is not empty.</br>
        /// </summary>
        public static ReadOnlyDictionary<ArtifactDef, IArtifactContentPiece> moonstormArtifacts { get; private set; }
        private static Dictionary<ArtifactDef, IArtifactContentPiece> _moonstormArtifacts = new Dictionary<ArtifactDef, IArtifactContentPiece>();

        /// <summary>
        /// Represents the Availability of this Module.
        /// </summary>
        public static ResourceAvailability moduleAvailability;


        private static Dictionary<BaseUnityPlugin, IArtifactContentPiece[]> _pluginToArtifacts = new Dictionary<BaseUnityPlugin, IArtifactContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<ArtifactDef>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<ArtifactDef>>();

        /// <summary>
        /// Adds a new provider to the ArtifactModule.
        /// <br>For more info, see <see cref="IContentPieceProvider"/></br>
        /// </summary>
        /// <param name="plugin">The plugin that's adding the new provider</param>
        /// <param name="provider">The provider from the plugin, can be one created using <see cref="ContentUtil.CreateGenericContentPieceProvider{T}(BaseUnityPlugin, RoR2.ContentManagement.ContentPack)"/></param>
        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<ArtifactDef> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        /// <summary>
        /// Obtains all the ArtifactContentPieces that where added by the specified plugin
        /// </summary>
        /// <param name="plugin">The plugin to obtain it's artifacts.</param>
        /// <returns>An array of IArtifactContentPieces, if the plugin has not added any artifacts, it returns an empty Array.</returns>
        public static IArtifactContentPiece[] GetArtifacts(BaseUnityPlugin plugin)
        {
            if (_pluginToArtifacts.TryGetValue(plugin, out var artifacts))
            {
                return artifacts;
            }
#if DEBUG
            MSULog.Info($"{plugin} has no registered artifacts");
#endif
            return Array.Empty<IArtifactContentPiece>();
        }

        /// <summary>
        /// A Coroutine used to initialize the Artifacts added by <paramref name="plugin"/>.
        /// <br>The coroutine yield breaks if the plugin has not added it's specified provider using <see cref="AddProvider(BaseUnityPlugin, IContentPieceProvider{ArtifactDef})"/></br>
        /// </summary>
        /// <param name="plugin">The plugin to initialize it's artifacts.</param>
        /// <returns>A Coroutine enumerator that can be Awaited or Yielded</returns>
        public static IEnumerator InitializeArtifacts(BaseUnityPlugin plugin)
        {
#if DEBUG
            if(!_pluginToContentProvider.ContainsKey(plugin))
            {
                MSULog.Info($"{plugin} has no IContentPieceProvider registered in the ArtifactModule.");
            }
#endif
            if (_pluginToContentProvider.TryGetValue(plugin, out IContentPieceProvider<ArtifactDef> provider))
            {
                var enumerator = InitializeArtifactsFromProvider(plugin, provider);
                while (enumerator.MoveNext())
                {
                    yield return null;
                }
            }
            yield break;
        }

        [SystemInitializer(typeof(ArtifactCatalog))]
        private static void SystemInit()
        {
            moonstormArtifacts = new ReadOnlyDictionary<ArtifactDef, IArtifactContentPiece>(_moonstormArtifacts);
            _moonstormArtifacts = null;

            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled; ;
            moduleAvailability.MakeAvailable();
        }

        private static void OnArtifactDisabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            foreach (var kvp in moonstormArtifacts)
            {
                if (!(artifactDef != kvp.Key))
                {
#if DEBUG
                    MSULog.Info($"Running OnArtifactDisabled() for artifact {kvp.Key.cachedName}");
#endif
                    kvp.Value.OnArtifactDisabled();
                }
            }
        }

        private static void OnArtifactEnabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            foreach (var kvp in moonstormArtifacts)
            {
                if (!(artifactDef != kvp.Key) && NetworkServer.active)
                {
#if DEBUG
                    MSULog.Info($"Running OnArtifactEnabled() for artifact {kvp.Key.cachedName}");
#endif
                    kvp.Value.OnArtifactEnabled();
                }
            }
        }

        private static IEnumerator InitializeArtifactsFromProvider(BaseUnityPlugin plugin, IContentPieceProvider<ArtifactDef> provider)
        {
            IContentPiece<ArtifactDef>[] content = provider.GetContents();
            List<IContentPiece<ArtifactDef>> artifacts = new List<IContentPiece<ArtifactDef>>();

            var helper = new ParallelMultiStartCoroutine();
            foreach (var artifact in content)
            {
                if (!artifact.IsAvailable(provider.contentPack))
                    continue;

                artifacts.Add(artifact);
                helper.Add(artifact.LoadContentAsync);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            InitializeArtifacts(plugin, artifacts, provider);
        }

        private static void InitializeArtifacts(BaseUnityPlugin plugin, List<IContentPiece<ArtifactDef>> artifacts, IContentPieceProvider<ArtifactDef> provider)
        {
            foreach(var artifact in artifacts)
            {
#if DEBUG
                try 
                {
#endif
                    artifact.Initialize();
                    var asset = artifact.asset;
                    provider.contentPack.artifactDefs.AddSingle(asset);

                    if (artifact is IContentPackModifier packModifier)
                    {
                        packModifier.ModifyContentPack(provider.contentPack);
                    }

                    if (artifact is IArtifactContentPiece artifactContentPiece)
                    {
                        if (!_pluginToArtifacts.ContainsKey(plugin))
                        {
                            _pluginToArtifacts.Add(plugin, Array.Empty<IArtifactContentPiece>());
                        }
                        var array = _pluginToArtifacts[plugin];
                        HG.ArrayUtils.ArrayAppend(ref array, artifactContentPiece);
                        _pluginToArtifacts[plugin] = array;

                        if (artifactContentPiece.artifactCode)
                        {
                            ArtifactCodeAPI.AddCode(artifactContentPiece.asset, artifactContentPiece.artifactCode);
                        }
                        _moonstormArtifacts.Add(asset, artifactContentPiece);
                    }

#if DEBUG
                    MSULog.Info($"Artifact {artifact.GetType().FullName} initialized.");
#endif

#if DEBUG
                }
                catch (Exception ex)
                {
                    MSULog.Error($"Artifact {artifact.GetType().FullName} threw an exception while initializing.\n{ex}");
                }
#endif
            }
        }
    }
} 