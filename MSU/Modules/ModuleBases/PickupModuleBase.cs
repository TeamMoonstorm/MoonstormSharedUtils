using Moonstorm.Components;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm
{
    /// <summary>
    /// A Module Base for managing Items and Equipments.
    /// </summary>
    public abstract class PickupModuleBase : ModuleBase
    {
        /// <summary>
        /// Dictionary of all the items loaded by Moonstorm Shared Utils
        /// </summary>
        public static readonly Dictionary<ItemDef, ItemBase> MoonstormItems = new Dictionary<ItemDef, ItemBase>();

        /// <summary>
        /// Returns all the equipments loaded by Moonstorm Shared Utils
        /// <para>Includes elite equipments.</para>
        /// </summary>
        public static Dictionary<EquipmentDef, EquipmentBase> MoonstormEquipments
        {
            get
            {
                return MoonstormNonEliteEquipments
                                                  .Union(MoonstormEliteEquipments.ToDictionary(k => k.Key, v => (EquipmentBase)v.Value))
                                                  .ToDictionary(x => x.Key, y => y.Value);
            }
        }

        //This dictionary is used in the DynamicDescription system for getting the property of either ItemDef or EquipmentDef
        internal static Dictionary<UnityEngine.Object, Type> AllPickups
        {
            get
            {
                Dictionary<UnityEngine.Object, Type> toReturn = new Dictionary<UnityEngine.Object, Type>();
                return toReturn.Union(MoonstormItems.ToDictionary(k => (UnityEngine.Object)k.Key, v => v.Value.GetType()))
                    .Union(MoonstormNonEliteEquipments.ToDictionary(k => (UnityEngine.Object)k.Key, v => v.Value.GetType()))
                    .Union(MoonstormEquipments.ToDictionary(k => (UnityEngine.Object)k.Key, v => v.Value.GetType()))
                    .ToDictionary(x => x.Key, y => y.Value);
            }
        }

        /// <summary>
        /// Dictionary of all the normal equipments loaded by Moonstorm Shared Utils
        /// </summary>
        public static readonly Dictionary<EquipmentDef, EquipmentBase> MoonstormNonEliteEquipments = new Dictionary<EquipmentDef, EquipmentBase>();

        /// <summary>
        /// Dictionary of all the Elite Equipments loaded by Moonstorm Shared Utils
        /// </summary>
        public static readonly Dictionary<EquipmentDef, EliteEquipmentBase> MoonstormEliteEquipments = new Dictionary<EquipmentDef, EliteEquipmentBase>();

        /// <summary>
        /// Returns all the ItemDefs inside MoonstormItems
        /// </summary>
        public static ItemDef[] LoadedItemDefs
        {
            get
            {
                return MoonstormItems.Keys.ToArray();
            }
        }

        /// <summary>
        /// Returns all the EquipmentDefs inside MoonstormEquipments and MoonstormEliteEquipments.
        /// </summary>
        public static EquipmentDef[] LoadedEquipDefs
        {
            get
            {
                var toReturn = new List<EquipmentDef>();
                MoonstormEquipments.Keys.ToList().ForEach(equip => toReturn.Add(equip));
                MoonstormEliteEquipments.Keys.ToList().ForEach(eliteEquip => toReturn.Add(eliteEquip));
                return toReturn.ToArray(); ;
            }
        }

        [SystemInitializer(typeof(PickupCatalog))]
        private static void HookInit()
        {
            MSULog.LogI("Subscribing to delegates related to Items and Equipments.");
            On.RoR2.CharacterBody.RecalculateStats += OnRecalculateStats;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += FireMoonstormEqp;
            CharacterBody.onBodyStartGlobal += AddManager;
        }

        #region Items

        /// <summary>
        /// Finds all the ItemBase inherited classes in your assembly and creates instances for each found.
        /// <para>Ignores classes with the "DisabledContent" attribute</para>
        /// </summary>
        /// <returns>An IEnumerable of all your Assembly's ItemBases</returns>
        public virtual IEnumerable<ItemBase> InitializeItems()
        {
            MSULog.LogD($"Getting the Items found inside {GetType().Assembly}...");
            return GetType().Assembly.GetTypes()
                           .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)))
                           .Where(type => !type.GetCustomAttributes(true)
                                    .Select(obj => obj.GetType())
                                    .Contains(typeof(DisabledContent)))
                           .Select(itemType => (ItemBase)Activator.CreateInstance(itemType));
        }

        /// <summary>
        /// Initializes and Adds an Item
        /// </summary>
        /// <param name="item">The ItemBase class</param>
        /// <param name="contentPack">The content pack of your mod</param>
        /// <param name="itemDictionary">Optional, a Dictionary for getting an ItemBase by feeding it the corresponding ItemDef.</param>
        public void AddItem(ItemBase item, SerializableContentPack contentPack, Dictionary<ItemDef, ItemBase> itemDictionary = null)
        {
            item.Initialize();
            HG.ArrayUtils.ArrayAppend(ref ContentPack.itemDefs, item.ItemDef);
            MoonstormItems.Add(item.ItemDef, item);
            if (itemDictionary != null)
                itemDictionary.Add(item.ItemDef, item);
            MSULog.LogD($"Item {item.ItemDef} added to {contentPack.name}");
        }
        #endregion

        #region Equipments

        /// <summary>
        /// Finds all the EquipmentBase inherited classes in your assembly and creates an instance for each found.
        /// <para>Ignores classes with the DisabledContent Attribute</para>
        /// </summary>
        /// <returns>An IEnumerable of all your Assembly's EquipmentBases</returns>
        public virtual IEnumerable<EquipmentBase> InitializeEquipments()
        {
            MSULog.LogD($"Getting the Equipments found inside {GetType().Assembly}...");
            return GetType().Assembly.GetTypes()
                           .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)) && !type.IsSubclassOf(typeof(EliteEquipmentBase)))
                           .Where(type => !type.GetCustomAttributes(true)
                                    .Select(obj => obj.GetType())
                                    .Contains(typeof(DisabledContent)))
                           .Select(eqpType => (EquipmentBase)Activator.CreateInstance(eqpType));
        }

        /// <summary>
        /// Initializes and Adds an Equipment
        /// </summary>
        /// <param name="equip">The EquipmentBase class</param>
        /// <param name="contentPack">Your Mod's content pack</param>
        /// <param name="equipDictionary">Optional, a dictionary for getting an EquipmentBase by feeding it the corresponding EquipmentDef</param>
        public void AddEquipment(EquipmentBase equip, SerializableContentPack contentPack, Dictionary<EquipmentDef, EquipmentBase> equipDictionary = null)
        {
            equip.Initialize();
            HG.ArrayUtils.ArrayAppend(ref contentPack.equipmentDefs, equip.EquipmentDef);
            MoonstormNonEliteEquipments.Add(equip.EquipmentDef, equip);
            if (equipDictionary != null)
                equipDictionary.Add(equip.EquipmentDef, equip);
            MSULog.LogD($"Equipment {equip.EquipmentDef} added to {contentPack.name}");
        }
        #endregion

        #region Elite Equipments
        /// <summary>
        /// Finds all the EliteEquipmentBase inherited classes in your assembly and creates instances for each found.
        /// <para>Ignores classes with the DisabledContent attribute</para>
        /// </summary>
        /// <returns>An IEnumerable of all your Assembly's EliteEquipmentBases</returns>
        public virtual IEnumerable<EliteEquipmentBase> InitializeEliteEquipments()
        {
            MSULog.LogD($"Getting the Elite Equipments found inside {GetType().Assembly}...");
            return GetType().Assembly.GetTypes()
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EliteEquipmentBase)))
                .Where(type => !type.GetCustomAttributes(true)
                                    .Select(obj => obj.GetType())
                                    .Contains(typeof(DisabledContent)))
                .Select(eqpEliteType => (EliteEquipmentBase)Activator.CreateInstance(eqpEliteType));
        }

        /// <summary>
        /// Adds an elite equipment to the moonstorm elite equipments dictionary.
        /// <para>Keep in mind that this does not completely initialize the elite.</para>
        /// <para>The rest is done in the EliteModuleBase</para>
        /// </summary>
        /// <param name="eliteEquip">The EliteEquipmentBase class</param>
        /// <param name="eliteEquipmentDictionary">Optional, a dictionary for getting an EliteEquipmentDef by feeding it the corresponding EquipmentDef</param>
        public void AddEliteEquipment(EliteEquipmentBase eliteEquip, Dictionary<EquipmentDef, EliteEquipmentBase> eliteEquipmentDictionary = null)
        {
            MoonstormEliteEquipments.Add(eliteEquip.EquipmentDef, eliteEquip);
            if(eliteEquipmentDictionary != null)
                eliteEquipmentDictionary.Add(eliteEquip.EquipmentDef, eliteEquip);
            MSULog.LogD($"Elite Equipment {eliteEquip.EquipmentDef} added to the Elite Equipment Dictionary.");
        }
        #endregion

        #region Hooks
        //Only time we should do this
        private static void OnRecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            var manager = self.GetComponent<MoonstormItemManager>();
            manager?.RunStatRecalculationsStart();
            orig(self);
            manager?.RunStatRecalculationsEnd();
        }

        // a hook is fine here because we need to check every single time no matter the circumstance
        private static bool FireMoonstormEqp(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Boolean RoR2.EquipmentSlot::PerformEquipmentAction(RoR2.EquipmentDef)' called on client");
                return false;
            }
            EquipmentBase equipment;
            if (MoonstormEquipments.TryGetValue(equipmentDef, out equipment))
            {
                var body = self.characterBody;
                return equipment.FireAction(self);
            }
            return orig(self, equipmentDef);
        }

        private static void AddManager(CharacterBody body)
        {
            if (!body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless) && body.master.inventory)
            {
                var itemManager = body.gameObject.AddComponent<MoonstormItemManager>();
                itemManager.CheckForItems();
                itemManager.CheckForBuffs();
            }
        }
        #endregion
    }
}
