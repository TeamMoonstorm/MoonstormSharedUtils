using RoR2;
using UnityEngine;

namespace Moonstorm
{
    public class MSEliteDef : EliteDef
    {
        public VanillaEliteTier eliteTier;

        public Color lightColor = Color.clear;

        public Texture2D eliteRamp;

        public Material overlay;

        public GameObject effect;
    }

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