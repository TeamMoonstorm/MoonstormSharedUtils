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
        public static ReadOnlyDictionary<BuffDef, BuffBase> MoonstormBuffs
        {
            get
            {
                if (!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve Dictionary {nameof(MoonstormBuffs)}", typeof(BuffModuleBase));
                    return null;
                }
                return MoonstormBuffs;
            }
            private set
            {
                MoonstormBuffs = value;
            }
        }
        internal static Dictionary<BuffDef, BuffBase> buffs = new Dictionary<BuffDef, BuffBase>();
        public static Action<ReadOnlyDictionary<BuffDef, BuffBase>, ReadOnlyDictionary<BuffDef, Material>> OnDictionariesCreated;

        public static ReadOnlyDictionary<BuffDef, Material> MoonstormOverlayMaterials
        {
            get
            {
                if (!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve Dictionary {nameof(MoonstormOverlayMaterials)}", typeof(BuffModuleBase));
                    return null;
                }
                return MoonstormOverlayMaterials;
            }
            private set
            {
                MoonstormOverlayMaterials = value;
            }
        }
        internal static Dictionary<BuffDef, Material> overlayMaterials = new Dictionary<BuffDef, Material>();

        public static BuffDef[] LoadedBuffDefs { get => MoonstormBuffs.Keys.ToArray(); }

        public static bool Initialized { get; private set; } = false;
        #endregion

        [SystemInitializer(typeof(BuffCatalog))]
        private static void SystemInit()
        {
            Initialized = true;
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
            if (Initialized)
            {
                ThrowModuleInitialized($"Retrieve BuffBase List", typeof(BuffModuleBase));
                return null;
            }

            MSULog.Debug($"Getting the Buffs found inside {GetType().Assembly}...");
            return GetContentClasses<BuffBase>();
        }

        protected void AddBuff(BuffBase buff, Dictionary<BuffDef, BuffBase> buffDictionary = null)
        {
            if (Initialized)
            {
                ThrowModuleInitialized($"Add BuffBase To ContentPack", typeof(ArtifactModuleBase));
                return;
            }

            if (InitializeContent(buff) && buffDictionary != null)
                AddSafelyToDict(ref buffDictionary, buff.BuffDef, buff);

            MSULog.Debug($"Buff {buff.BuffDef} added to {SerializableContentPack.name}");
        }

        protected override bool InitializeContent(BuffBase contentClass)
        {
            if(AddSafely(ref SerializableContentPack.buffDefs, contentClass.BuffDef))
            {
                contentClass.Initialize();

                if (contentClass.OverlayMaterial)
                    AddSafelyToDict(ref overlayMaterials, contentClass.BuffDef, contentClass.OverlayMaterial);

                AddSafelyToDict(ref buffs, contentClass.BuffDef, contentClass);
                return true;
            }
            return false;
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
