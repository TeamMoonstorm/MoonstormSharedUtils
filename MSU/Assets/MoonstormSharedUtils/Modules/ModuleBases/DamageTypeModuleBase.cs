using RoR2;
using System.Collections.Generic;
using System.Linq;
using static R2API.DamageAPI;

namespace Moonstorm
{
    /// <summary>
    /// A Module Base for managing DamageTypes
    /// </summary>
    public abstract class DamageTypeModuleBase : ModuleBase
    {
        /// <summary>
        /// Dictionary of all the damage types loaded by Moonstorm Shared Utils
        /// </summary>
        public static Dictionary<ModdedDamageType, DamageTypeBase> MoonstormDamageTypes = new Dictionary<ModdedDamageType, DamageTypeBase>();

        /// <summary>
        /// Returns all the DamageTypes loaded by MoonstormSharedUtils
        /// </summary>
        public static ModdedDamageType[] ModdedDamageTypes { get => MoonstormDamageTypes.Keys.ToArray(); }

        [SystemInitializer]
        private static void HookInit()
        {
            MSULog.LogI("Subscribing to delegates related to damage types.");
        }


        #region Damage Types
        /// <summary>
        /// Finds all the DamageTypeBase inheriting classes in your assembly and creates instances for each found.
        /// <para>Ignores classes with the "DisabledContent" attribute</para>
        /// </summary>
        /// <returns>An IEnumerable of all your Assembly's DamageTypeBases</returns>
        public virtual IEnumerable<DamageTypeBase> InitializeDamageTypes()
        {
            return GetContentClasses<DamageTypeBase>();
        }

        /// <summary>
        /// Initializes a damage type.
        /// </summary>
        /// <param name="damageType">The DamageTypeBase class</param>
        /// <param name="damageTypeDictionary">Optional, a Dictionary for getting a DamageTypeBase by feeding it the corresponding ModdedDamageType</param>
        public void AddDamageType(DamageTypeBase damageType, Dictionary<ModdedDamageType, DamageTypeBase> damageTypeDictionary = null)
        {
            damageType.Initialize();
            var dType = damageType.GetDamageType();

            MoonstormDamageTypes.Add(dType, damageType);
            if (damageTypeDictionary != null)
                damageTypeDictionary.Add(dType, damageType);
            MSULog.LogD($"Damage type {damageType} added");
        }
        #endregion
    }
}
