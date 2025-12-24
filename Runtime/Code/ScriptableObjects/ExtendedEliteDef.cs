using RoR2;
using System;
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
        [Header("Extended Elite Def specifics")]
        [Tooltip("An enum utilized to add this EliteDef to a vanilla game, each entry represents a different Tier in-game. If you need to handle the addition in a more specific manner, set this to none and add the tier manually on your IEliteContentPiece\n\n" +
            "None: No tier, this eltie won't be added to the vanilla tiers\n" +
            "Tier1: The Tier 1, includes Mending, Blazing, Glacial and Overloading\n" +
            "Tier1_5: Same as Tier 1, but also includes Gilded elites\n" +
            "GlobalTier1: A special entry that adds this elite to both Tier1 and Tier1_5\n" +
            "Tier1Honor: The Tier 1 utilized in the Artifact of Honor, includes weaker versions of Mending, Blazing, Glacial and Overloading" +
            "Tier1_5Honor: Same as Tier1Honor, but also includes Honor Gilded Elites\n" +
            "GlobalTier1Honor: A special entry that adds this elite to both Tier1Honor and Tier1_5Honor\n" +
            "Tier2: The Tier that contains Malachite, Celestine, Twisted and Collective elites\n" +
            "Lunar: The Tier that contains the Perfected elite.")]
        public VanillaEliteTierEntry vanillaEliteTier;

        [Tooltip("A texture ramp for this elite, which will be applied to its model.")]
        public Texture2D eliteRamp;

        [Tooltip("An overlay material for this elite, which will be applied to it's model.")]
        public Material overlayMaterial;

        [Tooltip("If true, The color associated to this elite will be applied to the character's lights.")]
        public bool applyLightColorOverrides;

        [Tooltip("A Material that will be used to replace particle system's materials.")]
        public Material particleReplacementMaterial;

        [Tooltip("An effect to spawn on the Elite.")]
        public GameObject effect;

        [Header("Deprecated, check Context Menus for Upgrading.")]
        [Obsolete("Utilize eliteTierFlags instead.")]
        public VanillaTier eliteTier;

        [Obsolete("Utilize VanillaEliteTierEntry instead.")]
        public enum VanillaTier
        {
            None = 0,

            HonorDisabled = 1,

            HonorActive = 2,

            PostLoop = 3,

            Lunar = 4
        }

        /// <summary>
        /// An enum representing the base game's Elite Tiers
        /// </summary>
        public enum VanillaEliteTierEntry
        {
            /// <summary>
            /// No tier, this elite WON'T be added to the Director's elite tiers
            /// </summary>
            None = 0,
            
            /// <summary>
            /// Tier which includes Mending, Overloading, Glacial and Blazing elites
            /// </summary>
            Tier1 = 1,

            /// <summary>
            /// Same as Tier 1, but includes the Gilded elites
            /// </summary>
            Tier1_5 = 2,

            /// <summary>
            /// A special entry that's used to add an Elite to both Tier1 and Tier1_5
            /// </summary>
            GlobalTier1 = 3,

            /// <summary>
            /// Tier which includes the Artifact of Honor versions of Mending, Overloading, Glacial and Blazing elites
            /// </summary>
            Tier1Honor = 4,

            /// <summary>
            /// Same as Tier1Honor, but includes the Honor version of the Gilded elites
            /// </summary>
            Tier1_5Honor = 5,

            /// <summary>
            /// A special entry that's used to add an Elite to both Tier1Honor and Tier1_5Honor
            /// </summary>
            GlobalTier1Honor = 6,

            /// <summary>
            /// Tier which includes Malachite, Celestine, Twisted and Collective elites
            /// </summary>
            Tier2 = 7,

            /// <summary>
            /// Tier which includes Perfected elites
            /// </summary>
            Lunar = 8
        }

        [ContextMenu("Upgrade to VanillaEliteTierEntry")]
        private void UpgradeToVanillaEliteTierEntry()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            vanillaEliteTier = eliteTier switch
            {
                VanillaTier.None => VanillaEliteTierEntry.None,
                VanillaTier.HonorDisabled => VanillaEliteTierEntry.GlobalTier1,
                VanillaTier.HonorActive => VanillaEliteTierEntry.GlobalTier1Honor,
                VanillaTier.PostLoop => VanillaEliteTierEntry.Tier2,
                VanillaTier.Lunar => VanillaEliteTierEntry.Lunar,
                _ => VanillaEliteTierEntry.None
            };

            eliteTier = VanillaTier.None;
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
