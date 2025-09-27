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
        public VanillaTierFlags eliteTierFlags;

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

        [Obsolete("Utilize VanillaTierFlags instead.")]
        public enum VanillaTier
        {
            None = 0,

            HonorDisabled = 1,

            HonorActive = 2,

            PostLoop = 3,

            Lunar = 4
        }

        [Flags]
        public enum VanillaTierFlags : uint
        {
            None = 0u,
            Tier1 = 1u,
            Tier1Honor = 2u,
            Tier1_5 = 4u,
            Tier1_5Honor = 8u,
            Tier2 = 16u,
            Lunar = 32u,

            GlobalTier1 = Tier1 | Tier1_5,
            GlobalTier1Honor = Tier1Honor | Tier1_5Honor
        }

        [ContextMenu("Upgrade to Flags")]
        private void UpgradeToFlags()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            switch (eliteTier)
            {
                case VanillaTier.None:
                    break;
                case VanillaTier.HonorDisabled:
                    eliteTierFlags = VanillaTierFlags.GlobalTier1;
                    break;
                case VanillaTier.HonorActive:
                    eliteTierFlags = VanillaTierFlags.GlobalTier1Honor;
                    break;
                case VanillaTier.PostLoop:
                    eliteTierFlags = VanillaTierFlags.Tier2;
                    break;
                case VanillaTier.Lunar:
                    eliteTierFlags = VanillaTierFlags.Lunar;
                    break;
            }

            if (eliteTier != VanillaTier.None)
            {
                eliteTier = VanillaTier.None;
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
