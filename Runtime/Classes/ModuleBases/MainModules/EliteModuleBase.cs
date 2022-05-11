using Mono.Cecil.Cil;
using MonoMod.Cil;
using Moonstorm.Components;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using RoR2BepInExPack;

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

            MoonstormElites = new ReadOnlyCollection<MSEliteDef>(eliteDefs);
            eliteDefs = null;

            OnListCreated?.Invoke(MoonstormElites);
        }

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
        /*[SystemInitializer(typeof(EliteCatalog))]
        public static void HookInit()
        {
            MSULog.Info("Subscribing to delegates related to Elites.");
            IL.RoR2.CharacterModel.UpdateMaterials += AddEliteMaterial;
            RoR2Application.onLoad += AddElites;
        }

        #region Elites
        public virtual IEnumerable<EliteEquipmentBase> GetNonInitializedEliteEquipments()
        {
            MSULog.Debug($"Getting the Elites found inside {AssetBundle}...");

            var toReturn = new List<EliteEquipmentBase>();
            var eliteDefs = AssetBundle.LoadAllAssets<MSEliteDef>();
            foreach (MSEliteDef def in eliteDefs)
            {
                EliteEquipmentBase equipment;
                bool flag = PickupModuleBase.nonInitializedEliteEquipments.TryGetValue(def.eliteEquipmentDef, out equipment);
                if (flag)
                {
                    toReturn.Add(equipment);
                }
            }
            return toReturn;
        }

        public void AddElite(EliteEquipmentBase equip, SerializableContentPack contentPack, List<MSEliteDef> eliteList = null, Dictionary<EquipmentDef, EquipmentBase> equipDictionary = null)
        {
            //Adding Equipmentdef to content pack
            HG.ArrayUtils.ArrayAppend(ref contentPack.equipmentDefs, equip.EquipmentDef);
            equip.Initialize();
            if (equipDictionary != null)
                equipDictionary.Add(equip.EquipmentDef, equip);
            MSULog.Debug($"Equipment {equip.EquipmentDef} added to {contentPack.name}");

            //Adding ElitteDef to contentpack
            equip.EliteDef.shaderEliteRampIndex = 0;
            HG.ArrayUtils.ArrayAppend(ref contentPack.eliteDefs, equip.EliteDef);
            MoonstormElites.Add(equip.EliteDef);
            if (equip.EliteDef.overlay)
                BuffModuleBase.MoonstormOverlayMaterials.Add(equip.EquipmentDef.passiveBuffDef, equip.EliteDef.overlay);
            MSULog.Debug($"Elite {equip.EliteDef} added to {contentPack.name}");

            //Adding elite equipment to dictionary.
            PickupModuleBase.MoonstormEliteEquipments.Add(equip.EquipmentDef, equip);
        }
        #endregion

        #region Hooks
        private static void AddElites()
        {
            MSULog.Info($"Adding Elites found in Moonstorm Elites.");
            foreach (var eliteDef in MoonstormElites)
            {
                switch (eliteDef.eliteTier)
                {
                    case EliteTiers.Basic:
                        HG.ArrayUtils.ArrayAppend(ref CombatDirector.eliteTiers[1].eliteTypes, eliteDef);
                        HG.ArrayUtils.ArrayAppend(ref CombatDirector.eliteTiers[2].eliteTypes, eliteDef);
                        MSULog.Debug($"Added Elite {eliteDef.name} to Combat Director's Tier 1 & 2's Elites.");
                        break;
                    case EliteTiers.PostLoop:
                        HG.ArrayUtils.ArrayAppend(ref CombatDirector.eliteTiers[3].eliteTypes, eliteDef);
                        MSULog.Debug($"Added Elite {eliteDef.name} to Combat Director's Tier 3 Elites.");
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
                if (body)
                {
                    body.GetComponent<MoonstormEliteBehavior>()?.UpdateShaderRamp();
                }
            });
        }
        #endregion*/
    }
}
