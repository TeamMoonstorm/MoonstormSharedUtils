/*using Moonstorm.Components;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm
{
    /*public abstract class PickupModuleBase : ContentModule
    {
        public static readonly Dictionary<ItemDef, ItemBase> MoonstormItems = new Dictionary<ItemDef, ItemBase>();

        public static Dictionary<EquipmentDef, EquipmentBase> MoonstormEquipments
        {
            get
            {
                return MoonstormNonEliteEquipments
                                                  .Union(MoonstormEliteEquipments.ToDictionary(k => k.Key, v => (EquipmentBase)v.Value))
                                                  .ToDictionary(x => x.Key, y => y.Value);
            }
        }

        public static readonly Dictionary<EquipmentDef, EquipmentBase> MoonstormNonEliteEquipments = new Dictionary<EquipmentDef, EquipmentBase>();
        public static readonly Dictionary<EquipmentDef, EliteEquipmentBase> MoonstormEliteEquipments = new Dictionary<EquipmentDef, EliteEquipmentBase>();
        internal static readonly Dictionary<EquipmentDef, EliteEquipmentBase> nonInitializedEliteEquipments = new Dictionary<EquipmentDef, EliteEquipmentBase>();
        public static ItemDef[] LoadedItemDefs { get => MoonstormItems.Keys.ToArray(); }
        public static EquipmentDef[] LoadedEquipDefs { get => MoonstormEquipments.Keys.ToList().Union(MoonstormEliteEquipments.Keys.ToList()).ToArray(); }
        public static event Action<CharacterBody, MoonstormItemManager> onManagerAdded;

        [SystemInitializer(typeof(PickupCatalog))]
        private static void HookInit()
        {
            MSULog.Info("Subscribing to delegates related to Items and Equipments.");

            On.RoR2.CharacterBody.RecalculateStats += OnRecalculateStats;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += FireMoonstormEqp;
            CharacterBody.onBodyStartGlobal += AddManager;

            R2API.RecalculateStatsAPI.GetStatCoefficients += OnGetStatCoefficients;
        }


        #region Items
        public virtual IEnumerable<ItemBase> InitializeItems()
        {
            MSULog.Debug($"Getting the Items found inside {GetType().Assembly}...");
            return GetContentClasses<ItemBase>();
        }
        public void AddItem(ItemBase item, Dictionary<ItemDef, ItemBase> itemDictionary = null)
        {
            item.Initialize();
            HG.ArrayUtils.ArrayAppend(ref SerializableContentPack.itemDefs, item.ItemDef);
            MoonstormItems.Add(item.ItemDef, item);
            if (itemDictionary != null)
                itemDictionary.Add(item.ItemDef, item);
            MSULog.Debug($"Item {item.ItemDef} added to {SerializableContentPack.name}");
        }
        #endregion

        #region Equipments
        public virtual IEnumerable<EquipmentBase> InitializeEquipments()
        {
            MSULog.Debug($"Getting the Equipments found inside {GetType().Assembly}...");
            return GetContentClasses<EquipmentBase>(typeof(EliteEquipmentBase));
        }
        public void AddEquipment(EquipmentBase equip, SerializableContentPack contentPack, Dictionary<EquipmentDef, EquipmentBase> equipDictionary = null)
        {
            equip.Initialize();
            HG.ArrayUtils.ArrayAppend(ref contentPack.equipmentDefs, equip.EquipmentDef);
            MoonstormNonEliteEquipments.Add(equip.EquipmentDef, equip);
            if (equipDictionary != null)
                equipDictionary.Add(equip.EquipmentDef, equip);
            MSULog.Debug($"Equipment {equip.EquipmentDef} added to {contentPack.name}");
        }
        #endregion

        #region Elite Equipments
        public virtual IEnumerable<EliteEquipmentBase> InitializeEliteEquipments()
        {
            MSULog.Debug($"Getting the Elite Equipments found inside {GetType().Assembly}...");
            return GetContentClasses<EliteEquipmentBase>();
        }

        public void AddEliteEquipment(EliteEquipmentBase eliteEquip)
        {
            nonInitializedEliteEquipments.Add(eliteEquip.EquipmentDef, eliteEquip);
            MSULog.Debug($"Added {typeof(EliteEquipmentBase).Name} to the Non Initialized Elite Equipments Dictionary.");
        }
        #endregion

        #region Hooks
        private static void OnGetStatCoefficients(CharacterBody body, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            var manager = body.GetComponent<MoonstormItemManager>();
            if (manager)
            {
                manager.RunStatHookEventModifiers(args);
            }
        }

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
                PickupModuleBase.onManagerAdded?.Invoke(body, itemManager);
            }
        }
        #endregion
    }
}*/
