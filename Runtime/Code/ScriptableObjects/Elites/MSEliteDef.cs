using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New MSEliteDef", menuName = "Moonstorm/Elites/MSEliteDef", order = 0)]
    public class MSEliteDef : EliteDef
    {
        [Tooltip("The vanilla tier of the elite. Leave this \"None\" if you use SerializableEliteTierDef for this eliteDef or if this elite has special spawning systems.")]
        public VanillaEliteTier eliteTier;
        public Color lightColor = Color.clear;
        [Tooltip("The color ramp of the elite.")]
        public Texture2D eliteRamp;
        [Tooltip("The overlay material of the elite, used mostly for post loop elites.")]
        public Material overlay;
        [Tooltip("Effect thats spawned once the elite spawns.")]
        public GameObject effect;
    }

    public enum VanillaEliteTier
    {
        None = 0,
        HonorDisabled = 1,
        HonorActive = 2,
        PostLoop = 4,
        Lunar = 8,
    }
}