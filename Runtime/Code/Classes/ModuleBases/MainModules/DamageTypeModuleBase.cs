using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static R2API.DamageAPI;

namespace Moonstorm
{
    /// <summary>
    /// The <see cref="DamageTypeModuleBase"/> is a <see cref="ModuleBase{T}"/> that handles the <see cref="DamageTypeBase"/> class
    /// <para><see cref="DamageTypeModuleBase"/>'s main job is to handle the proper addition of a <see cref="ModdedDamageType"/> and implementation with the <see cref="DamageTypeBase"/> inheriting classes using R2API's <see cref="R2API.DamageAPI"/></para>
    /// <para>Inherit from this module if you want to load and manage DamageTypes with <see cref="DamageTypeBase"/> systems</para>
    /// </summary>
    public abstract class DamageTypeModuleBase : ModuleBase<DamageTypeBase>
    {
        #region Properties and Fields
        /// <summary>
        /// A ReadOnlyDictionary that can be used for loading a specific <see cref="DamageTypeBase"/> by giving it's tied <see cref="ModdedDamageType"/>
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionaryCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<ModdedDamageType, DamageTypeBase> MoonstormDamageTypes { get; private set; }
        internal static Dictionary<ModdedDamageType, DamageTypeBase> damageTypes = new Dictionary<ModdedDamageType, DamageTypeBase>();

        /// <summary>
        /// Returns all the ModdedDamageTypes from <see cref="MoonstormDamageTypes"/>
        /// </summary>
        public static ModdedDamageType[] ModdedDamageTypes { get => MoonstormDamageTypes.Keys.ToArray(); }
        /// <summary>
        /// An action that gets invoked when the <see cref="MoonstormDamageTypes" dictionary has been populated
        /// </summary>
        [Obsolete("use \"ModuleAvailability.CallWhenAvailable()\" instead")]
        public static Action<ReadOnlyDictionary<ModdedDamageType, DamageTypeBase>> OnDictionaryCreated;
        /// <summary>
        /// Call ModuleAvailability.CallWhenAvailable() to run a method after the Module is initialized.
        /// </summary>
        public static ResourceAvailability ModuleAvailability { get; } = default(ResourceAvailability);
        #endregion

        [SystemInitializer]
        private static void SystemInit()
        {
            MSULog.Info("Initializing DamageType Module...");

            MoonstormDamageTypes = new ReadOnlyDictionary<ModdedDamageType, DamageTypeBase>(damageTypes);
            damageTypes = null;

            OnDictionaryCreated?.Invoke(MoonstormDamageTypes);
            ModuleAvailability.MakeAvailable();
        }


        #region Damage Types
        /// <summary>
        /// <inheritdoc cref="ModuleBase{T}.GetContentClasses{T}(Type)"/>
        /// <para>T in this case is <see cref="DamageTypeBase"/></para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's <see cref="DamageTypeBase"/></returns>
        protected virtual IEnumerable<DamageTypeBase> GetDamageTypeBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Damage Types found inside {GetType().Assembly}...");
#endif
            return GetContentClasses<DamageTypeBase>();
        }

        /// <summary>
        /// Adds a DamageType to the game
        /// </summary>
        /// <param name="damageType">The DamageType being added</param>
        /// <param name="damageTypeDictionary">Optional, a dictionary to add your initialized ModdedDamageType and DamageTypeBase</param>
        protected void AddDamageType(DamageTypeBase damageType, Dictionary<ModdedDamageType, DamageTypeBase> damageTypeDictionary = null)
        {
            InitializeContent(damageType);
            damageTypeDictionary?.Add(damageType.ModdedDamageType, damageType);
#if DEBUG
            MSULog.Debug($"Damage type {damageType} added to the game");
#endif
        }

        /// <summary>
        /// Reserves and adds the <see cref="ModdedDamageType"/> from <paramref name="contentClass"/> to the game using <see cref="R2API.DamageAPI"/>.
        /// <para>Once added, it'll call the <see cref="ContentBase.Initialize"/>and the <see cref="DamageTypeBase.Delegates"/> methods</para>
        /// </summary>
        /// <param name="contentClass">The content class being initialized</param>
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
