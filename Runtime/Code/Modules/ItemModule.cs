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
    public static class ItemModule
    {
        public static ReadOnlyDictionary<ItemDef, IItemContentPiece> MoonstormItems { get; private set; }
        private static Dictionary<ItemDef, IItemContentPiece> _moonstormItems = new Dictionary<ItemDef, IItemContentPiece>();

        public static ResourceAvailability moduleAvailability;


        private static Dictionary<BaseUnityPlugin, IItemContentPiece[]> _pluginToItems = new Dictionary<BaseUnityPlugin, IItemContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<ItemDef>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<ItemDef>>();
        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<ItemDef> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        public static IItemContentPiece[] GetItems(BaseUnityPlugin plugin)
        {
            if(_pluginToItems.TryGetValue(plugin, out var items))
            {
                return items;
            }
            return null;
        }

        public static IEnumerator InitializeItems(BaseUnityPlugin plugin)
        {
            if(_pluginToContentProvider.TryGetValue(plugin, out IContentPieceProvider<ItemDef> provider))
            {
                yield return InitializeItemsFromProvider(plugin, provider);
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

            foreach(var item in content)
            {
                yield return InitializeItem(item, plugin, provider);
            }
        }

        private static IEnumerator InitializeItem(IContentPiece<ItemDef> item, BaseUnityPlugin plugin, IContentPieceProvider<ItemDef> provider)
        {
            if (!item.IsAvailable())
            {
                yield break;
            }

            yield return item.LoadContentAsync();

            item.Initialize();

            var asset = item.Asset;
            provider.ContentPack.itemDefs.AddSingle(asset);

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
                    UnlockableManager.AddUnlockables(unlockableDefs);
                    provider.ContentPack.unlockableDefs.Add(unlockableDefs);
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
                            ItemDef.Pair[] existingInfections = ItemCatalog.itemRelationships[contagiousItem];
                            HG.ArrayUtils.ArrayAppend(ref existingInfections, transformation);
                            ItemCatalog.itemRelationships[contagiousItem] = existingInfections;
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
            orig();
        }

        static ItemModule()
        {
            On.RoR2.Items.ContagiousItemManager.Init += AddVoidItems;
        }
    }
}