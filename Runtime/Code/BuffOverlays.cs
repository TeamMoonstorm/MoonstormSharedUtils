using RoR2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

namespace MSU
{
    public static class BuffOverlays
    {
        public static ReadOnlyDictionary<BuffDef, Material> BuffOverlayDictionary { get; private set; }
        internal static Dictionary<BuffDef, Material> _buffOverlays = new Dictionary<BuffDef, Material>();

        public static bool DictionaryCreated { get; private set; } = false;

        [SystemInitializer(typeof(BuffCatalog))]
        private static void SystemInit()
        {
            DictionaryCreated = true;
            MSULog.Info("Initializing Buff Overlays...");
            On.RoR2.CharacterModel.UpdateOverlays += AddBuffOverlay;

            BuffOverlayDictionary = new ReadOnlyDictionary<BuffDef, Material>(_buffOverlays);
            _buffOverlays = null;
        }

        public static void AddBuffOverlay(BuffDef def, Material material)
        {
            if (DictionaryCreated)
            {
#if DEBUG
                MSULog.Info("Buff Overlay Dictionary already created.");
                return;
#endif
            }    

            if(_buffOverlays.ContainsKey(def))
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
            foreach(var (buff, material) in BuffOverlayDictionary)
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