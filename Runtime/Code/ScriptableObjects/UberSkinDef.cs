using MSU.AddressReferencedAssets;
using RoR2;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using R2API.AddressReferencedAssets;
using R2API;
using System.Reflection;

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
            [TransformPath(nameof(targetObject), rootComponentType = typeof(CharacterModel), siblingPropertyComponentTypeRequirement = nameof(reference), allowSelectingRoot = false)]
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
    }

    [Serializable]
    public struct SerializableStaticMethod
    {
        public string assemblyQualifiedTypeName;
        public string methodName;

        public bool TryGetMethod(out MethodInfo methodInfo)
        {
            Type t = Type.GetType(assemblyQualifiedTypeName);
            methodInfo = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            return methodInfo != null;
        }

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

    /// <summary>
    /// Used to display a string as a transform path
    /// </summary>
    public class TransformPathAttribute : PropertyAttribute
    {
        /// <summary>
        /// The Root Object from which the paths are calculated.
        /// <br></br>
        /// Supports both a GameObject, an AssetReferenceT{GameObject} and a AdressReferencedPrefab
        /// </summary>
        public string rootObjectProperty { get; }

        /// <summary>
        /// The name of a sibling property that can be used to inferr a required type within the RootTransform's childrens
        /// </summary>
        public string siblingPropertyComponentTypeRequirement { get; set; }

        /// <summary>
        /// If specified, the RootTransform will be the _first_ object where this component is found.
        /// </summary>
        public Type rootComponentType { get; set; }

        public bool allowSelectingRoot { get; set; }

        public TransformPathAttribute(string rootObjectProperty)
        {
            this.rootObjectProperty = rootObjectProperty;
        }
    }
}
