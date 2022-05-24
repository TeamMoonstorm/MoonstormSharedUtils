using Mono.Cecil.Cil;
using MonoMod.Cil;
using Moonstorm.Components;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using RoR2BepInExPack.GlobalEliteRampSolution;

namespace Moonstorm
{
    public abstract class EliteModuleBase : ContentModule<EliteEquipmentBase>
    {
        #region Propertiess and Fields
        public static ReadOnlyCollection<MSEliteDef> MoonstormElites { get; private set; }
        internal static List<MSEliteDef> eliteDefs = new List<MSEliteDef>();

        public abstract AssetBundle AssetBundle { get; }
        public static Action<ReadOnlyCollection<MSEliteDef>> OnListCreated;
        #endregion

        [SystemInitializer(new Type[] { typeof(BuffCatalog), typeof(EquipmentCatalog), typeof(EliteCatalog) })]
        private static void SystemInit()
        {
            MSULog.Info($"Initializing Elite Module...");

            AddElitesViaDirectorAPI();
            MoonstormElites = new ReadOnlyCollection<MSEliteDef>(eliteDefs);
            eliteDefs = null;

            OnListCreated?.Invoke(MoonstormElites);
        }

        private static void AddElitesViaDirectorAPI()
        {
            CombatDirector.EliteTierDef[] vanillaTiers = R2API.EliteAPI.VanillaEliteTiers;
            foreach (MSEliteDef eliteDef in eliteDefs)
            {
                if (eliteDef.eliteRamp)
                    EliteRampManager.AddRamp(eliteDef, eliteDef.eliteRamp);

                switch(eliteDef.eliteTier)
                {
                    case VanillaEliteTier.HonorDisabled:
                        MSULog.Debug($"Added {eliteDef} to the NotEliteOnlyArtifactEnabled tier (1)");
                        HG.ArrayUtils.ArrayAppend(ref vanillaTiers[1].eliteTypes, eliteDef);
                        break;
                    case VanillaEliteTier.HonorActive:
                        MSULog.Debug($"Added {eliteDef} to the EliteOnlyArtifactEnabled tier (2)");
                        HG.ArrayUtils.ArrayAppend(ref vanillaTiers[2].eliteTypes, eliteDef);
                        break;
                    case VanillaEliteTier.PostLoop:
                        MSULog.Debug($"Added {eliteDef} to the Post Loop tier (3)");
                        HG.ArrayUtils.ArrayAppend(ref vanillaTiers[3].eliteTypes, eliteDef);
                        break;
                    case VanillaEliteTier.Lunar:
                        MSULog.Debug($"Added {eliteDef} to the Lunar tier (4)");
                        HG.ArrayUtils.ArrayAppend(ref vanillaTiers[4].eliteTypes, eliteDef);
                        break;
                }
            }
        }

        #region Elites
        protected virtual IEnumerable<EliteEquipmentBase> GetInitializedEliteEquipmentBases()
        {
            MSULog.Debug($"Getting the initialized EliteEquipmentBases inside {GetType().Assembly}");
            var initializedEliteEquipmentBases = new List<EliteEquipmentBase>();
            foreach(MSEliteDef def in AssetBundle.LoadAllAssets<MSEliteDef>())
            {
                EliteEquipmentBase equipmentBase;
                bool flag = EquipmentModuleBase.eliteEquip.TryGetValue(def.eliteEquipmentDef, out equipmentBase);
                if(flag)
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
            MSULog.Debug($"Elite {elite} Initialized and Ensured in {SerializableContentPack.name}");
        }

        protected override void InitializeContent(EliteEquipmentBase contentClass)
        {
            contentClass.Initialize();
            foreach(MSEliteDef eliteDef in contentClass.EliteDefs)
            {
                eliteDefs.Add(eliteDef);
                if (eliteDef.overlay && contentClass.EquipmentDef.passiveBuffDef)
                {
                    BuffModuleBase.overlayMaterials[contentClass.EquipmentDef.passiveBuffDef] = eliteDef.overlay;
                }
            }
        }
        #endregion
    }
}
