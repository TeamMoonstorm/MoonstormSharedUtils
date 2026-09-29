using BepInEx;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MSU
{
    public static class KeyItemModule
    {
        public static ReadOnlyDictionary<KeyItemDef, IKeyItemContentPiece> moonstormKeyItems { get; private set; }
        private static Dictionary<KeyItemDef, IKeyItemContentPiece> _moonstormKeyItems = new Dictionary<KeyItemDef, IKeyItemContentPiece>();

        public static ResourceAvailability moduleAvailability;

        private static Dictionary<BaseUnityPlugin, IKeyItemContentPiece[]> _pluginToKeyItems = new Dictionary<BaseUnityPlugin, IKeyItemContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<KeyItemDef>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<KeyItemDef>>();
    
        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<KeyItemDef> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        public static IKeyItemContentPiece[] GetKeyItems(BaseUnityPlugin plugin)
        {
            if(_pluginToKeyItems.TryGetValue(plugin, out var keyItems))
            {
                return keyItems;
            }

#if DEBUG
            MSULog.Info($"{plugin} has no registered items");
#endif
            return Array.Empty<IKeyItemContentPiece>();
        }

        public static IEnumerator InitializeKeyItems(BaseUnityPlugin plugin)
        {
#if DEBUG
            if(!_pluginToContentProvider.ContainsKey(plugin))
            {
                MSULog.Info($"{plugin} has no IContentPieceProvider registered in the KeyItemModule.");
            }
#endif

            if(_pluginToContentProvider.TryGetValue(plugin, out var provider))
            {
                var enumerator = InitializeKeyItemsFromProvider(plugin, provider);
                while(enumerator.MoveNext())
                {
                    yield return null;
                }
            }
            yield break;
        }

        [SystemInitializer(typeof(KeyItemCatalog))]
        private static IEnumerator SystemInit()
        {
            MSULog.Info($"Initializing the KeyItem Module...");

            yield return null;

            moonstormKeyItems = new ReadOnlyDictionary<KeyItemDef, IKeyItemContentPiece>(_moonstormKeyItems);
            _moonstormKeyItems = null;

            moduleAvailability.MakeAvailable();
        }

        private static IEnumerator InitializeKeyItemsFromProvider(BaseUnityPlugin plugin, IContentPieceProvider<KeyItemDef> provider)
        {
            IContentPiece<KeyItemDef>[] content = provider.GetContents();
            List<IContentPiece<KeyItemDef>> keyItems = new List<IContentPiece<KeyItemDef>>();

            var helper = new HG.Coroutines.ParallelCoroutine();
            foreach(var keyItem in content)
            {
                if (!keyItem.IsAvailable(provider.contentPack))
                    continue;

                keyItems.Add(keyItem);
                helper.Add(keyItem.LoadContentAsync());
            }

            while (!helper.IsDone())
                yield return null;

            var subroutine = InitializeKeyItems(plugin, keyItems, provider);

            while (!subroutine.IsDone())
                yield return null;
        }

        private static IEnumerator InitializeKeyItems(BaseUnityPlugin plugin, List<IContentPiece<KeyItemDef>> keyItems, IContentPieceProvider<KeyItemDef> provider)
        {
            var initializeAsyncCoroutine = new HG.Coroutines.ParallelCoroutine();
            foreach(var keyItem in keyItems)
            {
                try
                {
                    keyItem.Initialize();

                    if(keyItem is IAsyncContentInitializer asyncContentInitializer)
                    {
                        initializeAsyncCoroutine.Add(asyncContentInitializer.InitializeAsync());
                    }

                    var asset = keyItem.asset;
                    provider.contentPack.keyItemDefs.AddSingle(asset);

                    if(keyItem is IContentPackModifier packModifier)
                    {
                        packModifier.ModifyContentPack(provider.contentPack);
                    }
                    if(keyItem is IKeyItemContentPiece keyItemContentPiece)
                    {
                        if(!_pluginToKeyItems.ContainsKey(plugin))
                        {
                            _pluginToKeyItems.Add(plugin, Array.Empty<IKeyItemContentPiece>());
                        }
                        var array = _pluginToKeyItems[plugin];
                        HG.ArrayUtils.ArrayAppend(ref array, keyItemContentPiece);
                        _pluginToKeyItems[plugin] = array;

                        _moonstormKeyItems.Add(asset, keyItemContentPiece);
                    }

#if DEBUG
                    MSULog.Info($"KeyItem {keyItem.GetType().FullName} initialized.");
#endif
                }
                catch (Exception ex)
                {
                    MSULog.Fatal($"KeyItem {keyItem.GetType().FullName} threw an exception while initializing.\n{ex}");
                    InitializationExceptionWatcher.AddException(ex, plugin);
                }
            }

            while(!initializeAsyncCoroutine.IsDone())
            {
                yield return null;
            }
        }
    }
}