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
    public static class ItemTierModule
    {
        public static ReadOnlyDictionary<ItemTierDef, IItemTierContentPiece> MoonstormItemTiers { get; private set; }
        private static Dictionary<ItemTierDef, IItemTierContentPiece> _moonstormItemTiers = new Dictionary<ItemTierDef, IItemTierContentPiece>();
        public static ResourceAvailability moduleAvailability;

        internal static Dictionary<ItemTierDef, GameObject> _itemTierToPickupFX = new Dictionary<ItemTierDef, GameObject>();
        private static Dictionary<BaseUnityPlugin, IItemTierContentPiece[]> _pluginToTiers = new Dictionary<BaseUnityPlugin, IItemTierContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<ItemTierDef>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<ItemTierDef>>();

        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<ItemTierDef> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        public static IItemTierContentPiece[] GetItemTiers(BaseUnityPlugin plugin)
        {
            if(_pluginToTiers.TryGetValue(plugin, out var tiers))
            {
                return tiers;
            }
            return null;
        }

        public static IEnumerator InitializeTiers(BaseUnityPlugin plugin)
        {
            if(_pluginToContentProvider.TryGetValue(plugin, out IContentPieceProvider<ItemTierDef> provider))
            {
                yield return InitializeItemTiersFromProvidder(plugin, provider);
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

        private static IEnumerator InitializeItemTiersFromProvidder(BaseUnityPlugin plugin, IContentPieceProvider<ItemTierDef> provider)
        {
            IContentPiece<ItemTierDef>[] content = provider.GetContents();

            foreach(var tier in content)
            {
                yield return InitializeItemTier(tier, plugin, provider);
            }
        }

        private static IEnumerator InitializeItemTier(IContentPiece<ItemTierDef> tier, BaseUnityPlugin plugin, IContentPieceProvider<ItemTierDef> provider)
        {
            if (!tier.IsAvailable())
                yield break;

            yield return tier.LoadContentAsync();

            tier.Initialize();

            var asset = tier.Asset;
            provider.ContentPack.itemTierDefs.AddSingle(asset);

            if(tier is IContentPackModifier packModifier)
            {
                packModifier.ModifyContentPack(provider.ContentPack);
            }
            if(tier is IItemTierContentPiece itemTierContentPiece)
            {
                if (!_pluginToTiers.ContainsKey(plugin))
                    _pluginToTiers.Add(plugin, Array.Empty<IItemTierContentPiece>());

                var array = _pluginToTiers[plugin];
                HG.ArrayUtils.ArrayAppend(ref array, itemTierContentPiece);
                _moonstormItemTiers.Add(asset, itemTierContentPiece);

                if(itemTierContentPiece.ColorIndex)
                {
                    ColorsAPI.AddSerializableColor(itemTierContentPiece.ColorIndex);
                    asset.colorIndex = itemTierContentPiece.ColorIndex.Value.ColorIndex;
                }
                if(itemTierContentPiece.DarkColorIndex)
                {
                    ColorsAPI.AddSerializableColor(itemTierContentPiece.DarkColorIndex);
                    asset.colorIndex = itemTierContentPiece.DarkColorIndex.Value.ColorIndex;
                }
                _itemTierToPickupFX.Add(asset, itemTierContentPiece.PickupDisplayVFX);
            }
        }
    }
}
