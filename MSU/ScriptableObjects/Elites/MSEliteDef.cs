using RoR2;
using UnityEngine;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New MSEliteDef", menuName = "Moonstorm/Elites/MSEliteDef", order = 0)]
    public class MSEliteDef : EliteDef
    {
        [Tooltip("The tier of the elite. Choose other if your Elite has its own tier or a different spawning system.")]
        public EliteTiers eliteTier;
        public Color lightColor = Color.clear;
        [Tooltip("The color ramp of the elite.")]
        public Texture eliteRamp;
        [Tooltip("The overlay material of the elite, used mostly for post loop elites.")]
        public Material overlay;
        [Tooltip("Effect thats spawned once the elite spawns.")]
        public GameObject effect;
    }
    public enum EliteTiers
    {
        Basic,
        PostLoop,
        Other
    }
}