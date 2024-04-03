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
using static RoR2.RoR2Content;

namespace MSU
{
    public static class ArtifactModule
    {
        public static ReadOnlyDictionary<ArtifactDef, IArtifactContentPiece> MoonstormArtifacts { get; private set; }
        private static Dictionary<ArtifactDef, IArtifactContentPiece> _moonstormArtifacts = new Dictionary<ArtifactDef, IArtifactContentPiece>();

        public static ResourceAvailability moduleAvailability;


        private static Dictionary<BaseUnityPlugin, IArtifactContentPiece[]> _pluginToArtifacts = new Dictionary<BaseUnityPlugin, IArtifactContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<ArtifactDef>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<ArtifactDef>>();

        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<ArtifactDef> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        public static IArtifactContentPiece[] GetItems(BaseUnityPlugin plugin)
        {
            if (_pluginToArtifacts.TryGetValue(plugin, out var artifacts))
            {
                return artifacts;
            }
            return null;
        }

        public static IEnumerator InitializeArtifacts(BaseUnityPlugin plugin)
        {
            if (_pluginToContentProvider.TryGetValue(plugin, out IContentPieceProvider<ArtifactDef> provider))
            {
                yield return InitializeArtifactsFromProvider(plugin, provider);
            }
            yield break;
        }

        [SystemInitializer(typeof(ArtifactCatalog))]
        private static void SystemInit()
        {
            MoonstormArtifacts = new ReadOnlyDictionary<ArtifactDef, IArtifactContentPiece>(_moonstormArtifacts);
            _moonstormArtifacts = null;

            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled; ;
            moduleAvailability.MakeAvailable();
        }

        private static void OnArtifactDisabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            foreach(var (artifact, contentPiece) in MoonstormArtifacts)
            {
                if(!(artifact == artifactDef))
                {
                    contentPiece.OnArtifactEnabled();
                }
            }
        }

        private static void OnArtifactEnabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            foreach(var (artifact, contentPiece) in MoonstormArtifacts)
            {
                if (!(artifact == artifactDef))
                {
                    contentPiece.OnArtifactEnabled();
                }
            }
        }

        private static IEnumerator InitializeArtifactsFromProvider(BaseUnityPlugin plugin, IContentPieceProvider<ArtifactDef> provider)
        {
            IContentPiece<ArtifactDef>[] content = provider.GetContents();
            List<IContentPiece<ArtifactDef>> artifacts = new List<IContentPiece<ArtifactDef>>();

            var helper = new ParallelCoroutineHelper();
            foreach (var artifact in content)
            {
                if (!artifact.IsAvailable(provider.ContentPack))
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
                artifact.Initialize();
                var asset = artifact.Asset;
                provider.ContentPack.artifactDefs.AddSingle(asset);

                if (artifact is IContentPackModifier packModifier)
                {
                    packModifier.ModifyContentPack(provider.ContentPack);
                }

                if (artifact is IArtifactContentPiece artifactContentPiece)
                {
                    if (!_pluginToArtifacts.ContainsKey(plugin))
                    {
                        _pluginToArtifacts.Add(plugin, Array.Empty<IArtifactContentPiece>());
                    }
                    var array = _pluginToArtifacts[plugin];
                    HG.ArrayUtils.ArrayAppend(ref array, artifactContentPiece);

                    if (artifactContentPiece.ArtifactCode)
                    {
                        ArtifactCodeAPI.AddCode(artifactContentPiece.Asset, artifactContentPiece.ArtifactCode);
                    }
                    _moonstormArtifacts.Add(asset, artifactContentPiece);
                }

                if (artifact is IUnlockableContent unlockableContent)
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
    }
} 