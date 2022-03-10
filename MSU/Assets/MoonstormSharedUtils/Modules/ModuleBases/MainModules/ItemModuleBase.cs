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
        public static ReadOnlyDictionary<ItemDef, ItemBase> MoonstormItems
        {
            get
            {
                if(!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve dictionary {MoonstormItems}", typeof(ItemModuleBase));
                    return null;
                }
                return MoonstormItems;
            }
            private set
            {
                MoonstormItems = value;
            }
        }
        internal static Dictionary<ItemDef, ItemBase> items = new Dictionary<ItemDef, ItemBase>();
        public static Action<ReadOnlyDictionary<ItemDef, ItemBase>> OnDictionaryCreated;

        public static ItemDef[] LoadedItemDefs { get => MoonstormItems.Keys.ToArray(); }

        public static bool Initialized { get; private set; } = false;
        #endregion

        [SystemInitializer(typeof(ItemCatalog))]
        private static void SystemInit()
        {
            Initialized = true;
            MSULog.Info($"Initializing Item Module...");

            On.RoR2.CharacterBody.RecalculateStats += OnRecalculateStats;
            R2API.RecalculateStatsAPI.GetStatCoefficients += OnGetStatCoefficients;

            MoonstormItems = new ReadOnlyDictionary<ItemDef, ItemBase>(items);
            items = null;

            OnDictionaryCreated?.Invoke(MoonstormItems);
        }

        #region Items
        protected virtual IEnumerable<ItemBase> GetItemBases()
        {
            if(Initialized)
            {
                ThrowModuleInitialized($"Retrieve ItemBase list", typeof(ItemModuleBase));
                return null;
            }

            MSULog.Debug($"Getting the Items found inside {GetType().Assembly}");
            return GetContentClasses<ItemBase>();
        }

        protected void AddItem(ItemBase item, Dictionary<ItemDef, ItemBase> dictionary)
        {
            if(Initialized)
            {
                ThrowModuleInitialized($"Add ItemBase to ContentPack", typeof(ItemModuleBase));
                return;
            }

            if (InitializeContent(item) && dictionary != null)
                AddSafelyToDict(ref dictionary, item.ItemDef, item);

            MSULog.Debug($"Item {item.ItemDef} addeed to {SerializableContentPack.name}");
        }

        protected override bool InitializeContent(ItemBase contentClass)
        {
            if(AddSafely(ref SerializableContentPack.itemDefs, contentClass.ItemDef))
            {
                contentClass.Initialize();

                AddSafelyToDict(ref items, contentClass.ItemDef, contentClass);
                return true;
            }
            return false;
        }
        #endregion

        #region Hooks
        private static void OnGetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            throw new NotImplementedException();
        }

        private static void OnRecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
