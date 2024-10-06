using RoR2;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// Class used to create new Material Overlays for BuffDefs
    /// </summary>
    public static class BuffOverlays
    {
        /// <summary>
        /// A read only dictionary of BuffDef to Material. These materials are later applied as Overlays to CharacterBodies when they have the BuffDef
        /// </summary>
        public static ReadOnlyDictionary<BuffIndex, Material> buffOverlayDictionary { get; private set; }
        private static Dictionary<BuffDef, Material> _buffOverlays = new Dictionary<BuffDef, Material>();

        /// <summary>
        /// Wether the BuffOverlayDictionary has been created or not.
        /// </summary>
        public static bool dictionaryCreated { get; private set; } = false;

        [SystemInitializer(typeof(BuffCatalog))]
        private static IEnumerator SystemInit()
        {
            dictionaryCreated = true;
            MSULog.Info("Initializing Buff Overlays...");

            Dictionary<BuffIndex, Material> readOnlyBase = new Dictionary<BuffIndex, Material>();
            foreach (var (bd, material) in _buffOverlays)
            {
                yield return new WaitForEndOfFrame();
                if (bd.buffIndex == BuffIndex.None)
                    continue;

                readOnlyBase[bd.buffIndex] = material;
            }
            buffOverlayDictionary = new ReadOnlyDictionary<BuffIndex, Material>(readOnlyBase);

            if(buffOverlayDictionary.Count == 0)
            {
#if DEBUG
                MSULog.Info("Not doing BuffOverlays hooks since there are no buff overlays registered.");
#endif
                yield break;
            }

            On.RoR2.CharacterModel.UpdateOverlays += AddBuffOverlay;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += ForceUpdateIfNeeded;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += ForceUpdateOnBuffFinalStackLostIfNeeded;
        }

        private static void ForceUpdateOnBuffFinalStackLostIfNeeded(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            if (buffOverlayDictionary.ContainsKey(buffDef.buffIndex) && self.modelLocator && self.modelLocator.modelTransform && self.modelLocator.modelTransform.TryGetComponent<CharacterModel>(out var mdl))
            {
                mdl.forceUpdate = true;
            }
        }

        private static void ForceUpdateIfNeeded(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            if (buffOverlayDictionary.ContainsKey(buffDef.buffIndex) && self.modelLocator && self.modelLocator.modelTransform && self.modelLocator.modelTransform.TryGetComponent<CharacterModel>(out var mdl))
            {
                mdl.forceUpdate = true;
            }
        }

        /// <summary>
        /// Adds a new Buff Material pair to the Overlays system
        /// </summary>
        /// <param name="def">The BuffDef that will have a new Overlay</param>
        /// <param name="material">The Material for the BuffDef</param>
        public static void AddBuffOverlay(BuffDef def, Material material)
        {
            if (dictionaryCreated)
            {
#if DEBUG
                MSULog.Info("Buff Overlay Dictionary already created.");
#endif
                return;
            }

            if (!def)
            {
#if DEBUG
                MSULog.Warning($"BuffDef is null for overlay with material {material}");
#endif
                return;
            }

            if (!material)
            {
#if DEBUG
                MSULog.Warning($"Material is null for buff def {def}");
#endif
                return;
            }

            if (_buffOverlays.ContainsKey(def))
            {
#if DEBUG
                MSULog.Info($"The BuffDef {def} already has an overlay material assigned. (Material={_buffOverlays[def]})");
#endif
                return;
            }
            _buffOverlays.Add(def, material);
        }

        private static void AddBuffOverlay(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
        {
            orig(self);

            if (!self.body)
                return;

            foreach (var (buff, material) in buffOverlayDictionary)
            {
                if (self.body.HasBuff(buff))
                    AddOverlay(self, material);
            }
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
    }
}