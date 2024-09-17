using BepInEx;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MSU
{
    /// <summary>
    /// The <see cref="VanillaSurvivorModule"/> is a Module that handles classes that implement <see cref="IVanillaSurvivorContentPiece"/>.
    /// <para>The <see cref="VanillaSurvivorModule"/>'s main job is to handle the proper initialization of new content added to vanilla survivors, this is done by keeping track of all the instances of <see cref="IVanillaSurvivorContentPiece"/> that affect a survivor. which can be obtained by using <see cref="moonstormVanillaSurvivorsContentPieces"/></para>
    /// </summary>
    public static class VanillaSurvivorModule
    {
        /// <summary>
        /// A ReadOnlyDictionary that can be used for obtaining all the <see cref="IVanillaSurvivorContentPiece"/> that modify a SurvivorDef
        /// <br>Subscribe to <see cref="moduleAvailability"/> to ensure the dictionary is not empty</br>
        /// </summary>
        public static ReadOnlyDictionary<SurvivorDef, IVanillaSurvivorContentPiece[]> moonstormVanillaSurvivorsContentPieces { get; private set; }
        private static Dictionary<SurvivorDef, IVanillaSurvivorContentPiece[]> _moonstormVanillaSurvivorsContentPieces = new Dictionary<SurvivorDef, IVanillaSurvivorContentPiece[]>();

        /// <summary>
        /// Represents the Availability of this module
        /// </summary>
        public static ResourceAvailability moduleAvailability;

        private static Dictionary<BaseUnityPlugin, IVanillaSurvivorContentPiece[]> _pluginToVanillaSurvivorContentPieces = new Dictionary<BaseUnityPlugin, IVanillaSurvivorContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider>();

        /// <summary>
        /// Adds a new provider to the VanillaSurvivorModule.
        /// <br>For more info, see <see cref="IContentPieceProvider"/></br>
        /// </summary>
        /// <param name="plugin">The plugin that's adding the new provider</param>
        /// <param name="provider">The provider from the plugin, can be one created using <see cref="ContentUtil.CreateContentPieceProvider{TContentPieceType}(BaseUnityPlugin, RoR2.ContentManagement.ContentPack)"/>. Make sure that "TContentPiece" is set to "IVanillaSurvivorContentPiece"</param>
        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        /// <summary>
        /// Retrieves all the <see cref="IVanillaSurvivorContentPiece"/> that where added by the specified plugin.
        /// </summary>
        /// <param name="plugin">The plugin to obtain it's VanillaSurvivorContentPieces</param>
        /// <returns>An array of <see cref="IVanillaSurvivorContentPiece"/>, if the plugin has not added any VanillaSurvivorContentPieces, it returns an empty array.</returns>
        public static IVanillaSurvivorContentPiece[] GetVanillaSurvivorContentPieces(BaseUnityPlugin plugin)
        {
            if (_pluginToVanillaSurvivorContentPieces.TryGetValue(plugin, out var vanillaSurvivorContentPieces))
                return vanillaSurvivorContentPieces;

#if DEBUG
            MSULog.Info($"{plugin} has no registered VanillaSurvivorContentPieces");
#endif

            return Array.Empty<IVanillaSurvivorContentPiece>();
        }

        /// <summary>
        /// A Coroutine used to initialize the VanillaSurvivorContentPieces added by <paramref name="plugin"/>.
        /// <br>The Coroutine yield breaks if the plugin has not added it's specified provider using <see cref="AddProvider(BaseUnityPlugin, IContentPieceProvider)"/></br>
        /// </summary>
        /// <param name="plugin">The plugin to initialize it's VanillaSurvivorContentPieces</param>
        /// <returns>A Coroutine that can be awaited or yielded</returns>
        public static IEnumerator InitializeVanillaSurvivorContentPieces(BaseUnityPlugin plugin)
        {
#if DEBUG
            if (!_pluginToContentProvider.ContainsKey(plugin))
                MSULog.Info($"{plugin} has no IContentPieceProvider registered in the VanillaSurvivorModule.");
#endif

            if (_pluginToContentProvider.TryGetValue(plugin, out IContentPieceProvider provider))
            {
                var enumerator = InitializeVanillaSurvivorContentPiecesFromProvider(plugin, provider);
                while (enumerator.MoveNext())
                {
                    yield return null;
                }
            }
            yield break;
        }

        [SystemInitializer(typeof(SurvivorCatalog))]
        private static IEnumerator SystemInit()
        {
            yield return null;
            moonstormVanillaSurvivorsContentPieces = new ReadOnlyDictionary<SurvivorDef, IVanillaSurvivorContentPiece[]>(_moonstormVanillaSurvivorsContentPieces);
            _moonstormVanillaSurvivorsContentPieces = null;

            moduleAvailability.MakeAvailable();
        }

        private static IEnumerator InitializeVanillaSurvivorContentPiecesFromProvider(BaseUnityPlugin plugin, IContentPieceProvider provider)
        {
            IVanillaSurvivorContentPiece[] content = provider.GetContents().OfType<IVanillaSurvivorContentPiece>().ToArray();
            List<IVanillaSurvivorContentPiece> vanillaSurvivors = new List<IVanillaSurvivorContentPiece>();

            var helper = new ParallelMultiStartCoroutine();
            foreach (var addition in content)
            {
                if (!addition.IsAvailable(provider.contentPack))
                    continue;

                vanillaSurvivors.Add(addition);
                helper.Add(addition.LoadContentAsync);
            }

            helper.Start();
            while (!helper.isDone)
                yield return null;

            var subroutine = InitializeVanillaSurvivorContentPieces(plugin, vanillaSurvivors, provider);
            while (!subroutine.IsDone())
                yield return null;

        }

        private static IEnumerator InitializeVanillaSurvivorContentPieces(BaseUnityPlugin plugin, List<IVanillaSurvivorContentPiece> contentPieces, IContentPieceProvider provider)
        {
            ParallelMultiStartCoroutine initializeAsyncCoroutine = new ParallelMultiStartCoroutine();
            foreach (var piece in contentPieces)
            {
#if DEBUG
                try
                {
#endif
                    piece.Initialize();
                    initializeAsyncCoroutine.Add(piece.InitializeAsync);
                    var survivorDef = piece.survivorDef;

                    if (piece is IContentPackModifier packModifier)
                    {
                        packModifier.ModifyContentPack(provider.contentPack);
                    }

                    IVanillaSurvivorContentPiece[] array = Array.Empty<IVanillaSurvivorContentPiece>();

                    if (!_moonstormVanillaSurvivorsContentPieces.ContainsKey(survivorDef))
                        _moonstormVanillaSurvivorsContentPieces.Add(survivorDef, Array.Empty<IVanillaSurvivorContentPiece>());
                    else
                    {
                        array = _moonstormVanillaSurvivorsContentPieces[survivorDef];
                        HG.ArrayUtils.ArrayAppend(ref array, piece);
                        _moonstormVanillaSurvivorsContentPieces[survivorDef] = array;
                    }

                    if (!_pluginToVanillaSurvivorContentPieces.ContainsKey(plugin))
                    {
                        _pluginToVanillaSurvivorContentPieces.Add(plugin, Array.Empty<IVanillaSurvivorContentPiece>());
                    }
                    array = _pluginToVanillaSurvivorContentPieces[plugin];
                    HG.ArrayUtils.ArrayAppend(ref array, piece);
                    _pluginToVanillaSurvivorContentPieces[plugin] = array;


#if DEBUG
                    MSULog.Info($"VanillaSurvivorAddition {piece.GetType().FullName} initialized.");
#endif
#if DEBUG
                }
                catch (Exception ex)
                {
                    MSULog.Error($"VanillaSurvivorAddition {piece.GetType().FullName} threw an exception while initializing.\n{ex}");
                }
#endif
            }

            initializeAsyncCoroutine.Start();
            while (!initializeAsyncCoroutine.isDone)
                yield return null;
        }
    }
}