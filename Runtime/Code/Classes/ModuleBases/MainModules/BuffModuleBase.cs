using Moonstorm.Components;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    public abstract class BuffModuleBase : ContentModule<BuffBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<BuffDef, BuffBase> MoonstormBuffs { get; private set; }
        internal static Dictionary<BuffDef, BuffBase> buffs = new Dictionary<BuffDef, BuffBase>();

        public static ReadOnlyDictionary<BuffDef, Material> MoonstormOverlayMaterials { get; private set; }
        internal static Dictionary<BuffDef, Material> overlayMaterials = new Dictionary<BuffDef, Material>();

        public static BuffDef[] LoadedBuffDefs { get => MoonstormBuffs.Keys.ToArray(); }
        public static Action<ReadOnlyDictionary<BuffDef, BuffBase>, ReadOnlyDictionary<BuffDef, Material>> OnDictionariesCreated;
        #endregion

        [SystemInitializer(typeof(BuffCatalog))]
        private static void SystemInit()
        {
            MSULog.Info("Initializing Buff Module...");
            On.RoR2.CharacterBody.SetBuffCount += OnBuffsChanged;
            On.RoR2.CharacterModel.UpdateOverlays += AddBuffOverlay;

            MoonstormBuffs = new ReadOnlyDictionary<BuffDef, BuffBase>(buffs);
            buffs = null;

            MoonstormOverlayMaterials = new ReadOnlyDictionary<BuffDef, Material>(overlayMaterials);
            overlayMaterials = null;
            
            OnDictionariesCreated?.Invoke(MoonstormBuffs, MoonstormOverlayMaterials);
        }

        #region Buffs
        protected virtual IEnumerable<BuffBase> GetBuffBases()
        {
            MSULog.Debug($"Getting the Buffs found inside {GetType().Assembly}...");
            return GetContentClasses<BuffBase>();
        }

        protected void AddBuff(BuffBase buff, Dictionary<BuffDef, BuffBase> buffDictionary = null)
        {
            InitializeContent(buff);
            buffDictionary?.Add(buff.BuffDef, buff);

            MSULog.Debug($"Buff {buff.BuffDef} Initialized and ensured in {SerializableContentPack.name}");
        }

        protected override void InitializeContent(BuffBase contentClass)
        {
            AddSafely(ref SerializableContentPack.buffDefs, contentClass.BuffDef);

            contentClass.Initialize();

            if(contentClass.OverlayMaterial)
                overlayMaterials[contentClass.BuffDef] = contentClass.OverlayMaterial;

            buffs[contentClass.BuffDef] = contentClass;
        }
        #endregion

        #region Hooks
        private static void OnBuffsChanged(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            orig(self, buffType, newCount);
            if (!self)
                return;

            var contentManager = self.GetComponent<MoonstormContentManager>();
            contentManager.StartGetInterfaces();
        }

        private static void AddBuffOverlay(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel model)
        {
            orig(model);
            if (!model.body)
                return;
            foreach (var buffKeyValue in MoonstormOverlayMaterials)
                if (model.body.HasBuff(buffKeyValue.Key))
                    AddOverlay(model, buffKeyValue.Value);
        }

        private static void AddOverlay(CharacterModel model, Material overlayMaterial)
        {
            if (model.activeOverlayCount >= CharacterModel.maxOverlays || !overlayMaterial)
                return;
            Material[] array = model.currentOverlays;
            int num = model.activeOverlayCount;
            model.activeOverlayCount = num + 1;
            array[num] = overlayMaterial;
        }
        #endregion
    }
}
