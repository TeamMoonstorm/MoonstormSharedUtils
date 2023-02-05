using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Moonstorm
{
    /// <summary>
    /// The <see cref="ItemTierModuleBase"/> is a <see cref="ContentModule{T}"/> that handles the <see cref="ItemTierBase"/> class
    /// <para><see cref="ItemTierModuleBase"/>'s main job is to handle the proper addition of ItemTiers from <see cref="ItemTierBase"/> inheriting classes</para>
    /// <para>ItemTiers managed by this module also have unique pickup display VFX, alongside a list of the items that use the tier and the available drop list.</para>
    /// <para>Inherit from this module if you want to load and manage ItemTierDefs with <see cref="ItemTierBase"/> systems</para>
    /// </summary>
    public abstract class ItemTierModuleBase : ContentModule<ItemTierBase>
    {
        #region Properties and Fields
        /// <summary>
        /// A ReadOnlyDictioanry that can be used for loading a specific <see cref="ItemTierBase"/> by giving it's tied <see cref="ItemTierDef"/>
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionaryCreated"/> to ensure the dictionary is not empty.</para>
        /// </summary>
        public static ReadOnlyDictionary<ItemTierDef, ItemTierBase> MoonstormItemTiers { get; private set; }
        internal static Dictionary<ItemTierDef, ItemTierBase> itemTiers = new Dictionary<ItemTierDef, ItemTierBase>();

        /// <summary>
        /// Loads all the <see cref="ItemTierDef"/> from the <see cref="MoonstormItemTiers"/> dictionary
        /// </summary>
        public static ItemTierDef[] LoadedItemTierDefs => MoonstormItemTiers.Keys.ToArray();

        /// <summary>
        /// An action that gets invoked when the <see cref="MoonstormItemTiers"/> dictionary has been populated.
        /// </summary>
        [Obsolete("use \"ModuleAvailability.CallWhenAvailable()\" instead")]
        public static event Action<ReadOnlyDictionary<ItemTierDef, ItemTierBase>> OnDictionaryCreated;
        /// <summary>
        /// Call ModuleAvailability.CallWhenAvailable() to run a method after the Module is initialized.
        /// </summary>
        public static ResourceAvailability ModuleAvailability { get; } = default(ResourceAvailability);
        #endregion

        [SystemInitializer(typeof(ItemTierCatalog), typeof(ItemCatalog))]
        private static void SystemInit()
        {
            MSULog.Info($"Initializing Item Tier Module");

            MoonstormItemTiers = new ReadOnlyDictionary<ItemTierDef, ItemTierBase>(itemTiers);
            itemTiers = null;
            BuildItemListForEachItemTier();


            OnDictionaryCreated?.Invoke(MoonstormItemTiers);
            ModuleAvailability.MakeAvailable();
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

        /// <summary>
        /// <inheritdoc cref="ModuleBase{T}.GetContentClasses{T}(Type)"/>
        /// <para>T in this case is <see cref="ItemTierBase"/></para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's <see cref="ItemTierBase"/></returns>
        protected virtual IEnumerable<ItemTierBase> GetItemTierBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Item Tiers fond inside {GetType().Assembly}");
#endif
            return GetContentClasses<ItemTierBase>();
        }

        /// <summary>
        /// Adds an <see cref="ItemTierBase"/>'s ItemTierDef to your mod's ContentPack
        /// </summary>
        /// <param name="itemTier">The ItemTierBase to add</param>
        /// <param name="dictionary">Optional, a dictionary to add your initialized ItemTierBase and ItemTierDef</param>
        protected void AddItemTier(ItemTierBase itemTier, Dictionary<ItemTierDef, ItemTierBase> dictionary = null)
        {
            InitializeContent(itemTier);
            dictionary?.Add(itemTier.ItemTierDef, itemTier);
#if DEBUG
            MSULog.Debug($"Item Tier {itemTier.ItemTierDef} Initialized and Ensured in {SerializableContentPack.name}");
#endif
        }

        /// <summary>
        /// Adds the <see cref="ItemTierDef"/> of <paramref name="contentClass"/> to your mod's SerializablecontentPack
        /// <para>Once added, it'll call <see cref="ContentBase.Initialize"/></para>
        /// <para>It'll also override the ItemTierDef's colorIndex and darkColorIndex, if <see cref="ItemTierBase.ColorIndex"/> and <see cref="ItemTierBase.DarkColorIndex"/> where supplied</para>
        /// </summary>
        /// <param name="contentClass">The content class being initialized</param>
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