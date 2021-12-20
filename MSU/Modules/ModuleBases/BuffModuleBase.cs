using Moonstorm.Components;
using RoR2;
using RoR2.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A Module Base for managing Buffs
    /// <para>Automatically handles MaterialOverlays used in buffs and adding the ItemBehavior for them</para>
    /// </summary>
    public abstract class BuffModuleBase : ModuleBase
    {
        /// <summary>
        /// Dictionary of all the Buffs loaded by Moonstorm Shared Utils
        /// </summary>
        public static Dictionary<BuffDef, BuffBase> MoonstormBuffs = new Dictionary<BuffDef, BuffBase>();

        /// <summary>
        /// Dictionary of all the Overlay materials loaded by Moonstorm Shared Utils
        /// </summary>
        public static Dictionary<BuffDef, Material> MoonstormOverlayMaterials = new Dictionary<BuffDef, Material>();

        /// <summary>
        /// Returns all the Buffs loaded by Moonstorm Shared Utils
        /// </summary>
        public BuffDef[] LoadedBuffDefs { get => MoonstormBuffs.Keys.ToArray(); }

        [SystemInitializer(typeof(BuffCatalog))]
        private static void HookInit()
        {
            MSULog.LogI("Subscribing to delegates related to buffs.");
            On.RoR2.CharacterBody.SetBuffCount += OnBuffsChanged;
            On.RoR2.CharacterModel.UpdateOverlays += AddBuffOverlay;
        }

        #region Buffs
        /// <summary>
        /// Finds all the BuffBase inheriting classes in your assembly and creates instances for each found.
        /// <para>Ignores classes with the "DisabledContent" attribute</para>
        /// </summary>
        /// <returns>An IEnumerable of all your Assembly's ItemBases</returns>
        public virtual IEnumerable<BuffBase> InitializeBuffs()
        {
            MSULog.LogD($"Getting the Buffs found inside {GetType().Assembly}...");
            return GetContentClasses<BuffBase>();
        }

        /// <summary>
        /// Initializes and Adds a Buff
        /// </summary>
        /// <param name="buff">The BuffBase class</param>
        /// <param name="contentPack">Your Mod's cntent pack</param>
        /// <param name="buffDictionary">Optional, a Dictionary for getting a BuffBase by feeding it the corresponding BuffDef.</param>
        public void AddBuff(BuffBase buff, SerializableContentPack contentPack, Dictionary<BuffDef, BuffBase> buffDictionary = null)
        {
            buff.Initialize();
            HG.ArrayUtils.ArrayAppend(ref contentPack.buffDefs, buff.BuffDef);
            MoonstormBuffs.Add(buff.BuffDef, buff);
            if (buffDictionary != null)
                buffDictionary.Add(buff.BuffDef, buff);
            MSULog.LogD($"Buff {buff.BuffDef} added to {contentPack.name}");
        }
        #endregion

        #region Hooks
        private static void OnBuffsChanged(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            orig(self, buffType, newCount);
            self.GetComponent<MoonstormItemManager>()?.CheckForBuffs();
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
