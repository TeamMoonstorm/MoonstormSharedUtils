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
        public static ReadOnlyDictionary<ModdedDamageType, DamageTypeBase> MoonstormDamageTypes
        {
            get
            {
                if(!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve Dictionary {nameof(MoonstormDamageTypes)}", typeof(DamageTypeModuleBase));
                    return null;
                }
                return MoonstormDamageTypes;
            }
            private set
            {
                MoonstormDamageTypes = value;
            }
        }
        internal static Dictionary<ModdedDamageType, DamageTypeBase> damageTypes = new Dictionary<ModdedDamageType, DamageTypeBase>();
        public static Action<ReadOnlyDictionary<ModdedDamageType, DamageTypeBase>> OnDictionaryCreated;

        public static ModdedDamageType[] ModdedDamageTypes { get => MoonstormDamageTypes.Keys.ToArray(); }
        public static bool Initialized { get; private set; }
        #endregion

        [SystemInitializer]
        private static void SystemInit()
        {
            Initialized = true;
            MSULog.Info("Initializing DamageType Module...");

            MoonstormDamageTypes = new ReadOnlyDictionary<ModdedDamageType, DamageTypeBase>(damageTypes);
            damageTypes = null;

            OnDictionaryCreated?.Invoke(MoonstormDamageTypes);
        }


        #region Damage Types
        protected virtual IEnumerable<DamageTypeBase> GetDamageTypeBases()
        {
            if (Initialized)
            {
                ThrowModuleInitialized($"Retrieve DamageTypeBase list", typeof(DamageTypeModuleBase));
                return null;
            }

            return GetContentClasses<DamageTypeBase>();
        }

        protected void AddDamageType(DamageTypeBase dType, Dictionary<ModdedDamageType, DamageTypeBase> damageTypeDictionary = null)
        {
            if (Initialized)
            {
                ThrowModuleInitialized($"Add ModdedDamageType", typeof(DamageTypeModuleBase));
                return;
            }

            if (InitializeContent(dType) && damageTypeDictionary != null)
                AddSafelyToDict(ref damageTypeDictionary, dType.ModdedDamageType, dType);

            MSULog.Debug($"Damage type {dType} added");
        }

        protected override bool InitializeContent(DamageTypeBase contentClass)
        {
            contentClass.Initialize();
            contentClass.Delegates();
            contentClass.ModdedDamageType = ReserveDamageType();

            damageTypes.Add(contentClass.ModdedDamageType, contentClass);
            return true;
        }
        #endregion
    }
}
