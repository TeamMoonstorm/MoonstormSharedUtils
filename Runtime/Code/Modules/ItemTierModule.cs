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
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// The ItemTierModule is a Module that handles classes that implement <see cref="IItemTierContentPiece"/>.
    /// <para>The ItemTierModule's main job is to handle the proper addition of ItemTierDefs to the ContentPack. Alongside this, it'll also implement custom pickup FX for the tier, a list of items with said tier, and a list of pickup indices mimicking lists such as <see cref="Run.availableTier3DropList"/></para>
    /// </summary>
    public static class ItemTierModule
    {
        /// <summary>
        /// A ReadOnlyDictionary that can be used for finding an ItemTier's IItemContentPiece
        /// <para>Subscribe to <see cref="moduleAvailability"/> to ensure the Dictionary is not Empty.</para>
        /// </summary>
        public static ReadOnlyDictionary<ItemTierDef, IItemTierContentPiece> MoonstormItemTiers { get; private set; }
        private static Dictionary<ItemTierDef, IItemTierContentPiece> _moonstormItemTiers = new Dictionary<ItemTierDef, IItemTierContentPiece>();

        /// <summary>
        /// Represents the Availability of this Module.
        /// </summary>
        public static ResourceAvailability moduleAvailability;

        internal static Dictionary<ItemTierDef, GameObject> _itemTierToPickupFX = new Dictionary<ItemTierDef, GameObject>();
        private static Dictionary<BaseUnityPlugin, IItemTierContentPiece[]> _pluginToTiers = new Dictionary<BaseUnityPlugin, IItemTierContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<ItemTierDef>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<ItemTierDef>>();

        /// <summary>
        /// Adds a new provider to the ItemTierModule.
        /// <br>For more info, see <see cref="IContentPieceProvider"/></br>
        /// </summary>
        /// <param name="plugin">The plugin that's adding the new provider.</param>
        /// <param name="provider">The provider from the plugin, can be one created using <see cref="ContentUtil.CreateContentPieceProvider{T}(BaseUnityPlugin, RoR2.ContentManagement.ContentPack)"/></param>
        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<ItemTierDef> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        /// <summary>
        /// Obtains all the ItemTierContentPieces that where added by a specified plugin
        /// </summary>
        /// <param name="plugin">The plugin to obtain it's ItemTiers</param>
        /// <returns>An array of IItemTierContentPiece, if the plugin has not added any ItemTiers, it returns an empty Array</returns>
        public static IItemTierContentPiece[] GetItemTiers(BaseUnityPlugin plugin)
        {
            if(_pluginToTiers.TryGetValue(plugin, out var tiers))
            {
                return tiers;
            }
#if DEBUG
            MSULog.Info($"{plugin} has no registered ItemTiers");
#endif
            return Array.Empty<IItemTierContentPiece>();
        }

        /// <summary>
        /// Coroutine used to initialize the ItemTiers added by <paramref name="plugin"/>
        /// <br>The coroutine yield breaks if the plugin has not added a provider using <see cref="AddProvider(BaseUnityPlugin, IContentPieceProvider{ItemTierDef})"/></br>
        /// </summary>
        /// <param name="plugin">The plugin to initialize it's Items</param>
        /// <returns>A Coroutine enumerator that can be Awaited or Yielded.</returns>
        public static IEnumerator InitializeTiers(BaseUnityPlugin plugin)
        {
#if DEBUG
            if (!_pluginToContentProvider.ContainsKey(plugin))
            {
                MSULog.Info($"{plugin} has no IContentPieceProvider registered in the ItemTierModule.");
            }
#endif
            if (_pluginToContentProvider.TryGetValue(plugin, out IContentPieceProvider<ItemTierDef> provider))
            {
                var enumerator = InitializeItemTiersFromProvider(plugin, provider);
                while (enumerator.MoveNext())
                {
                    yield return null;
                }
            }
        }

        [SystemInitializer(typeof(ItemCatalog), typeof(ItemTierCatalog))]
        private static void SystemInit()
        {
            MSULog.Info("Initializing Item Tier Module...");
            MoonstormItemTiers = new ReadOnlyDictionary<ItemTierDef, IItemTierContentPiece>(_moonstormItemTiers);
            _moonstormItemTiers = null;
            BuildItemListForEachItemTier();
            Run.onRunStartGlobal += BuildDropTable;
            On.RoR2.PickupDisplay.DestroyModel += DestroyCustomModel;
            On.RoR2.PickupDisplay.RebuildModel += RebuildCustomModel;

            moduleAvailability.MakeAvailable();
        }

        private static void RebuildCustomModel(On.RoR2.PickupDisplay.orig_RebuildModel orig, PickupDisplay self)
        {
            var component = self.gameObject.EnsureComponent<ItemTierPickupDisplayHelper>();
            orig(self);
            component.RebuildCustomModel();
        }

        private static void DestroyCustomModel(On.RoR2.PickupDisplay.orig_DestroyModel orig, PickupDisplay self)
        {
            var component = self.gameObject.EnsureComponent<ItemTierPickupDisplayHelper>();
            orig(self);
            component.DestroyCustomModel();
        }

        private static void BuildDropTable(Run obj)
        {
            foreach(var (itemTierDef, itemTierContentPiece) in MoonstormItemTiers)
            {
                itemTierContentPiece.AvailableTierDropList.Clear();
                foreach(var itemIndex in itemTierContentPiece.ItemsWithThisTier)
                {
                    if(obj.availableItems.Contains(itemIndex))
                    {
                        itemTierContentPiece.AvailableTierDropList.Add(PickupCatalog.FindPickupIndex(itemIndex));
                    }
                }
            }
        }

        private static void BuildItemListForEachItemTier()
        {
            foreach(var (itemTierDef, itemTierContentPiece) in MoonstormItemTiers)
            {
                itemTierContentPiece.ItemsWithThisTier.Clear();
                foreach(ItemDef itemDef in ItemCatalog.allItemDefs)
                {
                    if(itemDef.tier == itemTierDef.tier)
                    {
                        itemTierContentPiece.ItemsWithThisTier.Add(itemDef.itemIndex);
                    }
                }
            }
        }

        private static IEnumerator InitializeItemTiersFromProvider(BaseUnityPlugin plugin, IContentPieceProvider<ItemTierDef> provider)
        {
            IContentPiece<ItemTierDef>[] content = provider.GetContents();
            List<IContentPiece<ItemTierDef>> itemTiers = new List<IContentPiece<ItemTierDef>>();

            var helper = new ParallelMultiStartCoroutine();
            foreach(var tier in content)
            {
                if (!tier.IsAvailable(provider.ContentPack))
                    continue;

                itemTiers.Add(tier);
                helper.Add(tier.LoadContentAsync);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            InitializeItemTiers(plugin, itemTiers, provider);
        }

        private static void InitializeItemTiers(BaseUnityPlugin plugin, List<IContentPiece<ItemTierDef>> itemTiers, IContentPieceProvider<ItemTierDef> provider)
        {
            foreach(var tier in itemTiers)
            {
#if DEBUG
                try
                {
#endif
                    tier.Initialize();

                    var asset = tier.Asset;
                    provider.ContentPack.itemTierDefs.AddSingle(asset);

                    if (tier is IContentPackModifier packModifier)
                    {
                        packModifier.ModifyContentPack(provider.ContentPack);
                    }
                    if (tier is IItemTierContentPiece itemTierContentPiece)
                    {
                        if (!_pluginToTiers.ContainsKey(plugin))
                            _pluginToTiers.Add(plugin, Array.Empty<IItemTierContentPiece>());

                        var array = _pluginToTiers[plugin];
                        HG.ArrayUtils.ArrayAppend(ref array, itemTierContentPiece);
                        _moonstormItemTiers.Add(asset, itemTierContentPiece);

                        if (itemTierContentPiece.ColorIndex)
                        {
                            ColorsAPI.AddSerializableColor(itemTierContentPiece.ColorIndex);
                            asset.colorIndex = itemTierContentPiece.ColorIndex.Value.ColorIndex;
                        }
                        if (itemTierContentPiece.DarkColorIndex)
                        {
                            ColorsAPI.AddSerializableColor(itemTierContentPiece.DarkColorIndex);
                            asset.colorIndex = itemTierContentPiece.DarkColorIndex.Value.ColorIndex;
                        }
                        _itemTierToPickupFX.Add(asset, itemTierContentPiece.PickupDisplayVFX);
                    }
#if DEBUG
                }
                catch (Exception ex)
                {
                    MSULog.Error($"ItemTier {tier.GetType().FullName} threw an exception while initializing.\n{ex}");
                }
#endif
            }
        }
    }
}
