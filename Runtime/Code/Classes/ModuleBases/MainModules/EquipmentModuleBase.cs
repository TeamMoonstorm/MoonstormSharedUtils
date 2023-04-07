using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.Networking;

namespace Moonstorm
{
    /// <summary>
    /// The <see cref="EquipmentModuleBase"/> is a <see cref="ContentModule{T}"/> that handles the <see cref="EquipmentBase"/> class
    /// <para><see cref="EquipmentModuleBase"/>'s main job is to handle the proper addition of EquipmentDefs from <see cref="EquipmentBase"/> inheriting classes</para>
    /// <para><see cref="EquipmentModuleBase"/> also manages the <see cref="EliteEquipmentBase"/> class for adding the Affix Equipment, for full initialization of the elite itself, see <see cref="EliteModuleBase"/></para>
    /// <para><see cref="EquipmentBase"/> will automatically handle the use method of the Equipment by running <see cref="EquipmentBase.FireAction(EquipmentSlot)"/></para>
    /// <para>Inherit from this module if you want to load and manage Equipments with <see cref="EquipmentBase"/> systems</para>
    /// </summary>
    public abstract class EquipmentModuleBase : ContentModule<EquipmentBase>
    {
        #region Properties and Fields
        /// <summary>
        /// A ReadOnlyDictionary that can be used for loading a specific <see cref="EquipmentBase"/> by giving it's tied <see cref="EquipmentDef"/>
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionaryCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<EquipmentDef, EquipmentBase> NonEliteMoonstormEquipments { get; private set; }
        internal static Dictionary<EquipmentDef, EquipmentBase> nonEliteEquip = new Dictionary<EquipmentDef, EquipmentBase>();
        /// <summary>
        /// A ReadOnlyDictionary that can be used for loading a specific <see cref="EliteEquipmentBase"/> by giving it's tied <see cref="EquipmentDef"/>
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionaryCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<EquipmentDef, EliteEquipmentBase> EliteMoonstormEquipments { get; private set; }
        internal static Dictionary<EquipmentDef, EliteEquipmentBase> eliteEquip = new Dictionary<EquipmentDef, EliteEquipmentBase>();

        /// <summary>
        /// A ReadOnlyDictionary that can be used to obtain all equipment types that are being handled by MSU's systems.
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionaryCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<EquipmentDef, EquipmentBase> AllMoonstormEquipments
        {
            get
            {
                return allMoonstormEquipments;
            }
        }
        private static ReadOnlyDictionary<EquipmentDef, EquipmentBase> allMoonstormEquipments;

        /// <summary>
        /// Returns all the EquipmentDefs that are not Elite Equipments
        /// </summary>
        public static EquipmentDef[] LoadedNonEliteEquipmentDefs { get => NonEliteMoonstormEquipments.Keys.ToArray(); }
        /// <summary>
        /// Returns all the EquipmentDefs that are Elite Equipments
        /// </summary>
        public static EquipmentDef[] EliteEquipmentDefs { get => EliteMoonstormEquipments.Keys.ToArray(); }
        /// <summary>
        /// Returns all the EquipmentDefs, regardless if theyre from Elites or not
        /// </summary>
        public static EquipmentDef[] AllEquipmentDefs { get => AllMoonstormEquipments.Keys.ToArray(); }
        /// <summary>
        /// An action that gets invoked when the <see cref="NonEliteMoonstormEquipments"/> & <see cref="EliteMoonstormEquipments"/> dictionaries have been populated
        /// </summary>
        [Obsolete("use \"moduleAvailability.CallWhenAvailable()\" instead")]
        public static Action<ReadOnlyDictionary<EquipmentDef, EquipmentBase>, ReadOnlyDictionary<EquipmentDef, EliteEquipmentBase>> OnDictionariesCreated;
        /// <summary>
        /// Call moduleAvailability.CallWhenAvailable() to run a method after the Module is initialized.
        /// </summary>
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

            OnDictionariesCreated?.Invoke(NonEliteMoonstormEquipments, EliteMoonstormEquipments);
            moduleAvailability.MakeAvailable();
        }

        #region Equipments
        /// <summary>
        /// <inheritdoc cref="ModuleBase{T}.GetContentClasses{T}(Type)"/>
        /// <para>T in this case is <see cref="EquipmentBase"/></para>
        /// <para>While Type in this case is <see cref="EliteEquipmentBase"/></para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's <see cref="EquipmentBase"/></returns>
        protected virtual IEnumerable<EquipmentBase> GetEquipmentBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Equipments found inside {GetType().Assembly}");
#endif
            return GetContentClasses<EquipmentBase>(typeof(EliteEquipmentBase));
        }

        /// <summary>
        /// Adds an EquipmentBase's EquipmentDef to the game and to the ContentPack
        /// </summary>
        /// <param name="equip">The EquipmentBase to add</param>
        /// <param name="dictionary">Optional, a dictionary to add your initialized EquipmentBase and EquipmentDef</param>
        protected void AddEquipment(EquipmentBase equip, Dictionary<EquipmentDef, EquipmentBase> dictionary = null)
        {
            InitializeContent(equip);
            dictionary?.Add(equip.EquipmentDef, equip);
        }

        #region EliteEquipments
        /// <summary>
        /// <inheritdoc cref="ModuleBase{T}.GetContentClasses{T}(Type)"/>
        /// <para>T in this case is <see cref="EliteEquipmentBase"/></para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's <see cref="EliteEquipmentBase"/></returns>
        protected virtual IEnumerable<EliteEquipmentBase> GetEliteEquipmentBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Elite Equipments found inside {GetType().Assembly}");
#endif
            return GetContentClasses<EliteEquipmentBase>();
        }

        /// <summary>
        /// Adds an EliteEquipmentBase's EquipmentDef to the game and to the ContentPack
        /// <para>The EliteDef itself is not added, for adding the EliteDef, implement <see cref="EliteModuleBase"/></para>
        /// </summary>
        /// <param name="eliteEqp">The EliteEquipmentBase to add</param>
        /// <param name="dictionary">Optional, a dictionary to add your initialized EquipmentDef and EliteEquipmentBase</param>
        protected void AddEliteEquipment(EliteEquipmentBase eliteEqp, Dictionary<EquipmentDef, EliteEquipmentBase> dictionary = null)
        {
            InitializeContent(eliteEqp);
            dictionary?.Add(eliteEqp.EquipmentDef, eliteEqp);
        }
        #endregion

        /// <summary>
        /// Adds the <see cref="EquipmentDef"/> from the <paramref name="contentClass"/> to your ContentPack.
        /// <para>Once added, it'll call <see cref="ContentBase.Initialize"/>, unless if <paramref name="contentClass"/> is an EliteEquipmentBase</para>
        /// </summary>
        /// <param name="contentClass">The content class being initialized</param>
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
