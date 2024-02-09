using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace Moonstorm
{
    public abstract class ItemModuleBase : ContentModule<ItemBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<ItemDef, ItemBase> MoonstormItems { get; private set; }
        internal static Dictionary<ItemDef, ItemBase> items = new Dictionary<ItemDef, ItemBase>();

        public static ItemDef[] LoadedItemDefs { get => MoonstormItems.Keys.ToArray(); }

        public static ResourceAvailability moduleAvailability;
        #endregion

        //Due to potential timing issues, there is the posibility of the ContagiousItemManager's init to run before SystemInit, which would be too late for us to add new void items.
        //So this travesty lies here.
        [SystemInitializer]
        private static void VoidItemHooks() => On.RoR2.Items.ContagiousItemManager.Init += FinishVoidItemBases;

        [SystemInitializer(typeof(ItemCatalog))]
        private static void SystemInit()
        {
            MSULog.Info($"Initializing Item Module...");

            MoonstormItems = new ReadOnlyDictionary<ItemDef, ItemBase>(items);
            items = null;

            moduleAvailability.MakeAvailable();
        }

        private static void FinishVoidItemBases(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            VoidItemBase[] voidItems = items == null ? MoonstormItems.Values.OfType<VoidItemBase>().ToArray() : items.Values.OfType<VoidItemBase>().ToArray();
            ItemRelationshipType contagiousItem = Addressables.LoadAssetAsync<ItemRelationshipType>("RoR2/DLC1/Common/ContagiousItem.asset").WaitForCompletion();

            for (int i = 0; i < voidItems.Length; i++)
            {
                VoidItemBase itemBase = voidItems[i];
                try
                {
                    ItemDef[] itemsToInfect = itemBase.LoadItemsToInfect().ToArray();
                    if (itemsToInfect.Length == 0)
                    {
                        throw new Exception($"The VoidItemBase {itemBase.GetType().Name} failed to provide any item to infect, Is the function returning ItemDefs properly?");
                    }

                    for (int j = 0; j < itemsToInfect.Length; j++)
                    {
                        ItemDef itemToInfect = itemsToInfect[j];
                        try
                        {
                            ItemDef.Pair transformation = new ItemDef.Pair
                            {
                                itemDef1 = itemToInfect,
                                itemDef2 = itemBase.ItemDef
                            };
                            ItemDef.Pair[] existingInfections = ItemCatalog.itemRelationships[contagiousItem];
                            HG.ArrayUtils.ArrayAppend(ref existingInfections, in transformation);
                            ItemCatalog.itemRelationships[contagiousItem] = existingInfections;
                        }
                        catch (Exception e)
                        {
                            MSULog.Error($"Failed to add transformation of {itemToInfect} to {itemBase.ItemDef}\n{e}");
                        }
                    }
                }
                catch (Exception e)
                {
                    MSULog.Error($"VoidItemBase {itemBase.GetType().Name} failed to intialize properly\n{e}");
                }
            }
            orig();
        }

        #region Items
        protected virtual IEnumerable<ItemBase> GetItemBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Items found inside {GetType().Assembly}");
#endif
            return GetContentClasses<ItemBase>();
        }

        protected void AddItem(ItemBase item, Dictionary<ItemDef, ItemBase> dictionary = null)
        {
            InitializeContent(item);
            dictionary?.Add(item.ItemDef, item);
#if DEBUG
            MSULog.Debug($"Item {item.ItemDef} Initialized and Ensured in {SerializableContentPack.name}");
#endif
        }

        protected override void InitializeContent(ItemBase contentClass)
        {
            AddSafely(ref SerializableContentPack.itemDefs, contentClass.ItemDef);
            contentClass.Initialize();
            items[contentClass.ItemDef] = contentClass;
        }
        #endregion
    }
}
