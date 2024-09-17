using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using System;
using UnityEngine;

namespace MSU
{
    public static class LoadingScreenSpriteUtility
    {
        private static List<SimpleSpriteAnimation> _animations = new List<SimpleSpriteAnimation>();
        private static HashSet<AssetBundle> _bundles;
        private static bool _hooked = false;
        private static bool _alreadyPastLoadingScreen;

        public static void AddSpriteAnimations(AssetBundle bundleLoadedOnAwake)
        {
            if (_alreadyPastLoadingScreen)
            {
                MSULog.Info("Too late! we're already past the loading screen...");
                return;
            }

            HookIfNeeded();
            _bundles.Add(bundleLoadedOnAwake);
            foreach(var ssa in bundleLoadedOnAwake.LoadAllAssets<SimpleSpriteAnimation>())
            {
                _animations.Add(ssa);
            }
        }

        public static void AddSpriteAnimation(SimpleSpriteAnimation animation, AssetBundle parentBundle)
        {

            if (_alreadyPastLoadingScreen)
            {
                MSULog.Info("Too late! we're already past the loading screen...");
                return;
            }

            HookIfNeeded();

            _bundles.Add(parentBundle);
            _animations.Add(animation);
        }

        private static void HookIfNeeded() 
        {
            if (_hooked)
                return;

            _hooked = true;
            On.RoR2.PickRandomObjectOnAwake.Awake += AddSpriteAnimations;
            On.RoR2.UI.MainMenu.MainMenuController.Awake += UnhookAndUnload;
        }

        private static void UnhookAndUnload(On.RoR2.UI.MainMenu.MainMenuController.orig_Awake orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            _alreadyPastLoadingScreen = true;
            _hooked = false;
            On.RoR2.PickRandomObjectOnAwake.Awake -= AddSpriteAnimations;
            On.RoR2.UI.MainMenu.MainMenuController.Awake -= UnhookAndUnload;

            foreach(var bundle in _bundles)
            {
                bundle.Unload(true);
            }
        }

        private static void AddSpriteAnimations(On.RoR2.PickRandomObjectOnAwake.orig_Awake orig, PickRandomObjectOnAwake self)
        {
            if (self.gameObject.name != "MiniScene")
                goto callOrig;

            Transform commandoWalk = self.transform.Find("CommandoWalk");

            if (!commandoWalk)
                goto callOrig;

            foreach(var anim in _animations)
            {
                try
                {
                    GameObject copyOfCommandoWalk = UnityEngine.Object.Instantiate(commandoWalk.gameObject);
                    Transform copyOfCommandoWalkTransform = copyOfCommandoWalk.transform;

                    copyOfCommandoWalkTransform.SetParent(self.transform);

                    Transform spriteTransform = copyOfCommandoWalkTransform.Find("MonsterSprite");
                    SimpleSpriteAnimator animator = spriteTransform.GetComponent<SimpleSpriteAnimator>();

                    animator.animation = anim;

                    HG.ArrayUtils.ArrayAppend(ref self.ObjectsToSelect, copyOfCommandoWalk);
                }
                catch(Exception e)
                {
                    MSULog.Error($"Failed to add sprite animation for {anim}.\n{e}");
                }
            }
        callOrig:
            orig(self);
        }

    }
}