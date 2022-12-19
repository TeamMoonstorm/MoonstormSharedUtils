using Mono.Cecil.Cil;
using MonoMod.Cil;
using Moonstorm.Components;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using R2API;
//using RoR2BepInExPack.GlobalEliteRampSolution;

namespace Moonstorm
{
    /// <summary>
    /// The <see cref="EliteModuleBase"/> is a <see cref="ContentModule{T}"/> that handles the <see cref="EliteEquipmentBase"/> class
    /// <para><see cref="EliteModuleBase"/>'s main job is to handle the proper addition of the EliteDefs from <see cref="EliteEquipmentBase"/> inheriting classes</para>
    /// <para>This has an indirect dependency with <see cref="EquipmentModuleBase"/>, it is highly recommended to initialize that module before this one</para>
    /// <para><see cref="EliteModuleBase"/> will automatically handle the use method of the Equipment by running <see cref="EquipmentBase.FireAction(EquipmentSlot)"/>, alongside adding the EliteDefs to the CombatDirector</para>
    /// <para>Inherit from this module if you want to load and manage Elites with <see cref="EliteEquipmentBase"/> systems</para>
    /// </summary>
    public abstract class EliteModuleBase : ContentModule<EliteEquipmentBase>
    {
        #region Propertiess and Fields
        /// <summary>
        /// A ReadOnlyCollection of all the EliteDefs from MSU
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnListCreated"/> to ensure the list is not empty</para>
        /// </summary>
        public static ReadOnlyCollection<MSEliteDef> MoonstormElites { get; private set; }
        internal static List<MSEliteDef> eliteDefs = new List<MSEliteDef>();

        /// <summary>
        /// The AssetBundle where your EliteDefs are stored
        /// </summary>
        public abstract AssetBundle AssetBundle { get; }
        /// <summary>
        /// An action that gets invoked when the <see cref="MoonstormElites"/> List has been populated
        /// </summary>
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
        /// <summary>
        /// Calling this method will look into your <see cref="AssetBundle"/> and load all the <see cref="MSEliteDef"/> from it
        /// <para>Once all Elitedefs are loaded, it'll look for all the <see cref="EliteEquipmentBase"/> initialized that have the Elitedef in the bundle, and return them</para>
        /// <para>This effectively means you need to call <see cref="EquipmentModuleBase.GetEliteEquipmentBases"/> and call <see cref="EquipmentModuleBase.AddEliteEquipment(EliteEquipmentBase, Dictionary{EquipmentDef, EliteEquipmentBase})"/> before calling this method</para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's initialized <see cref="EliteEquipmentBase"/>s</returns>
        protected virtual IEnumerable<EliteEquipmentBase> GetInitializedEliteEquipmentBases()
        {
            MSULog.Debug($"Getting the initialized EliteEquipmentBases inside {GetType().Assembly}");
            var initializedEliteEquipmentBases = new List<EliteEquipmentBase>();
            foreach(MSEliteDef def in AssetBundle.LoadAllAssets<MSEliteDef>())
            {
                EliteEquipmentBase equipmentBase;
                bool flag = EquipmentModuleBase.eliteEquip.TryGetValue(def.eliteEquipmentDef, out equipmentBase);
                if(flag && !initializedEliteEquipmentBases.Contains(equipmentBase))
                {
                    initializedEliteEquipmentBases.Add(equipmentBase);
                }
            }
            return initializedEliteEquipmentBases;
        }

        /// <summary>
        /// Adds an <see cref="EliteEquipmentBase"/>'s Elitedefs to the game and to the ContentPack
        /// </summary>
        /// <param name="elite">The EliteEquipmentBase to add</param>
        /// <param name="list">Optional, a list to store your initialized EliteDefs</param>
        protected void AddElite(EliteEquipmentBase elite, List<MSEliteDef> list = null)
        {
            InitializeContent(elite);
            list?.AddRange(elite.EliteDefs);
            MSULog.Debug($"Elite {elite} Initialized and Ensured in {SerializableContentPack.name}");
        }

        /// <summary>
        /// Adds the <see cref="MSEliteDef"/> from the <paramref name="contentClass"/> to your ContentPack
        /// <para>Once added, it'll call <see cref="ContentBase.Initialize"/></para>
        /// </summary>
        /// <param name="contentClass">The content class being initialized</param>
        protected override void InitializeContent(EliteEquipmentBase contentClass)
        {
            contentClass.Initialize();
            Dictionary<Texture2D, List<MSEliteDef>> rampToElites = new Dictionary<Texture2D, List<MSEliteDef>>();
            foreach(MSEliteDef eliteDef in contentClass.EliteDefs)
            {
                AddSafely(ref SerializableContentPack.eliteDefs, eliteDef);
                eliteDefs.Add(eliteDef);
                if (eliteDef.overlay && contentClass.EquipmentDef.passiveBuffDef)
                {
                    BuffModuleBase.overlayMaterials[contentClass.EquipmentDef.passiveBuffDef] = eliteDef.overlay;
                }
                if(eliteDef.eliteRamp)
                {
                    if(!rampToElites.ContainsKey(eliteDef.eliteRamp))
                    {
                        rampToElites.Add(eliteDef.eliteRamp, new List<MSEliteDef>());
                    }
                    rampToElites[eliteDef.eliteRamp].Add(eliteDef);
                }
            }

            foreach(var kvp in rampToElites)
            {
                EliteRamp.AddRampToMultipleElites(kvp.Value, kvp.Key);
            }
        }
        #endregion
    }
}
