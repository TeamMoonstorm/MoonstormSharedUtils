using RoR2;
using System;
using UnityEngine;

namespace Moonstorm
{
    [Obsolete]
    public class MSEliteDef : EliteDef
    {
        public VanillaEliteTier eliteTier;

        public Color lightColor = Color.clear;

        public Texture2D eliteRamp;

        public Material overlay;

        public GameObject effect;
    }

    [Obsolete]
    public enum VanillaEliteTier
    {
        None = 0,
        HonorDisabled = 1,
        /// </summary>
        HonorActive = 2,
        PostLoop = 4,
        Lunar = 8,
    }
}