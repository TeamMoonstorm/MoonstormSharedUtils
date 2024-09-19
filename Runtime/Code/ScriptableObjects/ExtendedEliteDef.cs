using RoR2;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// An ExtendedEliteDef is an extension of <see cref="EliteDef"/> which allows for more complex behaviours and systems tied to Elites. This class allows you to easily assign an EliteDef to a specific EliteTier in <see cref="CombatDirector.eliteTiers"/>, a custom texture ramp to apply to the Elite, an Overlay material that'll be applied to the elite, and an optional effect to spawn around the eliteDef.
    /// <br>This in turn allows you to easily implement elites that fit into the existing EliteTiers of Lunar, Tier 1 and Tier 2</br>
    /// </summary>
    [CreateAssetMenu(fileName = "New ExtendedEliteDef", menuName = "MSU/ExtendedEliteDef")]
    public class ExtendedEliteDef : EliteDef
    {
        [Tooltip("The tiers of this Elite, leave this to \"None\" if your Elite is part of a non vanilla tier.")]
        public VanillaTier eliteTier;

        [Tooltip("A texture ramp for this elite, which will be applied to its model.")]
        public Texture2D eliteRamp;

        [Tooltip("An overlay material for this elite, which will be applied to it's model.")]
        public Material overlayMaterial;

        [Tooltip("An effect to spawn on the Elite.")]
        public GameObject effect;

        /// <summary>
        /// Represents all of the game's vanilla elite tiers.
        /// </summary>
        public enum VanillaTier
        {
            /// <summary>
            /// None - The elite is not added to any vanilla tiers, useful if you're adding it to a custom tier.
            /// </summary>
            None = 0,

            /// <summary>
            /// The elite is added to the "Tier 1" of elites, which include blazing, mending, overloading and glacial
            /// </summary>
            HonorDisabled = 1,

            /// <summary>
            /// The elite is added to ther "Tier 1" of elites used when the Artifact of Honor is active.
            /// </summary>
            HonorActive = 2,

            /// <summary>
            /// The elite is added to the "Tier 2" of elites, which include Malachite and Celestine elites
            /// </summary>
            PostLoop = 3,

            /// <summary>
            /// The elite is added to the "Lunar" tier of elites, which only includes the Perfected elites.
            /// </summary>
            Lunar = 4
        }
    }
}
