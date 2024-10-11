using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSU
{
    [Obsolete("Interact with the sub-dependency \"LoadingScreenFix\" directly.")]
    public static class LoadingScreenSpriteUtility
    {
        public static void AddSpriteAnimations(AssetBundle bundleLoadedOnAwake)
        {
            LoadingScreenFix.LoadingScreenFix.AddSpriteAnimations(bundleLoadedOnAwake);
        }

        public static void AddSpriteAnimation(SimpleSpriteAnimation animation, AssetBundle parentBundle)
        {
            LoadingScreenFix.LoadingScreenFix.AddSpriteAnimation(animation, parentBundle);
        }

        public static void AddSpriteAnimation(SimpleSpriteAnimation animation)
        {
            LoadingScreenFix.LoadingScreenFix.AddSpriteAnimation(animation);
        }
    }
}