using RoR2;
using System;
using UnityEngine;

namespace Moonstorm
{
    [Obsolete("VanillaSkinDefinition is obsolete and causes issues at load, utilize VanillaSkinDefinition instead")]
    public class VanillaSkinDef : SkinDef
    {
        public CustomGameObjectActivation[] customGameObjectActivations = Array.Empty<CustomGameObjectActivation>();
        public VanillaProjectileGhostReplacement[] vanillaProjectileGhostReplacements = Array.Empty<VanillaProjectileGhostReplacement>();
        public VanillaMinionSkinReplacement[] vanillaMinionSkinReplacements = Array.Empty<VanillaMinionSkinReplacement>();

        [Space]
        [Tooltip("e.g. \"Toolbot\"")]
        public string bodyResourcePathKeyword;

        [Serializable]
        public struct CustomGameObjectActivation
        {
            public GameObject gameObjectPrefab;
            public string childName;
            public bool shouldActivate;
        }

        [Serializable]
        public struct VanillaProjectileGhostReplacement
        {
            public string projectilePrefabPath;
            public GameObject projectileGhostReplacementPrefab;
        }

        [Serializable]
        public struct VanillaMinionSkinReplacement
        {
            public string minionBodyPrefabResourcePath;

            public SkinDef minionSkin;
        }
    }
}