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
        public static ReadOnlyCollection<MSEliteDef> MoonstormElites
        {
            get
            {
                if(!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve dictionary {nameof(MoonstormElites)}", typeof(EliteModuleBase));
                    return null;
                }
                return moonstormElites;
            }
            private set
            {
                moonstormElites = value;
            }
        }
        private static ReadOnlyCollection<MSEliteDef> moonstormElites;
        internal static List<MSEliteDef> eliteDefs = new List<MSEliteDef>();
        public static Action<ReadOnlyCollection<MSEliteDef>> OnListCreated;

        public static bool Initialized { get; private set; } = false;

        public abstract AssetBundle AssetBundle { get; }

        [SystemInitializer(new Type[] { typeof(BuffCatalog), typeof(EquipmentCatalog), typeof(EliteCatalog) })]
        private static void SystemInit()
        {
            Initialized = true;
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
            if(Initialized)
            {
                ThrowModuleInitialized($"Retrieve Initialized EliteEquipmentBases list", typeof(EliteModuleBase));
                return null;
            }

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
            if (Initialized)
            {
                ThrowModuleInitialized($"Add EliteEquipmentBase to ContentPack", typeof(EliteModuleBase));
                return;
            }

            if (InitializeContent(elite) && list != null)
                AddSafelyToList(ref list, elite.EliteDef);

            MSULog.Debug($"Elite {elite.EliteDef} added to {SerializableContentPack.name}");
        }

        protected override bool InitializeContent(EliteEquipmentBase contentClass)
        {
            if(AddSafely(ref SerializableContentPack.eliteDefs, contentClass.EliteDef))
            {
                contentClass.Initialize();

                AddSafelyToList(ref eliteDefs, contentClass.EliteDef);

                if(contentClass.EliteDef.overlay && contentClass.EquipmentDef.passiveBuffDef)
                {
                    BuffModuleBase.overlayMaterials.Add(contentClass.EquipmentDef.passiveBuffDef, contentClass.EliteDef.overlay);
                }
                return true;
            }
            return false;
        }
        #endregion
    }
}
