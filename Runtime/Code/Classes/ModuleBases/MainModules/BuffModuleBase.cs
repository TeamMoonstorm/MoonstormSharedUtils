using Moonstorm.Components;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// The <see cref="BuffModuleBase"/> is a <see cref="ContentModule{T}"/> that handles the <see cref="BuffBase"/> class
    /// <para>The BuffModule's main job is to handle the proper addition of BuffDefs and <see cref="Moonstorm.Components.BaseBuffBodyBehavior"/>s</para>
    /// <para>It can also tie a specific BuffDef to a Material so an overlay can appear on a body that has the buff</para>
    /// <para>Inherit from this module if you want to load and manage Characters with <see cref="CharacterBase"/> systems</para>
    /// </summary>
    public abstract class BuffModuleBase : ContentModule<BuffBase>
    {
        #region Properties and Fields
        /// <summary>
        /// A ReadOnlyDictionary that can be useed for loading a specific BuffBase by giving it's tied BuffDef
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionariesCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<BuffDef, BuffBase> MoonstormBuffs { get; private set; }
        internal static Dictionary<BuffDef, BuffBase> buffs = new Dictionary<BuffDef, BuffBase>();

        /// <summary>
        /// A ReadOnlyDictionary that can be used for obtaining a specific BuffDef's overlay material
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionariesCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<BuffDef, Material> MoonstormOverlayMaterials { get; private set; }
        internal static Dictionary<BuffDef, Material> overlayMaterials = new Dictionary<BuffDef, Material>();

        /// <summary>
        /// Loads all the BuffDefs from <see cref="MoonstormBuffs">
        /// </summary>
        public static BuffDef[] LoadedBuffDefs { get => MoonstormBuffs.Keys.ToArray(); }
        /// <summary>
        /// An action that gets invoked when the <see cref="MoonstormBuffs"/> and <see cref="MoonstormOverlayMaterials"/> have been populated
        /// </summary>
        [Obsolete("use \"ModuleAvailability.CallWhenAvailable()\" instead")]
        public static Action<ReadOnlyDictionary<BuffDef, BuffBase>, ReadOnlyDictionary<BuffDef, Material>> OnDictionariesCreated;
        /// <summary>
        /// Call ModuleAvailability.CallWhenAvailable() to run a method after the Module is initialized.
        /// </summary>
        public static ResourceAvailability ModuleAvailability { get; } = default(ResourceAvailability);
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
            ModuleAvailability.MakeAvailable();
        }

        #region Buffs
        /// <summary>
        /// <inheritdoc cref="ModuleBase{T}.GetContentClasses{T}(Type)"/>
        /// <para>T in this case is <see cref="BuffBase"/></para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's <see cref="BuffBase"/></returns>
        protected virtual IEnumerable<BuffBase> GetBuffBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Buffs found inside {GetType().Assembly}...");
#endif
            return GetContentClasses<BuffBase>();
        }

        /// <summary>
        /// Adds a BuffBase's BuffDef to your mod's ContentPack
        /// <para>If the BuffDef implements <see cref="BuffBase.OverlayMaterial"/>, then the material and buff def will be added to the <see cref="MoonstormOverlayMaterials"/> as well</para>
        /// </summary>
        /// <param name="buff">The BuffBase to add</param>
        /// <param name="buffDictionary">Optional, a dictionary to add your initialied BuffBase and BuffDefs</param>
        protected void AddBuff(BuffBase buff, Dictionary<BuffDef, BuffBase> buffDictionary = null)
        {
            InitializeContent(buff);
            buffDictionary?.Add(buff.BuffDef, buff);

#if DEBUG
            MSULog.Debug($"Buff {buff.BuffDef} Initialized and ensured in {SerializableContentPack.name}");
#endif
        }

        /// <summary>
        /// Adds a BuffBase's BuffDef to your mod's ContentPack
        /// <para>If the BuffDef implements <see cref="BuffBase.OverlayMaterial"/>, then the material and buff def will be added to the <see cref="MoonstormOverlayMaterials"/> as well</para>
        /// <para>Once added, it'll call <see cref="ContentBase.Initialize"/></para>
        /// </summary>
        /// <param name="contentClass">The content class being initialized</param>
        protected override void InitializeContent(BuffBase contentClass)
        {
            AddSafely(ref SerializableContentPack.buffDefs, contentClass.BuffDef);

            contentClass.Initialize();

            if (contentClass.OverlayMaterial)
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
