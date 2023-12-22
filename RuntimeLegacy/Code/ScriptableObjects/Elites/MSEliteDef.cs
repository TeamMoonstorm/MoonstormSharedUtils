using RoR2;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="MSEliteDef"/> is an extension of <see cref="EliteDef"/> that allows the easy implementation of an elite to vanilla tiers, ramps, overlay effects and visual effects
    /// </summary>
    public class MSEliteDef : EliteDef
    {
        [Tooltip("The vanilla tier of the elite. Leave this \"None\" if you use SerializableEliteTierDef for this eliteDef or if this elite has special spawning systems.")]
        public VanillaEliteTier eliteTier;

        [Tooltip("The color of the light for this elite")]
        public Color lightColor = Color.clear;

        [Tooltip("The color ramp of the elite.")]
        public Texture2D eliteRamp;

        [Tooltip("The overlay material of the elite, used mostly for post loop elites.")]
        public Material overlay;

        [Tooltip("Effect thats spawned once the elite spawns.")]
        public GameObject effect;
    }

    /// <summary>
    /// Represents the base game's elite tiers
    /// </summary>
    public enum VanillaEliteTier
    {
        /// <summary>
        /// This elite will not be added to any tiers
        /// </summary>
        None = 0,
        /// <summary>
        /// The tier thats normally used for non artifact runs, includes Blazing, Overloading, Mending and Glacial elites
        /// </summary>
        HonorDisabled = 1,
        /// <summary>
        /// The tier that's used when the Artifact of Honor is active, includes weaker versions of Blazing, Overloading, Mending and Glacial elites
        /// </summary>
        HonorActive = 2,
        /// <summary>
        /// The tier that's used when the first loop is complete, includes Malachite and Celestine elites
        /// </summary>
        PostLoop = 4,
        /// <summary>
        /// The tier that's used in Commencement, includes the Perfected elite
        /// </summary>
        Lunar = 8,
    }
}