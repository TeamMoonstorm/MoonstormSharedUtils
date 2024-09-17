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
        private static bool _hooked = false;

        public static void AddSpriteAnimation(SimpleSpriteAnimation animation)
        {
            HookIfNeeded();

            _animations.Add(animation);
        }

        private static void HookIfNeeded() 
        {
            if (_hooked)
                return;

            On.RoR2.PickRandomObjectOnAwake.Awake += AddSpriteAnimations;
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