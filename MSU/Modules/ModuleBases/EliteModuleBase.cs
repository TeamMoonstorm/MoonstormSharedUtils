using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using Moonstorm.Components;

namespace Moonstorm
{
    /// <summary>
    /// A Module Base for managing Elites
    /// </summary>
    public abstract class EliteModuleBase : ModuleBase
    {
        /// <summary>
        /// List of all the loaded Elites by Moonstorm Shared Utils
        /// </summary>
        public static List<MSEliteDef> MoonstormElites = new List<MSEliteDef>();

        //public static Dictionary<MSEliteDef, EliteEquipmentBase> MoonstormElites = new Dictionary<MSEliteDef, EliteEquipmentBase>();

        /// <summary>
        /// Returns all the loaded elites.
        /// </summary>
        public static MSEliteDef[] LoadedEliteDefs
        {
            get
            {
                return MoonstormElites.ToArray();
            }
        }

        [SystemInitializer(typeof(EliteCatalog))]
        public static void HookInit()
        {
            MSULog.LogI("Subscribing to delegates related to Elites.");
            IL.RoR2.CharacterModel.UpdateMaterials += AddEliteMaterial;

            //On.RoR2.CombatDirector.Init += AddElites; Initializer literally runs this hook too late, lol. so i'm switching it to OnLoad
            RoR2Application.onLoad += AddElites;
        }

        #region Elites
        /// <summary>
        /// Finds all the EliteEquipmentBases corresponding to the eliteDefs in your assetbundle
        /// <para>Requires the PickupModuleBase to be initialzed</para>
        /// </summary>
        /// <returns>An IEnumerable of all your Assembly's EliteEquipmentBases</returns>
        public virtual IEnumerable<EliteEquipmentBase> InitializeEliteEquipments()
        {
            MSULog.LogD($"Getting the Elites found inside {AssetBundle}...");
            var toReturn = new List<EliteEquipmentBase>();
            var eliteDefs = AssetBundle.LoadAllAssets<MSEliteDef>();
            foreach (MSEliteDef def in eliteDefs)
            {
                EliteEquipmentBase equipment;
                bool flag = PickupModuleBase.MoonstormEliteEquipments.TryGetValue(def.eliteEquipmentDef, out equipment);
                if (flag)
                {
                    toReturn.Add(equipment);
                }
            }
            return toReturn;
        }

        /// <summary>
        /// Completely adds an elite to the game
        /// </summary>
        /// <param name="equip">The EliteEquipmentBase class</param>
        /// <param name="contentPack">The content pack of your mod</param>
        /// <param name="eliteList">Optional, a List for storing all the eliteDefs</param>
        /// <param name="equipDictionary">Optional, a Dictionary for getting an EquipmentBase by feeding it the corresponding EquipmentDef (You can cast the EqpBase into an EliteEquipmentBase)</param>
        public void AddElite(EliteEquipmentBase equip, SerializableContentPack contentPack, List<MSEliteDef> eliteList = null, Dictionary<EquipmentDef, EquipmentBase> equipDictionary = null)
        {
            HG.ArrayUtils.ArrayAppend(ref contentPack.equipmentDefs, equip.EquipmentDef);
            equip.Initialize();
            if (equipDictionary != null)
                equipDictionary.Add(equip.EquipmentDef, equip);

            MSULog.LogD($"Equipment {equip.EquipmentDef} added to {contentPack.name}");

            equip.EliteDef.shaderEliteRampIndex = 0;
            HG.ArrayUtils.ArrayAppend(ref contentPack.eliteDefs, equip.EliteDef);
            MoonstormElites.Add(equip.EliteDef);
            if (equip.EliteDef.overlay)
                BuffModuleBase.MoonstormOverlayMaterials.Add(equip.EquipmentDef.passiveBuffDef, equip.EliteDef.overlay);

            MSULog.LogD($"Elite {equip.EliteDef} added to {contentPack.name}");

        }
        #endregion

        #region Hooks
        private static void AddElites()
        {
            MSULog.LogI($"Adding Elites found in Moonstorm Elites.");
            foreach (var eliteDef in MoonstormElites)
            {
                switch (eliteDef.eliteTier)
                {
                    case EliteTiers.Basic:
                        HG.ArrayUtils.ArrayAppend(ref CombatDirector.eliteTiers[1].eliteTypes, eliteDef);
                        HG.ArrayUtils.ArrayAppend(ref CombatDirector.eliteTiers[2].eliteTypes, eliteDef);
                        MSULog.LogD($"Added Elite {eliteDef.name} to Combat Director's Tier 1 & 2's Elites.");
                        break;
                    case EliteTiers.PostLoop:
                        HG.ArrayUtils.ArrayAppend(ref CombatDirector.eliteTiers[3].eliteTypes, eliteDef);
                        MSULog.LogD($"Added Elite {eliteDef.name} to Combat Director's Tier 3 Elites.");
                        break;
                    case EliteTiers.Other:
                        break;
                }
            }
        }

        //TYVM Mystic!
        private static void AddEliteMaterial(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterModel>("propertyStorage"),
                x => x.MatchLdsfld(typeof(CommonShaderProperties), "_EliteIndex")
            );
            c.GotoNext(
                MoveType.After,
                x => x.MatchCallOrCallvirt<MaterialPropertyBlock>("SetFloat")
            );
            c.Emit(OpCodes.Ldarg, 0);
            c.EmitDelegate<Action<CharacterModel>>((model) =>
            {
                var body = model.body;
                if(body)
                {
                    body.GetComponent<MoonstormEliteBehavior>()?.UpdateShaderRamp();
                }
            });
        }
        #endregion
    }
}
