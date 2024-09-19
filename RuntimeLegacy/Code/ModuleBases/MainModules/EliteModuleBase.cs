using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Moonstorm
{
    public abstract class EliteModuleBase : ContentModule<EliteEquipmentBase>
    {
        #region Propertiess and Fields

        public static ReadOnlyCollection<MSEliteDef> MoonstormElites { get; private set; }
        internal static List<MSEliteDef> eliteDefs = new List<MSEliteDef>();

        public abstract AssetBundle AssetBundle { get; }

        public static ResourceAvailability moduleAvailability;
        #endregion

        [SystemInitializer(new Type[] { typeof(BuffCatalog), typeof(EquipmentCatalog), typeof(EliteCatalog) })]
        private static void SystemInit()
        {
            MSULog.Info($"Initializing Elite Module...");

            AddElitesViaDirectorAPI();
            MoonstormElites = new ReadOnlyCollection<MSEliteDef>(eliteDefs);
            eliteDefs = null;

            moduleAvailability.MakeAvailable();
        }

        private static void AddElitesViaDirectorAPI()
        {
            CombatDirector.EliteTierDef[] vanillaTiers = R2API.EliteAPI.VanillaEliteTiers;
            foreach (MSEliteDef eliteDef in eliteDefs)
            {
                switch (eliteDef.eliteTier)
                {
                    case VanillaEliteTier.HonorDisabled:
                        HG.ArrayUtils.ArrayAppend(ref vanillaTiers[1].eliteTypes, eliteDef);
                        break;
                    case VanillaEliteTier.HonorActive:
                        HG.ArrayUtils.ArrayAppend(ref vanillaTiers[2].eliteTypes, eliteDef);
                        break;
                    case VanillaEliteTier.PostLoop:
                        HG.ArrayUtils.ArrayAppend(ref vanillaTiers[3].eliteTypes, eliteDef);
                        break;
                    case VanillaEliteTier.Lunar:
                        HG.ArrayUtils.ArrayAppend(ref vanillaTiers[4].eliteTypes, eliteDef);
                        break;
                }
#if DEBUG
                MSULog.Debug($"Added {eliteDef} to the {eliteDef.eliteTier} tier");
#endif
            }
        }

        #region Elites
        protected virtual IEnumerable<EliteEquipmentBase> GetInitializedEliteEquipmentBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the initialized EliteEquipmentBases inside {GetType().Assembly}");
#endif
            var initializedEliteEquipmentBases = new List<EliteEquipmentBase>();
            foreach (MSEliteDef def in AssetBundle.LoadAllAssets<MSEliteDef>())
            {
                EliteEquipmentBase equipmentBase;
                bool flag = EquipmentModuleBase.eliteEquip.TryGetValue(def.eliteEquipmentDef, out equipmentBase);
                if (flag && !initializedEliteEquipmentBases.Contains(equipmentBase))
                {
                    initializedEliteEquipmentBases.Add(equipmentBase);
                }
            }
            return initializedEliteEquipmentBases;
        }

        protected void AddElite(EliteEquipmentBase elite, List<MSEliteDef> list = null)
        {
            InitializeContent(elite);
            list?.AddRange(elite.EliteDefs);
#if DEBUG
            MSULog.Debug($"Elite {elite} Initialized and Ensured in {SerializableContentPack.name}");
#endif
        }

        protected override void InitializeContent(EliteEquipmentBase contentClass)
        {
            contentClass.Initialize();
            Dictionary<Texture2D, List<MSEliteDef>> rampToElites = new Dictionary<Texture2D, List<MSEliteDef>>();
            foreach (MSEliteDef eliteDef in contentClass.EliteDefs)
            {
                AddSafely(ref SerializableContentPack.eliteDefs, eliteDef);
                eliteDefs.Add(eliteDef);
                if (eliteDef.overlay && contentClass.EquipmentDef.passiveBuffDef)
                {
                    BuffModuleBase.overlayMaterials[contentClass.EquipmentDef.passiveBuffDef] = eliteDef.overlay;
                }
                if (eliteDef.eliteRamp)
                {
                    if (!rampToElites.ContainsKey(eliteDef.eliteRamp))
                    {
                        rampToElites.Add(eliteDef.eliteRamp, new List<MSEliteDef>());
                    }
                    rampToElites[eliteDef.eliteRamp].Add(eliteDef);
                }
            }

            foreach (var kvp in rampToElites)
            {
                EliteRamp.AddRampToMultipleElites(kvp.Value, kvp.Key);
            }
        }
        #endregion
    }
}
