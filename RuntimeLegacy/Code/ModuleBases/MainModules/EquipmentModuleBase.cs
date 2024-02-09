using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.Networking;

namespace Moonstorm
{
    public abstract class EquipmentModuleBase : ContentModule<EquipmentBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<EquipmentDef, EquipmentBase> NonEliteMoonstormEquipments { get; private set; }
        internal static Dictionary<EquipmentDef, EquipmentBase> nonEliteEquip = new Dictionary<EquipmentDef, EquipmentBase>();

        public static ReadOnlyDictionary<EquipmentDef, EliteEquipmentBase> EliteMoonstormEquipments { get; private set; }
        internal static Dictionary<EquipmentDef, EliteEquipmentBase> eliteEquip = new Dictionary<EquipmentDef, EliteEquipmentBase>();

        public static ReadOnlyDictionary<EquipmentDef, EquipmentBase> AllMoonstormEquipments
        {
            get
            {
                return allMoonstormEquipments;
            }
        }
        private static ReadOnlyDictionary<EquipmentDef, EquipmentBase> allMoonstormEquipments;

        public static EquipmentDef[] LoadedNonEliteEquipmentDefs { get => NonEliteMoonstormEquipments.Keys.ToArray(); }

        public static EquipmentDef[] EliteEquipmentDefs { get => EliteMoonstormEquipments.Keys.ToArray(); }

        public static EquipmentDef[] AllEquipmentDefs { get => AllMoonstormEquipments.Keys.ToArray(); }

        public static ResourceAvailability moduleAvailability;
        #endregion

        [SystemInitializer(typeof(EquipmentCatalog))]
        private static void SystemInit()
        {
            MSULog.Info($"Initializing Equipment Module...");

            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformAction;

            EliteMoonstormEquipments = new ReadOnlyDictionary<EquipmentDef, EliteEquipmentBase>(eliteEquip);
            eliteEquip = null;

            NonEliteMoonstormEquipments = new ReadOnlyDictionary<EquipmentDef, EquipmentBase>(nonEliteEquip);
            nonEliteEquip = null;

            var mergedDictionary = NonEliteMoonstormEquipments.Union(EliteMoonstormEquipments.ToDictionary(k => k.Key, v => (EquipmentBase)v.Value))
                                                              .ToDictionary(k => k.Key, v => v.Value);
            allMoonstormEquipments = new ReadOnlyDictionary<EquipmentDef, EquipmentBase>(mergedDictionary);

            moduleAvailability.MakeAvailable();
        }

        #region Equipments

        protected virtual IEnumerable<EquipmentBase> GetEquipmentBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Equipments found inside {GetType().Assembly}");
#endif
            return GetContentClasses<EquipmentBase>(typeof(EliteEquipmentBase));
        }

        protected void AddEquipment(EquipmentBase equip, Dictionary<EquipmentDef, EquipmentBase> dictionary = null)
        {
            InitializeContent(equip);
            dictionary?.Add(equip.EquipmentDef, equip);
        }

        #region EliteEquipments

        protected virtual IEnumerable<EliteEquipmentBase> GetEliteEquipmentBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Elite Equipments found inside {GetType().Assembly}");
#endif
            return GetContentClasses<EliteEquipmentBase>();
        }

        protected void AddEliteEquipment(EliteEquipmentBase eliteEqp, Dictionary<EquipmentDef, EliteEquipmentBase> dictionary = null)
        {
            InitializeContent(eliteEqp);
            dictionary?.Add(eliteEqp.EquipmentDef, eliteEqp);
        }
        #endregion

        protected override void InitializeContent(EquipmentBase contentClass)
        {
            AddSafely(ref SerializableContentPack.equipmentDefs, contentClass.EquipmentDef);

            if (contentClass is EliteEquipmentBase eeb)
            {
                eliteEquip[eeb.EquipmentDef] = eeb;
#if DEBUG
                MSULog.Debug($"EliteEquipmentBase {eeb}'s equipment def ensured in {SerializableContentPack.name}" +
                    $"\nBe sure to create an EliteModule to finalize the initialization of the elite equipment base!");
#endif
                return;
            }

            nonEliteEquip[contentClass.EquipmentDef] = contentClass;
            contentClass.Initialize();
#if DEBUG
            MSULog.Debug($"Equipment {contentClass} Initialized and ensured in {SerializableContentPack.name}");
#endif
        }
        #endregion

        #region Hooks
        private static bool PerformAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (!NetworkServer.active)
            {
                MSULog.Warning($"[Server] function 'System.Boolean RoR2.EquipmentSlot::PerformEquipmentAction(RoR2.EquipmentDef)' called on client");
                return false;
            }

            EquipmentBase equip;
            if (AllMoonstormEquipments.TryGetValue(equipmentDef, out equip))
            {
                var body = self.characterBody;
                return equip.FireAction(self);
            }
            return orig(self, equipmentDef);
        }
        #endregion
    }
}
