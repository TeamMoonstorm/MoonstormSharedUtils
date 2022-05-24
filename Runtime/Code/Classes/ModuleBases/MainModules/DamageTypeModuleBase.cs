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
        public static Action<ReadOnlyDictionary<ModdedDamageType, DamageTypeBase>> OnDictionaryCreated;
        #endregion

        [SystemInitializer]
        private static void SystemInit()
        {
            MSULog.Info("Initializing DamageType Module...");

            MoonstormDamageTypes = new ReadOnlyDictionary<ModdedDamageType, DamageTypeBase>(damageTypes);
            damageTypes = null;

            OnDictionaryCreated?.Invoke(MoonstormDamageTypes);
        }


        #region Damage Types
        protected virtual IEnumerable<DamageTypeBase> GetDamageTypeBases()
        {
            MSULog.Debug($"Getting the Damage Types found inside {GetType().Assembly}...");
            return GetContentClasses<DamageTypeBase>();
        }

        protected void AddDamageType(DamageTypeBase damageType, Dictionary<ModdedDamageType, DamageTypeBase> damageTypeDictionary = null)
        {
            InitializeContent(damageType);
            damageTypeDictionary?.Add(damageType.ModdedDamageType, damageType);
            MSULog.Debug($"Damage type {damageType} added to the game");
        }

        protected override void InitializeContent(DamageTypeBase contentClass)
        {
            contentClass.Initialize();
            contentClass.Delegates();
            contentClass.SetDamageType(ReserveDamageType());

            damageTypes[contentClass.ModdedDamageType] = contentClass;
        }
        #endregion
    }
}
