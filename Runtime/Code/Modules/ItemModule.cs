using BepInEx;
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
    /// <summary>
    /// The ItemModule is a Module that handles classes that implement <see cref="IItemContentPiece"/> and <see cref="IVoidItemContentPiece"/>.
    /// <para>The ItemModule's main job is to handle the proper addition of ItemDefs to the ContentPack. Alongside setting up proper corruption logic for Boss items (Adding them to the <see cref="DLC1Content.Items.VoidMegaCrabItem"/>'s corruption pool). And proper corruption for Void Items added via <see cref="IVoidItemContentPiece"/></para>
    /// </summary>
    public static class ItemModule
    {
        /// <summary>
        /// A ReadOnlyDictionary that can be used for finding an Item's IItemContentPiece
        /// <para>Subscribe to <see cref="moduleAvailability"/> to ensure the Dictionary is not Empty.</para>
        /// </summary>
        public static ReadOnlyDictionary<ItemDef, IItemContentPiece> MoonstormItems { get; private set; }
        private static Dictionary<ItemDef, IItemContentPiece> _moonstormItems = new Dictionary<ItemDef, IItemContentPiece>();

        /// <summary>
        /// Represents the Availability of this Module
        /// </summary>
        public static ResourceAvailability moduleAvailability;


        private static Dictionary<BaseUnityPlugin, IItemContentPiece[]> _pluginToItems = new Dictionary<BaseUnityPlugin, IItemContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<ItemDef>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<ItemDef>>();
        private static HashSet<ItemDef> _bossItems = new HashSet<ItemDef>();

        /// <summary>
        /// Adds a new provider to the ItemModule.
        /// <br>For more info, see <see cref="IContentPieceProvider"/></br>
        /// </summary>
        /// <param name="plugin">The plugin that's adding the new provider</param>
        /// <param name="provider">The provider from the plugin, can be one created using <see cref="ContentUtil.CreateContentPieceProvider{T}(BaseUnityPlugin, RoR2.ContentManagement.ContentPack)"/></param>
        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<ItemDef> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        /// <summary>
        /// Obtains all the ItemContentPieces that where added by a specified plugin
        /// </summary>
        /// <param name="plugin">The plugin to obtain it's Items</param>
        /// <returns>An array of IItemContentPieces, if the plugin has not added any Items, it returns an empty Array</returns>
        public static IItemContentPiece[] GetItems(BaseUnityPlugin plugin)
        {
            if(_pluginToItems.TryGetValue(plugin, out var items))
            {
                return items;
            }
            return Array.Empty<IItemContentPiece>();
        }

        /// <summary>
        /// Coroutine used to initialize the Items added by <paramref name="plugin"/>.
        /// <br>The coroutine yield breaks if the plugin has not added a provider using <see cref="AddProvider(BaseUnityPlugin, IContentPieceProvider{ItemDef})"/></br>
        /// </summary>
        /// <param name="plugin">The plugin to initialize it's Items</param>
        /// <returns>A Coroutine enumerator that can be Awaited or Yielded.</returns>
        public static IEnumerator InitializeItems(BaseUnityPlugin plugin)
        {
            if(_pluginToContentProvider.TryGetValue(plugin, out IContentPieceProvider<ItemDef> provider))
            {
                var enumerator = InitializeItemsFromProvider(plugin, provider);
                while(enumerator.MoveNext())
                {
                    yield return null;
                }
            }
            yield break;
        }

        [SystemInitializer(typeof(ItemCatalog))]
        private static void SystemInit()
        {
            MoonstormItems = new ReadOnlyDictionary<ItemDef, IItemContentPiece>(_moonstormItems);
            _moonstormItems = null;

            moduleAvailability.MakeAvailable();
        }
        private static IEnumerator InitializeItemsFromProvider(BaseUnityPlugin plugin, IContentPieceProvider<ItemDef> provider)
        {
            IContentPiece<ItemDef>[] content = provider.GetContents();
            List<IContentPiece<ItemDef>> items = new List<IContentPiece<ItemDef>>();

            var helper = new ParallelMultiStartCoroutine();
            foreach(var item in content)
            {
                if (!item.IsAvailable(provider.ContentPack))
                    continue;

                items.Add(item);
                helper.Add(item.LoadContentAsync);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            InitializeItems(plugin, items, provider);
        }

        private static void InitializeItems(BaseUnityPlugin plugin, List<IContentPiece<ItemDef>> items, IContentPieceProvider<ItemDef> provider)
        {
            foreach(var item in items)
            {
                item.Initialize();

                var asset = item.Asset;
                provider.ContentPack.itemDefs.AddSingle(asset);

                if(asset.deprecatedTier == ItemTier.Boss)
                {
                    _bossItems.Add(asset);
                }

                if (item is IContentPackModifier packModifier)
                {
                    packModifier.ModifyContentPack(provider.ContentPack);
                }
                if (item is IItemContentPiece itemContentPiece)
                {
                    if (!_pluginToItems.ContainsKey(plugin))
                    {
                        _pluginToItems.Add(plugin, Array.Empty<IItemContentPiece>());
                    }
                    var array = _pluginToItems[plugin];
                    HG.ArrayUtils.ArrayAppend(ref array, itemContentPiece);
                    _moonstormItems.Add(asset, itemContentPiece);
                }

                if (item is IUnlockableContent unlockableContent)
                {
                    UnlockableDef[] unlockableDefs = unlockableContent.TiedUnlockables;
                    if (unlockableDefs.Length > 0)
                    {
                        UnlockableManager.AddUnlockables(unlockableDefs.OfType<AchievableUnlockableDef>().ToArray());
                        provider.ContentPack.unlockableDefs.Add(unlockableDefs);
                    }
                }
            }
        }

        private static void AddVoidItems(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            IVoidItemContentPiece[] voidItems = _moonstormItems == null ? MoonstormItems.Values.OfType<IVoidItemContentPiece>().ToArray() : _moonstormItems.Values.OfType<IVoidItemContentPiece>().ToArray();

            ItemRelationshipType contagiousItem = Addressables.LoadAssetAsync<ItemRelationshipType>("RoR2/DLC1/Common/ContagiousItem.asset").WaitForCompletion();

            for (int i = 0; i < voidItems.Length; i++)
            {
                IVoidItemContentPiece voidItem = voidItems[i];  
                try
                {
                    List<ItemDef> itemsToInfect = voidItem.GetInfectableItems();
                    if(itemsToInfect.Count == 0)
                    {
                        throw new Exception($"The IVoidItemContentPiece {voidItem.GetType().Name} failed to provide any item to infect, Is the function returning ItemDefs properly?");
                    }

                    for(int j = 0; j < itemsToInfect.Count; j++)
                    {
                        ItemDef itemToInfect = itemsToInfect[j];
                        try
                        {
                            ItemDef.Pair transformation = new ItemDef.Pair
                            {
                                itemDef1 = itemToInfect,
                                itemDef2 = voidItem.Asset
                            };
                            ItemDef.Pair[] existingInfections0 = ItemCatalog.itemRelationships[contagiousItem];
                            HG.ArrayUtils.ArrayAppend(ref existingInfections0, transformation);
                            ItemCatalog.itemRelationships[contagiousItem] = existingInfections0;
                        }
                        catch(Exception ex)
                        {
                            MSULog.Error($"Failed to add transformation of {itemToInfect} to {voidItem.Asset}\n{ex}");
                        }
                    }
                }
                catch(Exception ex)
                {
                    MSULog.Error($"IVoidItemContentPiece {voidItem.GetType().Name} failed to intialize properly\n{ex}");
                }
            }

            List<ItemDef.Pair> bossVoidPairs = new List<ItemDef.Pair>();
            foreach(ItemDef bossItem in _bossItems)
            {
                bossVoidPairs.Add(new ItemDef.Pair
                {
                    itemDef1 = bossItem,
                    itemDef2 = RoR2.DLC1Content.Items.VoidMegaCrabItem
                });   
            }
            ItemDef.Pair[] existingInfections1 = ItemCatalog.itemRelationships[contagiousItem];
            existingInfections1 = existingInfections1.Union(bossVoidPairs).ToArray();
            ItemCatalog.itemRelationships[contagiousItem] = existingInfections1;
            orig();
        }

        static ItemModule()
        {
            On.RoR2.Items.ContagiousItemManager.Init += AddVoidItems;
        }
    }
}