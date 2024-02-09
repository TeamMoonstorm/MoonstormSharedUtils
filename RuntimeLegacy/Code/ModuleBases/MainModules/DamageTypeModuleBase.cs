using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static R2API.DamageAPI;

namespace Moonstorm
{
    public abstract class DamageTypeModuleBase : ModuleBase<DamageTypeBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<ModdedDamageType, DamageTypeBase> MoonstormDamageTypes { get; private set; }
        internal static Dictionary<ModdedDamageType, DamageTypeBase> damageTypes = new Dictionary<ModdedDamageType, DamageTypeBase>();

        public static ModdedDamageType[] ModdedDamageTypes { get => MoonstormDamageTypes.Keys.ToArray(); }

        public static ResourceAvailability moduleAvailability;
        #endregion

        [SystemInitializer]
        private static void SystemInit()
        {
            MSULog.Info("Initializing DamageType Module...");

            MoonstormDamageTypes = new ReadOnlyDictionary<ModdedDamageType, DamageTypeBase>(damageTypes);
            damageTypes = null;

            moduleAvailability.MakeAvailable();
        }


        #region Damage Types
        protected virtual IEnumerable<DamageTypeBase> GetDamageTypeBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Damage Types found inside {GetType().Assembly}...");
#endif
            return GetContentClasses<DamageTypeBase>();
        }

        protected void AddDamageType(DamageTypeBase damageType, Dictionary<ModdedDamageType, DamageTypeBase> damageTypeDictionary = null)
        {
            InitializeContent(damageType);
            damageTypeDictionary?.Add(damageType.ModdedDamageType, damageType);
#if DEBUG
            MSULog.Debug($"Damage type {damageType} added to the game");
#endif
        }

        protected override void InitializeContent(DamageTypeBase contentClass)
        {
            contentClass.SetDamageType(ReserveDamageType());
            contentClass.Initialize();
            contentClass.Delegates();

            damageTypes[contentClass.ModdedDamageType] = contentClass;
        }
        #endregion
    }
}
