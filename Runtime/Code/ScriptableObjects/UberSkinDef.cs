using MSU.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using R2API.AddressReferencedAssets;
using RoR2BepInExPack.GameAssetPaths;
using R2API;

namespace MSU
{
    /// <summary>
    /// A Custom ScriptableObject that's used to both interface with R2API's Skins submodule, and create new skins for vanilla characters.
    /// </summary>
    [CreateAssetMenu(menuName = "MSU/UberSkinDef")]
    public class UberSkinDef : ScriptableObject
    {
        #region SubTypes
        [Serializable]
        public struct RendererInfo
        {
            public PrefabReferenceOrTransformPath<Renderer> renderer;
            public Material defaultMaterial;
            public ShadowCastingMode defaultShadowCastingMode;
            public bool ignoreOverlays;
            public bool hideOnDeath;
            public bool ignoresMaterialOverrides;
        }

        [Serializable]
        public struct GameObjectActivation
        {
            public bool spawnPrefabOnModelObject;
            public PrefabReferenceOrTransformPath<GameObject> gameObject;
            public bool shouldActivate;
            public GameObject prefab;
            public Vector3 localPosition;
            public Vector3 localRotation;
            public Vector3 localScale;
        }

        [Serializable]
        public struct MeshReplacement
        {
            public PrefabReferenceOrTransformPath<Renderer> renderer;
            public Mesh mesh;
        }

        [Serializable]
        public struct ProjectileGhostReplacement
        {
            public AddressReferencedPrefab projectilePrefab;
            public GameObject projectileGhostReplacementPrefab;
        }

        [Serializable]
        public struct MinionSkinReplacement
        {
            public AddressReferencedPrefab minionBodyPrefab;
            public SkinDef minionSkin;
        }

        [Serializable]
        public struct LightInfo
        {
            public PrefabReferenceOrTransformPath<Light> light;
            public Color lightColor;
        }


        [Serializable]
        public struct VFXOverride
        {
            public AddressReferencedPrefab targetEffect;
            public GameObject replacementEffectPrefab;

            public SkinVFX.OnEffectSpawnedDelegate onEffectSpawned;
            [SerializableStaticMethod.RequiredArguments(typeof(void), typeof(GameObject))]
            public SerializableStaticMethod serializedOnEffectSpawned;
        }

        [Serializable]
        public struct PrefabReferenceOrTransformPath<T> where T : UnityEngine.Object
        {
            [PrefabReference]
            public T reference;
            [TransformPath(nameof(targetObject))]
            public string transformPath;
        }
        #endregion


        public SkinDef targetSkinDef;
        public SkinDefParams targetSkinDefParams;

        public AddressReferencedSkinDef[] baseSkins;
        public AddressReferencedPrefab targetObject;

        public RendererInfo[] rendererInfos = Array.Empty<RendererInfo>();
        public GameObjectActivation[] gameObjectActivations = Array.Empty<GameObjectActivation>();
        public MeshReplacement[] meshReplacements = Array.Empty<MeshReplacement>();
        public ProjectileGhostReplacement[] projectileGhostReplacements = Array.Empty<ProjectileGhostReplacement>();
        public MinionSkinReplacement[] minionSkinReplacements = Array.Empty<MinionSkinReplacement>();
        public LightInfo[] lightInfos = Array.Empty<LightInfo>();
        public VFXOverride[] vfxOverrides = Array.Empty<VFXOverride>();

        [SerializableStaticMethod.MethodDetector]
        private static void Test(GameObject instance)
        {

        }
    }

    [Serializable]
    public struct SerializableStaticMethod
    {
        public string typeName;
        public string methodName;
        public class RequiredArguments : PropertyAttribute
        {
            public Type returnType;
            public Type[] arguments;
            public RequiredArguments(Type returnType, params Type[] arguments)
            {
                this.returnType = returnType;
                this.arguments = arguments;
            }
        }

        /// <summary>
        /// This is used in combination with the TypeCache to easily find methods.
        /// </summary>
        public class MethodDetector : Attribute
        {

        }
    }


    public class TransformPath : PropertyAttribute
    {
        public string rootObjectProperty { get; }

        public TransformPath(string rootObjectProperty)
        {
            this.rootObjectProperty = rootObjectProperty;
        }
    }
}
