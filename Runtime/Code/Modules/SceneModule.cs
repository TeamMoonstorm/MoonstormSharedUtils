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

namespace MSU
{
    /// <summary>
    /// The SceneModule is a Module that handles classes that implement <see cref="ISceneContentPiece"/>.
    /// <para>The SceneModule's main job is to handle the proper addition of SceneDefs to the ContentPack, alongside adding SceneDefs that are part of the Loop system to said loop using <see cref="R2API.StageRegistration"/></para>
    /// </summary>
    public static class SceneModule
    {
        /// <summary>
        /// A ReadOnlyDictionary that can be used for finding a SceneDef's ISceneContentPiece
        /// <para>Subscribe to <see cref="moduleAvailability"/> to ensure the Dictionary is not Empty.</para>
        /// </summary>
        public static ReadOnlyDictionary<SceneDef, ISceneContentPiece> MoonstormScenes { get; private set; }
        private static Dictionary<SceneDef, ISceneContentPiece> _moonstormScenes = new Dictionary<SceneDef, ISceneContentPiece>();

        /// <summary>
        /// Represents the Availability of this Module
        /// </summary>
        public static ResourceAvailability moduleAvailability;

        private static Dictionary<BaseUnityPlugin, ISceneContentPiece[]> _pluginToScenes = new Dictionary<BaseUnityPlugin, ISceneContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<SceneDef>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<SceneDef>>();

        /// <summary>
        /// Adds a new provider to the SceneModule
        /// <br>For more info, see <see cref="IContentPieceProvider"/></br>
        /// </summary>
        /// <param name="plugin">The plugin that's adding the new provider.</param>
        /// <param name="provider">The provider from the plugin, can be one created using <see cref="ContentUtil.CreateContentPieceProvider{T}(BaseUnityPlugin, RoR2.ContentManagement.ContentPack)"/></param>
        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<SceneDef> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        /// <summary>
        /// Obtains all the SceneContentPieces that where added by a specified plugin
        /// </summary>
        /// <param name="plugin">The plugin to obtain it's Scenes</param>
        /// <returns>An array of ISceneContentPiece, if the plugin has not added any scenes, it returns an empty Array</returns>
        public static ISceneContentPiece[] GetScenes(BaseUnityPlugin plugin)
        {
            if(!_pluginToScenes.TryGetValue(plugin, out var scenes))
            {
                return scenes;
            }
#if DEBUG
            MSULog.Info($"{plugin} has no registered scenes");
#endif
            return Array.Empty<ISceneContentPiece>();
        }

        /// <summary>
        /// Coroutine used to initialize the Scenes added by <paramref name="plugin"/>
        /// <br>The coroutine yield breaks if the plugin has not added a provider using <see cref="AddProvider(BaseUnityPlugin, IContentPieceProvider{SceneDef})"/></br>
        /// </summary>
        /// <param name="plugin">The plugin to initialize it's Scenes</param>
        /// <returns>A Coroutine enumerator that can be Awaited or Yielded</returns>
        public static IEnumerator InitializeScenes(BaseUnityPlugin plugin)
        {
#if DEBUG
            if (!_pluginToContentProvider.ContainsKey(plugin))
            {
                MSULog.Info($"{plugin} has no IContentPieceProvider registered in the SceneModule.");
            }
#endif
            if (_pluginToContentProvider.TryGetValue(plugin, out IContentPieceProvider<SceneDef> provider))
            {
                var enumerator = InitializeScenesFromProvider(plugin, provider);
                while(enumerator.MoveNext())
                {
                    yield return null;
                }
            }
            yield break;
        }

        [SystemInitializer(typeof(SceneCatalog))]
        private static void SystemInit()
        {
            MoonstormScenes = new ReadOnlyDictionary<SceneDef, ISceneContentPiece>(_moonstormScenes);
            _moonstormScenes = null;

            Stage.onServerStageBegin += Stage_onServerStageBegin;
            Stage.onServerStageComplete += Stage_onServerStageComplete;

            moduleAvailability.MakeAvailable();
        }

        private static void Stage_onServerStageComplete(Stage obj)
        {
            var sceneDef = obj.sceneDef;

            if (sceneDef && MoonstormScenes.TryGetValue(sceneDef, out var sceneContentPiece))
            {
                sceneContentPiece.OnServerStageComplete(obj);
            }
        }

        private static void Stage_onServerStageBegin(Stage obj)
        {
            var sceneDef = obj.sceneDef;

            if(sceneDef && MoonstormScenes.TryGetValue(sceneDef, out var sceneContentPiece))
            {
                sceneContentPiece.OnServerStageBegin(obj);
            }
        }

        private static IEnumerator InitializeScenesFromProvider(BaseUnityPlugin plugin, IContentPieceProvider<SceneDef> provider)
        {
            IContentPiece<SceneDef>[] content = provider.GetContents();
            List<IContentPiece<SceneDef>> _scenes = new List<IContentPiece<SceneDef>>();

            var helper = new ParallelMultiStartCoroutine();
            foreach(var scene in content)
            {
                if (!scene.IsAvailable(provider.ContentPack))
                    continue;

                _scenes.Add(scene);
                helper.Add(scene.LoadContentAsync);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            InitializeScenes(plugin, _scenes, provider);
        }

        private static void InitializeScenes(BaseUnityPlugin plugin, List<IContentPiece<SceneDef>> scenes, IContentPieceProvider<SceneDef> provider)
        {
            foreach(var scene in scenes)
            {
#if DEBUG
                try
                {
#endif
                    scene.Initialize();
                    var asset = scene.Asset;
                    provider.ContentPack.sceneDefs.AddSingle(asset);

                    if (scene is IContentPackModifier packModifier)
                    {
                        packModifier.ModifyContentPack(provider.ContentPack);
                    }

                    if (scene is ISceneContentPiece sceneContentPiece)
                    {
                        if (!_pluginToScenes.ContainsKey(plugin))
                        {
                            _pluginToScenes.Add(plugin, Array.Empty<ISceneContentPiece>());
                        }
                        var array = _pluginToScenes[plugin];
                        HG.ArrayUtils.ArrayAppend(ref array, sceneContentPiece);

                        if (sceneContentPiece.MainTrack.HasValue)
                            provider.ContentPack.musicTrackDefs.AddSingle(sceneContentPiece.MainTrack.Value);

                        if (sceneContentPiece.BossTrack.HasValue)
                            provider.ContentPack.musicTrackDefs.AddSingle(sceneContentPiece.BossTrack.Value);

                        sceneContentPiece.Asset.portalMaterial = StageRegistration.MakeBazaarSeerMaterial(sceneContentPiece.BazaarTextureBase);

                        if (sceneContentPiece.Asset.sceneType == SceneType.Stage)
                        {
                            StageRegistration.RegisterSceneDefToLoop(sceneContentPiece.Asset);
                        }
                        _moonstormScenes.Add(scene.Asset, sceneContentPiece);
                    }
#if DEBUG
                }
                catch (Exception ex)
                {
                    MSULog.Error($"Scene {scene.GetType().FullName} threw an exception while initializing.\n{ex}");
                }
#endif
            }
        }
    }
}