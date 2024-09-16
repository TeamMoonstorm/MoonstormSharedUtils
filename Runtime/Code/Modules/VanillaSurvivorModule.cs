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
using UnityEngine.AddressableAssets;
using static RoR2.RoR2Content;

namespace MSU
{
    public static class VanillaSurvivorModule
    {
        public static ReadOnlyDictionary<SurvivorDef, IVanillaSurvivorContentPiece[]> moonstormVanillaSurvivorsContentPieces { get; private set; }
        private static Dictionary<SurvivorDef, IVanillaSurvivorContentPiece[]> _moonstormVanillaSurvivorsContentPieces = new Dictionary<SurvivorDef, IVanillaSurvivorContentPiece[]>();

        public static ResourceAvailability moduleAvailability;

        private static Dictionary<BaseUnityPlugin, IVanillaSurvivorContentPiece[]> _pluginToVanillaSurvivorContentPieces = new Dictionary<BaseUnityPlugin, IVanillaSurvivorContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider>();

        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        public static IVanillaSurvivorContentPiece[] GetVanillaSurvivorContentPieces(BaseUnityPlugin plugin)
        {
            if (_pluginToVanillaSurvivorContentPieces.TryGetValue(plugin, out var vanillaSurvivorContentPieces))
                return vanillaSurvivorContentPieces;

#if DEBUG
            MSULog.Info($"{plugin} has no registered VanillaSurvivorContentPieces");
#endif

            return Array.Empty<IVanillaSurvivorContentPiece>();
        }

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
        private static void SystemInit()
        {
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

                    if(piece is IContentPackModifier packModifier)
                    {
                        packModifier.ModifyContentPack(provider.contentPack);
                    }

                    IVanillaSurvivorContentPiece[] array = Array.Empty<IVanillaSurvivorContentPiece>();

                    if(!_moonstormVanillaSurvivorsContentPieces.ContainsKey(survivorDef))
                        _moonstormVanillaSurvivorsContentPieces.Add(survivorDef, Array.Empty<IVanillaSurvivorContentPiece>());
                    else
                    {
                        array = _moonstormVanillaSurvivorsContentPieces[survivorDef];
                        HG.ArrayUtils.ArrayAppend(ref array, piece);
                        _moonstormVanillaSurvivorsContentPieces[survivorDef] = array;
                    }    

                    if(!_pluginToVanillaSurvivorContentPieces.ContainsKey(plugin))
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