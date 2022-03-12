using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Moonstorm
{
    public abstract class EquipmentModuleBase : ContentModule<EquipmentBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<EquipmentDef, EquipmentBase> NonEliteMoonstormEquipments
        {
            get
            {
                if(!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve dictionary {NonEliteMoonstormEquipments}", typeof(EquipmentModuleBase));
                    return null;
                }
                return NonEliteMoonstormEquipments;
            }
            private set
            {
                NonEliteMoonstormEquipments = value;
            }
        }
        internal static Dictionary<EquipmentDef, EquipmentBase> nonEliteEquip = new Dictionary<EquipmentDef, EquipmentBase>();

        public static ReadOnlyDictionary<EquipmentDef, EliteEquipmentBase> EliteMoonstormEquipments
        {
            get
            {
                if(!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve dictionary {nameof(EliteMoonstormEquipments)}", typeof(EquipmentModuleBase));
                    return null;
                }
                return EliteMoonstormEquipments;
            }
            private set
            {
                EliteMoonstormEquipments = value;
            }
        }
        internal static Dictionary<EquipmentDef, EliteEquipmentBase> eliteEquip = new Dictionary<EquipmentDef, EliteEquipmentBase>();

        public static ReadOnlyDictionary<EquipmentDef, EquipmentBase> AllMoonstormEquipments
        {
            get
            {
                if(!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve dictionary {nameof(AllMoonstormEquipments)}", typeof(EquipmentModuleBase));
                    return null;
                }

                if(allMoonstormEquipments == null)
                {
                    var mergedDictionary = NonEliteMoonstormEquipments.Union(EliteMoonstormEquipments.ToDictionary(k => k.Key, v => (EquipmentBase)v.Value))
                                                                      .ToDictionary(k => k.Key, v => v.Value);
                    allMoonstormEquipments = new ReadOnlyDictionary<EquipmentDef, EquipmentBase>(mergedDictionary);
                }
                return allMoonstormEquipments;
            }
        }
        private static ReadOnlyDictionary<EquipmentDef, EquipmentBase> allMoonstormEquipments;
        public static Action<ReadOnlyDictionary<EquipmentDef, EquipmentBase>, ReadOnlyDictionary<EquipmentDef, EliteEquipmentBase>> OnDictionariesCreated;

        public static EquipmentDef[] LoadedNonEliteEquipmentDefs { get => NonEliteMoonstormEquipments.Keys.ToArray(); }
        public static EquipmentDef[] EliteEquipmentDefs { get => EliteMoonstormEquipments.Keys.ToArray(); }
        public static EquipmentDef[] AllEquipmentDefs { get => AllMoonstormEquipments.Keys.ToArray(); }

        public static bool Initialized { get; private set; } = false;
        #endregion

        [SystemInitializer(typeof(EquipmentCatalog))]
        private static void SystemInit()
        {
            Initialized = true;
            MSULog.Info($"Initializing Equipment Module...");

            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformAction;

            EliteMoonstormEquipments = new ReadOnlyDictionary<EquipmentDef, EliteEquipmentBase>(eliteEquip);
            eliteEquip = null;

            NonEliteMoonstormEquipments = new ReadOnlyDictionary<EquipmentDef, EquipmentBase>(nonEliteEquip);
            nonEliteEquip = null;

            _ = AllMoonstormEquipments;
            OnDictionariesCreated?.Invoke(NonEliteMoonstormEquipments, EliteMoonstormEquipments);
        }

        #region Equipments
        protected virtual IEnumerable<EquipmentBase> GetEquipmentBases()
        {
            if(Initialized)
            {
                ThrowModuleInitialized($"Retrieve EquipmentBase list", typeof(EquipmentModuleBase));
                return null;
            }

            MSULog.Debug($"Getting the Equipments found inside {GetType().Assembly}");
            return GetContentClasses<EquipmentBase>(typeof(EliteEquipmentBase));
        }

        protected void AddEquipment(EquipmentBase equip, Dictionary<EquipmentDef, EquipmentBase> dictionary)
        {
            if (Initialized)
            {
                ThrowModuleInitialized($"Add EquipmentBase to ContentPack", typeof(EquipmentModuleBase));
                return;
            }
            if(equip is EliteEquipmentBase)
            {
                throw new InvalidOperationException($"Cannot Add equipment {equip.EquipmentDef} because it's declaring type {equip.GetType()} inherits from EliteEquipmentBase!");
            }

            if (InitializeContent(equip) && dictionary != null)
                AddSafelyToDict(ref dictionary, equip.EquipmentDef, equip);

            MSULog.Debug($"Equipment {equip.EquipmentDef} added to {SerializableContentPack.name}");
        }

        #region EliteEquipments
        protected virtual IEnumerable<EliteEquipmentBase> GetEliteEquipmentBases()
        {
            if (Initialized)
            {
                ThrowModuleInitialized($"Retrieve EliteEquipmentBase list", typeof(EquipmentModuleBase));
                return null;
            }

            MSULog.Debug($"Getting the Elite Equipments found inside {GetType().Assembly}");
            return GetContentClasses<EliteEquipmentBase>();
        }

        protected void AddEliteEquipment(EliteEquipmentBase eliteEqp, Dictionary<EquipmentDef, EliteEquipmentBase> dictionary)
        {
            if (Initialized)
            {
                ThrowModuleInitialized($"Add EliteEquipmentBase to ContentPack", typeof(EquipmentModuleBase));
                return;
            }
            if (eliteEqp is EliteEquipmentBase)
            {
                throw new InvalidOperationException($"Cannot Add Elite Equipment {eliteEqp.EquipmentDef} because it's declaring type {eliteEqp.GetType()} inherits from EquipmentBase!");
            }

            if (InitializeContent(eliteEqp) && dictionary != null)
                AddSafelyToDict(ref dictionary, eliteEqp.EquipmentDef, eliteEqp);

            MSULog.Debug($"Equipment {eliteEqp.EquipmentDef} Added to {SerializableContentPack.name}");
        }
        #endregion

        protected override bool InitializeContent(EquipmentBase contentClass)
        {
            if(AddSafely(ref SerializableContentPack.equipmentDefs, contentClass.EquipmentDef))
            {

                if(contentClass is EliteEquipmentBase eeb)
                {
                    AddSafelyToDict(ref eliteEquip, eeb.EquipmentDef, eeb);
                    contentClass.Initialize();
                    return true;
                }
                else if(contentClass is EquipmentBase eb)
                {
                    AddSafelyToDict(ref nonEliteEquip, eb.EquipmentDef, eb);
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Hooks
        private static bool PerformAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if(!NetworkServer.active)
            {
                MSULog.Warning($"[Server] function 'System.Boolean RoR2.EquipmentSlot::PerformEquipmentAction(RoR2.EquipmentDef)' called on client");
                return false;
            }

            EquipmentBase equip;
            if(AllMoonstormEquipments.TryGetValue(equipmentDef, out equip))
            {
                var body = self.characterBody;
                return equip.FireAction(self);
            }
            return orig(self, equipmentDef);
        }
        #endregion
    }
}
