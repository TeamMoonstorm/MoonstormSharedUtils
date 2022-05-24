using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm
{
    public abstract class ItemModuleBase : ContentModule<ItemBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<ItemDef, ItemBase> MoonstormItems { get; private set; }
        internal static Dictionary<ItemDef, ItemBase> items = new Dictionary<ItemDef, ItemBase>();

        public static ItemDef[] LoadedItemDefs { get => MoonstormItems.Keys.ToArray(); }
        public static Action<ReadOnlyDictionary<ItemDef, ItemBase>> OnDictionaryCreated;
        #endregion

        [SystemInitializer(typeof(ItemCatalog))]
        private static void SystemInit()
        {
            MSULog.Info($"Initializing Item Module...");

            MoonstormItems = new ReadOnlyDictionary<ItemDef, ItemBase>(items);
            items = null;

            OnDictionaryCreated?.Invoke(MoonstormItems);
        }

        #region Items
        protected virtual IEnumerable<ItemBase> GetItemBases()
        {
            MSULog.Debug($"Getting the Items found inside {GetType().Assembly}");
            return GetContentClasses<ItemBase>();
        }

        protected void AddItem(ItemBase item, Dictionary<ItemDef, ItemBase> dictionary = null)
        {
            InitializeContent(item);
            dictionary?.Add(item.ItemDef, item);
            MSULog.Debug($"Item {item.ItemDef} Initialized and Ensured in {SerializableContentPack.name}");
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
