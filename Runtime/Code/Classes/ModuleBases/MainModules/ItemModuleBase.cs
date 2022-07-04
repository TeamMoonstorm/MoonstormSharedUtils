using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm
{
    /// <summary>
    /// The <see cref="ItemModuleBase"/> is a <see cref="ContentModule{T}"/> that handles the <see cref="ItemBase"/> class
    /// <para><see cref="ItemModuleBase"/>'s main job is to handle the proper addition of ItemDefs from <see cref="ItemBase"/> inheriting classes</para>
    /// <para>Inherit from this module if you want to load and manage ItemDefs with <see cref="ItemBase"/> systems</para>
    /// </summary>
    public abstract class ItemModuleBase : ContentModule<ItemBase>
    {
        #region Properties and Fields
        /// <summary>
        /// A ReadOnlyDictionary that can be used for loading a specific <see cref="ItemBase"/> by giving it's tied <see cref="ItemDef"/>
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionaryCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<ItemDef, ItemBase> MoonstormItems { get; private set; }
        internal static Dictionary<ItemDef, ItemBase> items = new Dictionary<ItemDef, ItemBase>();

        /// <summary>
        /// Loads all the <see cref="ItemDef"/> from the <see cref="MoonstormItems"/> dictionary
        /// </summary>
        public static ItemDef[] LoadedItemDefs { get => MoonstormItems.Keys.ToArray(); }
        /// <summary>
        /// An action that gets invoked when the <see cref="MoonstormItems"/> dictionary has been populated
        /// </summary>
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
        /// <summary>
        /// <inheritdoc cref="ModuleBase{T}.GetContentClasses{T}(Type)"/>
        /// <para>T in this case is <see cref="ItemBase"/></para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's <see cref="ItemBase"/></returns>
        protected virtual IEnumerable<ItemBase> GetItemBases()
        {
            MSULog.Debug($"Getting the Items found inside {GetType().Assembly}");
            return GetContentClasses<ItemBase>();
        }

        /// <summary>
        /// Adds an ItemBase's ItemDef to your mod's ContentPack
        /// </summary>
        /// <param name="item">The ItemBase to add</param>
        /// <param name="dictionary">Optional, a dictionary to add your initialized ItemBase and ItemDef</param>
        protected void AddItem(ItemBase item, Dictionary<ItemDef, ItemBase> dictionary = null)
        {
            InitializeContent(item);
            dictionary?.Add(item.ItemDef, item);
            MSULog.Debug($"Item {item.ItemDef} Initialized and Ensured in {SerializableContentPack.name}");
        }

        /// <summary>
        /// Adds the <see cref="ItemDef"/> of <paramref name="contentClass"/> to your mod's SerializableContentPack
        /// <para>Once added, it'll call <see cref="ContentBase.Initialize"/></para>
        /// </summary>
        /// <param name="contentClass">The content class being initialized</param>
        protected override void InitializeContent(ItemBase contentClass)
        {
            AddSafely(ref SerializableContentPack.itemDefs, contentClass.ItemDef);
            contentClass.Initialize();
            items[contentClass.ItemDef] = contentClass;
        }
        #endregion
    }
}
