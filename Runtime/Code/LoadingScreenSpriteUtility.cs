using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// Utility class for adding new Sprite Animations to the Loading Screen
    /// </summary>
    public static class LoadingScreenSpriteUtility
    {
        private static List<SimpleSpriteAnimation> _anims = new List<SimpleSpriteAnimation>();
        private static HashSet<AssetBundle> _bundles = new HashSet<AssetBundle>();

        private static bool _hooked = false;
        private static bool _alreadyPastLoadingScreen = false;
        private static GameObject _walkPrefab;

        /// <summary>
        /// Adds all the <see cref="SimpleSpriteAnimation"/>s found within <paramref name="bundleLoadedOnAwake"/> to the loading screen.
        /// 
        /// <para>Due to needing the bundle in <paramref name="bundleLoadedOnAwake"/> to be loaded on Awake time, MSU will UNLOAD the passed bundle once the title screen is reached, its recommended that you use a separate bundle EXCLUSIVELY for these SimpleSpriteAnimations.</para>
        /// </summary>
        /// <param name="bundleLoadedOnAwake">The bundle that contains the sprite animations, and which will be unloaded once the main menu appears</param>
        public static void AddSpriteAnimations(AssetBundle bundleLoadedOnAwake)
        {
            if (_alreadyPastLoadingScreen)
            {
                MSULog.Info("Too late! we're already past the loading screen...");
                return;
            }

            HookIfNeeded();
            _bundles.Add(bundleLoadedOnAwake);
            foreach (var ssa in bundleLoadedOnAwake.LoadAllAssets<SimpleSpriteAnimation>())
            {
                _anims.Add(ssa);
            }
        }

        /// <summary>
        /// Adds the specified <see cref="SimpleSpriteAnimation"/> in <paramref name="animation"/> to the loading screen.
        /// 
        /// <para>Due to needing the bundle in <paramref name="parentBundle"/> to be loaded on Awake time, MSU will UNLOAD the passed bundle once the title screen is reached, its recommended that you use a separate bundle EXCLUSIVELY for these SimpleSpriteAnimations.</para>
        /// </summary>
        /// <param name="animation">The sprite animation to add</param>
        /// <param name="parentBundle">The bundle from which <paramref name="animation"/> was loaded. this bundle will be unloaded once the main menu appears.</param>
        public static void AddSpriteAnimation(SimpleSpriteAnimation animation, AssetBundle parentBundle)
        {

            if (_alreadyPastLoadingScreen)
            {
                MSULog.Info("Too late! we're already past the loading screen...");
                return;
            }

            HookIfNeeded();

            _bundles.Add(parentBundle);
            _anims.Add(animation);
        }

        /// <summary>
        /// Adds the specified <see cref="SimpleSpriteAnimation"/> in <paramref name="animation"/> to the loading screen.
        /// 
        /// <para>The SimpleSpriteAnimation will be DESTROYED once the loading screen is finished.</para>
        /// </summary>
        /// <param name="animation">The sprite animation to add</param>
        /// <param name="parentBundle">The bundle from which <paramref name="animation"/> was loaded. this bundle will be unloaded once the main menu appears.</param>
        public static void AddSpriteAnimation(SimpleSpriteAnimation animation)
        {
            if (_alreadyPastLoadingScreen)
            {
                MSULog.Info("Too late! we're already past the loading screen...");
                return;
            }

            HookIfNeeded();

            _anims.Add(animation);
        }

        private static void HookIfNeeded()
        {
            if (_hooked)
                return;

            _hooked = true;
            On.RoR2.PickRandomObjectOnAwake.Awake += AddSpriteAnimations;
#if !DEBUG
            On.RoR2.UI.MainMenu.MainMenuController.Awake += UnhookAndUnload;
#endif
            _walkPrefab = MSUMain.msuAssetBundle.LoadAsset<GameObject>("CustomSpriteWalk");
        }

        private static void UnhookAndUnload(On.RoR2.UI.MainMenu.MainMenuController.orig_Awake orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            _alreadyPastLoadingScreen = true;
            _hooked = false;
            On.RoR2.PickRandomObjectOnAwake.Awake -= AddSpriteAnimations;
            On.RoR2.UI.MainMenu.MainMenuController.Awake -= UnhookAndUnload;
            _walkPrefab = null;

            foreach(var anim in _anims)
            {
                UnityEngine.Object.Destroy(anim);
            }
            _anims.Clear();
            foreach (var bundle in _bundles)
            {
                if(bundle)
                    bundle.Unload(true);
            }
            _bundles.Clear();
        }

        private static void AddSpriteAnimations(On.RoR2.PickRandomObjectOnAwake.orig_Awake orig, PickRandomObjectOnAwake self)
        {
            if (self.gameObject.name != "MiniScene")
                goto callOrig;

            foreach (var anim in _anims)
            {
                try
                {
                    var instance = GameObject.Instantiate(_walkPrefab, self.transform);

                    instance.name = $"{anim}Animator";
                    var animator = instance.GetComponentInChildren<SimpleSpriteAnimator>();
                    animator.animation = anim;
                    animator.target.sprite = anim.frames.FirstOrDefault().sprite;

                    HG.ArrayUtils.ArrayAppend(ref self.ObjectsToSelect, instance);
                }
                catch (Exception e)
                {
                    MSULog.Error($"Failed to add sprite animation for {anim}.\n{e}");
                }
            }
        callOrig:
            orig(self);
        }

    }
}