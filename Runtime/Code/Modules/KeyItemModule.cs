using BepInEx;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MSU
{
    /// <summary>
    /// The KeyItemModule is a Module that handles classes that implement <see cref="IKeyItemContentPiece"/>.
    /// <para>The module's main job is to handle the proper addition of KeyItemDefs to the ContentPack.</para>
    /// </summary>
    public static class KeyItemModule
    {
        /// <summary>
        /// A ReadOnlyDictionary that can be used for finding a KeyItem's IKeyItemContentPiece.
        /// <para>Subscribe to <see cref="moduleAvailability"/> to ensure the Dictionary is not Empty.</para>
        /// </summary>
        public static ReadOnlyDictionary<KeyItemDef, IKeyItemContentPiece> moonstormKeyItems { get; private set; }
        private static Dictionary<KeyItemDef, IKeyItemContentPiece> _moonstormKeyItems = new Dictionary<KeyItemDef, IKeyItemContentPiece>();

        /// <summary>
        /// Represents the Availability of this module.
        /// </summary>
        public static ResourceAvailability moduleAvailability;

        private static Dictionary<BaseUnityPlugin, IKeyItemContentPiece[]> _pluginToKeyItems = new Dictionary<BaseUnityPlugin, IKeyItemContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<KeyItemDef>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<KeyItemDef>>();
    
        /// <summary>
        /// Adds a new provider to the KeyItemModule.
        /// <br>For more info, see <see cref="IContentPieceProvider"/></br>
        /// </summary>
        /// <param name="plugin">The plugin that's adding the new provider</param>
        /// <param name="provider">The provider from the plugin, can be one created using <see cref="ContentUtil.CreateGenericContentPieceProvider{TUObjectType}(BaseUnityPlugin, RoR2.ContentManagement.ContentPack)"/></param>
        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<KeyItemDef> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        /// <summary>
        /// Obtains all the KeyItemContentPieces that where added by a specified plugin
        /// </summary>
        /// <param name="plugin">The plugin to obtain it's Items</param>
        /// <returns>An array of IKeyItemContentPieces, if the plugin has not added any items, it returns an empty Array</returns>
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

        /// <summary>
        /// Coroutine used to initialize the Items added by <paramref name="plugin"/>.
        /// <br>The coroutine yield breaks if the plugin has not added a provider using <see cref="AddProvider(BaseUnityPlugin, IContentPieceProvider{KeyItemDef})"/></br>
        /// </summary>
        /// <param name="plugin">The plugin to initialize it's Items</param>
        /// <returns>A Coroutine enumerator that can be Awaited or Yielded</returns>
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