using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Moonstorm
{
    public abstract class ItemTierModuleBase : ContentModule<ItemTierBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<ItemTierDef, ItemTierBase> MoonstormItemTiers { get; private set; }
        internal static Dictionary<ItemTierDef, ItemTierBase> itemTiers = new Dictionary<ItemTierDef, ItemTierBase>();

        public static ItemTierDef[] LoadedItemTierDefs => MoonstormItemTiers.Keys.ToArray();

        public static ResourceAvailability moduleAvailability;
        #endregion

        [SystemInitializer(typeof(ItemTierCatalog), typeof(ItemCatalog))]
        private static void SystemInit()
        {
            MSULog.Info($"Initializing Item Tier Module");

            MoonstormItemTiers = new ReadOnlyDictionary<ItemTierDef, ItemTierBase>(itemTiers);
            itemTiers = null;
            BuildItemListForEachItemTier();


            moduleAvailability.MakeAvailable();
        }


        private static void BuildItemListForEachItemTier()
        {
            foreach (var (itemTierDef, itemTierBase) in MoonstormItemTiers)
            {
                itemTierBase.ItemsWithThisTier.Clear();
                foreach (ItemDef itemDef in ItemCatalog.allItemDefs)
                {
                    if (itemDef.tier == itemTierDef.tier)
                    {
                        itemTierBase.ItemsWithThisTier.Add(itemDef.itemIndex);
                        itemTierBase.AvailableTierDropList.Add(PickupCatalog.FindPickupIndex(itemDef.itemIndex));
                    }
                }
            }
        }

        #region ItemTiers

        protected virtual IEnumerable<ItemTierBase> GetItemTierBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Item Tiers fond inside {GetType().Assembly}");
#endif
            return GetContentClasses<ItemTierBase>();
        }

        protected void AddItemTier(ItemTierBase itemTier, Dictionary<ItemTierDef, ItemTierBase> dictionary = null)
        {
            InitializeContent(itemTier);
            dictionary?.Add(itemTier.ItemTierDef, itemTier);
#if DEBUG
            MSULog.Debug($"Item Tier {itemTier.ItemTierDef} Initialized and Ensured in {SerializableContentPack.name}");
#endif
        }

        protected override void InitializeContent(ItemTierBase contentClass)
        {
            AddSafely(ref SerializableContentPack.itemTierDefs, contentClass.ItemTierDef);
            if (contentClass.ColorIndex)
            {
                ColorsAPI.AddSerializableColor(contentClass.ColorIndex);
                contentClass.ItemTierDef.colorIndex = contentClass.ColorIndex.ColorIndex;
            }

            if (contentClass.DarkColorIndex)
            {
                ColorsAPI.AddSerializableColor(contentClass.DarkColorIndex);
                contentClass.ItemTierDef.darkColorIndex = contentClass.DarkColorIndex.ColorIndex;
            }

            contentClass.Initialize();
            itemTiers[contentClass.ItemTierDef] = contentClass;
        }
        #endregion
    }
}