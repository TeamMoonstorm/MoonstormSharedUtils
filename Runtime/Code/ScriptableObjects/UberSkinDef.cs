using MSU.AddressReferencedAssets;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using R2API.AddressReferencedAssets;
using R2API;
using System.Reflection;
using System.Collections;

namespace MSU
{
    /// <summary>
    /// A Custom ScriptableObject which is meant to be an all in one solution for Skins.
    /// <para></para>
    /// The UberSkinDef can be utilized either as a metadata for a custom character's skin, or for the metadata for a vanilla character's skin.
    /// <br></br>
    /// It comes built in with R2API.Skins' SkinVFX Info, allowing you to serialize VFX replacements based on skins.
    /// </summary>
    [CreateAssetMenu(menuName = "MSU/UberSkinDef")]
    public class UberSkinDef : ScriptableObject
    {
        #region SubTypes
        /// <summary>
        /// UberSkinDef representation of a <see cref="CharacterModel.RendererInfo"/>
        /// </summary>
        [Serializable]
        public struct RendererInfo
        {
            /// <summary>
            /// A Reference to the Renderer, or a TransformPath to the Renderer.
            /// <br></br>
            /// If the UberSkinDef's targetObject is a direct reference, this should point to a direct reference of the prefab itself. Otherwise it should point to a Transform reference utilizing a transform path.
            /// </summary>
            public PrefabReferenceOrTransformPath<Renderer> renderer;

            /// <summary>
            /// The default material to use for this renderer
            /// </summary>
            public Material defaultMaterial;

            /// <summary>
            /// Should this renderer cast shadows?
            /// </summary>
            public ShadowCastingMode defaultShadowCastingMode;

            /// <summary>
            /// Wether this renderer ignores overlays
            /// </summary>
            public bool ignoreOverlays;

            /// <summary>
            /// Wether this renderer is hidden on death
            /// </summary>
            public bool hideOnDeath;

            /// <summary>
            /// Wether this renderer ignores material overrides
            /// </summary>
            public bool ignoresMaterialOverrides;

            /// <summary>
            /// Creates a <see cref="CharacterModel.RendererInfo"/> based off the internal metadata and the root object passed in by <paramref name="rootObject"/>
            /// </summary>
            /// <param name="rootObject">The root object itself</param>
            /// <returns>The computed <see cref="CharacterModel.RendererInfo"/></returns>
            public CharacterModel.RendererInfo ToCharacterModelRendererInfo(Transform rootObject)
            {
                var result = new CharacterModel.RendererInfo();
                result.defaultMaterial = defaultMaterial;
                result.defaultShadowCastingMode = defaultShadowCastingMode;
                result.ignoreOverlays = ignoreOverlays;
                result.hideOnDeath = hideOnDeath;
                result.ignoresMaterialOverrides = ignoresMaterialOverrides;

                Renderer resultRenderer = renderer.ResolveReference(rootObject);
                result.renderer = resultRenderer;
                return result;
            }
        }

        /// <summary>
        /// UberSkinDef representation of a <see cref="SkinDefParams.GameObjectActivation"/>
        /// </summary>
        [Serializable]
        public struct GameObjectActivation
        {
            /// <summary>
            /// Wether we should spawn a prefab on the referenced model object
            /// </summary>
            public bool spawnPrefabOnModelObject;

            /// <summary>
            /// A Reference to the transform that's being activated, or a TransformPath to the transform itself..
            /// <br></br>
            /// If the UberSkinDef's targetObject is a direct reference, this should point to a direct reference of the prefab itself. Otherwise it should point to a Transform reference utilizing a transform path.
            /// </summary>
            public PrefabReferenceOrTransformPath<Transform> gameObject;

            /// <summary>
            /// Wether the GameObject should be active, only valid of <see cref="spawnPrefabOnModelObject"/> is false
            /// </summary>
            public bool shouldActivate;

            /// <summary>
            /// The Prefab to instantiate, only valid of <see cref="spawnPrefabOnModelObject"/> is true
            /// </summary>
            public GameObject prefab;

            /// <summary>
            /// The local position of the prefab, relative to <see cref="gameObject"/>, only valid of <see cref="spawnPrefabOnModelObject"/> is true
            /// </summary>
            public Vector3 localPosition;

            /// <summary>
            /// The local rotation of the prefab, relative to <see cref="gameObject, only valid of <see cref="spawnPrefabOnModelObject"/> is true
            /// </summary>
            public Vector3 localRotation;

            /// <summary>
            /// The local scale of the prefab, relative to <see cref="gameObject, only valid of <see cref="spawnPrefabOnModelObject"/> is true
            /// </summary>
            public Vector3 localScale;

            /// <summary>
            /// Creates a <see cref="SkinDefParams.GameObjectActivation"/> based off the internal metadata and the root object passed in by <paramref name="rootObject"/>
            /// </summary>
            /// <param name="rootObject">The root object itself</param>
            /// <returns>The computed <see cref="SkinDefParams.GameObjectActivation"/></returns>
            public SkinDefParams.GameObjectActivation ToSkinDefParamsGameObjectActivation(Transform rootObject)
            {
                var result = new SkinDefParams.GameObjectActivation();
                result.spawnPrefabOnModelObject = spawnPrefabOnModelObject;
                if (spawnPrefabOnModelObject)
                {
                    result.prefab = prefab;
                    result.localPosition = localPosition;
                    result.localRotation = localRotation;
                    result.localScale = localScale;
                }
                else
                {
                    result.shouldActivate = shouldActivate;
                }

                GameObject resultGameObject = gameObject.ResolveReference(rootObject).gameObject;
                result.gameObject = resultGameObject;
                return result;
            }
        }

        /// <summary>
        /// UberSkinDef representation of a <see cref="SkinDefParams.MeshReplacement"/>
        /// </summary>
        [Serializable]
        public struct MeshReplacement
        {
            /// <summary>
            /// A Reference to the Renderer, or a TransformPath to the Renderer.
            /// <br></br>
            /// If the UberSkinDef's targetObject is a direct reference, this should point to a direct reference of the prefab itself. Otherwise it should point to a Transform reference utilizing a transform path.
            /// </summary>
            public PrefabReferenceOrTransformPath<Renderer> renderer;

            /// <summary>
            /// The mesh that the <see cref="renderer"/> should use
            /// </summary>
            public Mesh mesh;

            /// <summary>
            /// Creates a <see cref="SkinDefParams.MeshReplacement"/> based off the internal metadata and the root object passed in by <paramref name="rootObject"/>
            /// </summary>
            /// <param name="rootObject">The root object itself</param>
            /// <returns>The computed <see cref="SkinDefParams.MeshReplacement"/></returns>
            public SkinDefParams.MeshReplacement ToSkinDefParamsMeshReplacement(Transform rootObject)
            {
                var result = new SkinDefParams.MeshReplacement();
                result.mesh = mesh;
                result.renderer = renderer.ResolveReference(rootObject);
                return result;
            }
        }

        /// <summary>
        /// UberSkinDef representation of a <see cref="SkinDefParams.ProjectileGhostReplacement"/>
        /// </summary>
        [Serializable]
        public struct ProjectileGhostReplacement
        {
            /// <summary>
            /// The Projectile that we're going to replace it's ghost. Can be either a direct reference or an address reference.
            /// </summary>
            public AddressReferencedPrefab projectilePrefab;

            /// <summary>
            /// The GhostReplacement to use
            /// </summary>
            public GameObject projectileGhostReplacementPrefab;

            /// <summary>
            /// Creates a <see cref="SkinDefParams.ProjectileGhostReplacement"/> based off the internal metadata.
            /// </summary>
            /// <returns>The computed <see cref="SkinDefParams.ProjectileGhostReplacement"/></returns>
            public SkinDefParams.ProjectileGhostReplacement ToSkinDefParamsProjectileGhostReplacement()
            {
                var result = new SkinDefParams.ProjectileGhostReplacement();
                result.projectileGhostReplacementPrefab = projectileGhostReplacementPrefab;
                result.projectilePrefab = projectilePrefab.LoadAssetNow();
                return result;
            }
        }

        /// <summary>
        /// UberSkinDef representation of a <see cref="SkinDefParams.MinionSkinReplacement"/>
        /// </summary>
        [Serializable]
        public struct MinionSkinReplacement
        {
            /// <summary>
            /// The CharacterBody prefab that we're going to apply a specific skin. Can be either a direct reference or an address reference.
            /// </summary>
            public AddressReferencedPrefab minionBodyPrefab;
            /// <summary>
            /// The SkinDef to utilize
            /// </summary>
            public SkinDef minionSkin;

            /// <summary>
            /// Creates a <see cref="SkinDefParams.MinionSkinReplacement"/> based off the internal metadata.
            /// </summary>
            /// <returns>The computed <see cref="SkinDefParams.MinionSkinReplacement"/></returns>
            public SkinDefParams.MinionSkinReplacement ToSkinDefParamsMinionSkinReplacement()
            {
                var result = new SkinDefParams.MinionSkinReplacement();
                result.minionSkin = minionSkin;
                result.minionBodyPrefab = minionBodyPrefab.LoadAssetNow();

                return result;
            }
        }

        /// <summary>
        /// UberSkinDef representation of a <see cref="CharacterModel.LightInfo"/>
        /// </summary>
        [Serializable]
        public struct LightInfo
        {
            /// <summary>
            /// A Reference to the Light, or a TransformPath to the Light.
            /// <br></br>
            /// If the UberSkinDef's targetObject is a direct reference, this should point to a direct reference of the prefab itself. Otherwise it should point to a Transform reference utilizing a transform path.
            /// </summary>
            public PrefabReferenceOrTransformPath<Light> light;

            /// <summary>
            /// The LightColor to utilize
            /// </summary>
            public Color lightColor;

            /// <summary>
            /// Creates a <see cref="CharacterModel.LightInfo"/> based off the internal metadata and the root object passed in by <paramref name="rootObject"/>
            /// </summary>
            /// <param name="rootObject">The root object itself</param>
            /// <returns>The computed <see cref="CharacterModel.LightInfo"/></returns>
            public CharacterModel.LightInfo ToCharacterModelLightInfo(Transform rootTransform)
            {
                var result = new CharacterModel.LightInfo();
                result.light = light.ResolveReference(rootTransform);
                result.defaultColor = lightColor;

                return result;
            }
        }

        /// <summary>
        /// A Serialized representation of a <see cref="R2API.SkinVFXInfo"/>
        /// </summary>
        [Serializable]
        public struct VFXOverride
        {
            /// <summary>
            /// The Effect Prefab that we're going to replace or modify. Can be either a direct reference or an address reference.
            /// </summary>
            public AddressReferencedPrefab targetEffect;

            /// <summary>
            /// The ReplacementEffect to utilize
            /// </summary>
            public GameObject replacementEffectPrefab;

            /// <summary>
            /// A SerializableStaticMethod that will be executed to modify the instance of <see cref="targetEffect"/>. This is only used if <see cref="replacementEffectPrefab"/> is null
            /// <para></para>
            /// The method must be Static, return Void, and have a singular argument of type GameObject.
            /// <br></br>
            /// You need to add a <see cref="SerializableStaticMethod.MethodDetectorAttribute"/> for the method to be detected by the editor
            /// <code>
            /// [SerializableStaticMethod.MethodDetector]
            /// private static void ModifyEffect(GameObject gameObject)
            /// {
            /// }
            /// </code>
            /// </summary>
            [SerializableStaticMethod.RequiredArgumentsAttribute(typeof(void), typeof(GameObject))]
            public SerializableStaticMethod serializedOnEffectSpawned;

            /// <summary>
            /// Creates a <see cref="SkinVFXInfo"/> based off the internal metadata and the <see cref="SkinDef"/> passed in by <paramref name="targetSkinDef"/>
            /// </summary>
            /// <param name="rootObject">The root object itself</param>
            /// <returns>The computed <see cref="SkinVFXInfo"/></returns>
            public SkinVFXInfo ToSkinVFXInfo(SkinDef targetSkinDef)
            {
                var result = new SkinVFXInfo();
                result.RequiredSkin = targetSkinDef;
                result.EffectPrefab = targetEffect.LoadAssetNow();

                if(replacementEffectPrefab)
                {
                    result.ReplacementEffectPrefab = replacementEffectPrefab;
                }
                else
                {
                    if (!serializedOnEffectSpawned.TryGetMethod(out MethodInfo methodInfo))
                    {
                        throw new MissingMethodException($"Could not deserialize method");
                    }

                    SkinVFX.OnEffectSpawnedDelegate onEffectSpawnedDelegate = (SkinVFX.OnEffectSpawnedDelegate)Delegate.CreateDelegate(typeof(SkinVFX.OnEffectSpawnedDelegate), methodInfo);
                    result.OnEffectSpawned = onEffectSpawnedDelegate;
                }
                return result;
            }
        }

        /// <summary>
        /// Represents a PrefabReference, which can be either directly referenced in <see cref="reference"/>, or indirectly referenced utilizing the <see cref="transformPath"/>
        /// </summary>
        /// <typeparam name="T">The type of reference, If you need a catch-all, utilize Transform</typeparam>
        [Serializable]
        public struct PrefabReferenceOrTransformPath<T> where T : UnityEngine.Component
        {
            /// <summary>
            /// A direct prefab reference.
            /// </summary>
            [PrefabReference]
            public T reference;

            /// <summary>
            /// A TransformPath that will be used to resolve the Reference. Must not contain the Clone suffix, or the root object's name.
            /// </summary>
            [TransformPath(nameof(targetObject), rootComponentType = typeof(CharacterModel), siblingPropertyComponentTypeRequirement = nameof(reference), allowSelectingRoot = false)]
            public string transformPath;

            /// <summary>
            /// Returns the Reference, if no reference exists, it'll be resolved utilizing <see cref="transformPath"/> and <paramref name="rootTransform"/>
            /// </summary>
            /// <param name="rootTransform">The root transform used to resolve the reference</param>
            /// <returns></returns>
            /// <exception cref="Exception">Thrown when <see cref="reference"/> is not a child of <paramref name="rootTransform"/></exception>
            /// <exception cref="NullReferenceException">Thrown when <paramref name="rootTransform"/> does not contain a child in the path <see cref="transformPath"/></exception>
            public T ResolveReference(Transform rootTransform)
            {
                T result = null;
                if (reference)
                {
                    string path = Util.BuildPrefabTransformPath(rootTransform, reference.transform, false, false);

                    if (!rootTransform.Find(path))
                    {
                        throw new Exception($"{reference} is not a child of {rootTransform}");
                    }

                    result = reference;
                }
                else
                {
                    string path = transformPath;
                    Transform child = rootTransform.Find(path);
                    if (!child)
                    {
                        throw new NullReferenceException($"{rootTransform} does not have a transform within the path {path}");
                    }

                    if(child is T childT)
                    {
                        result = childT;
                    }
                    else
                    {
                        T childComponent = child.GetComponent<T>();
                        if (!childComponent)
                        {
                            throw new NullReferenceException($"Transform {child} at path {path} does not have a Renderer component");
                        }
                        result = childComponent;
                    }
                }

                return result;
            }
        }
        #endregion

        /// <summary>
        /// The SkinDef from which all the metadata in here will be populated
        /// </summary>
        public SkinDef targetSkinDef;

        /// <summary>
        /// An array of SkinDef references, which can be either direct or address references. Keep in mind that these Skins CANNOT be loaded from the catalog!
        /// </summary>
        public AddressReferencedSkinDef[] baseSkins;

        /// <summary>
        /// The TargetObject, which should be either a direct reference to the CharacterBody, or an address reference to the CharacterBody. The mod will resolve the CharacterModel reference automatically. 
        /// </summary>
        public AddressReferencedPrefab targetObject;

        /// <summary>
        /// an Array of <see cref="RendererInfo"/>s that are utilized by this skin
        /// </summary>
        public RendererInfo[] rendererInfos = Array.Empty<RendererInfo>();

        /// <summary>
        /// An array of <see cref="GameObjectActivation"/>s that are utilized by this skin
        /// </summary>
        public GameObjectActivation[] gameObjectActivations = Array.Empty<GameObjectActivation>();

        /// <summary>
        /// An array of <see cref="MeshReplacement"/>s that are utilized by this skin
        /// </summary>
        public MeshReplacement[] meshReplacements = Array.Empty<MeshReplacement>();

        /// <summary>
        /// An array of <see cref="ProjectileGhostReplacement"/>s that are utilized by this skin
        /// </summary>
        public ProjectileGhostReplacement[] projectileGhostReplacements = Array.Empty<ProjectileGhostReplacement>();

        /// <summary>
        /// An array of <see cref="MinionSkinReplacement"/>s that are utilized by this skin
        /// </summary>
        public MinionSkinReplacement[] minionSkinReplacements = Array.Empty<MinionSkinReplacement>();

        /// <summary>
        /// An array of <see cref="LightInfo"/>s that are utilized by this skin
        /// </summary>
        public LightInfo[] lightReplacements = Array.Empty<LightInfo>();

        /// <summary>
        /// An array of <see cref="VFXOverride"/>s that are utilized by this skin
        /// </summary>
        public VFXOverride[] vfxOverrides = Array.Empty<VFXOverride>();

        /// <summary>
        /// Call this method to PreBake your VanillaSkinDef, this must be ran BEFORE <see cref="SkinCatalog"/> initializes!
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">Thrown when no TargetSkinDef is specified</exception>
        public IEnumerator PreBake()
        {
            if (!targetSkinDef)
                throw new NullReferenceException($"No TargetSkinDef specified in {this}");

            if(!targetSkinDef.skinDefParams)
            {
                targetSkinDef.skinDefParams = ScriptableObject.CreateInstance<SkinDefParams>();
                targetSkinDef.name = string.Format("{0}_params", targetSkinDef.name);
            }

            IEnumerator coroutine = ResolveRootObject();
            while(!coroutine.IsDone())
            {
                yield return null;
            }

            HG.Coroutines.ParallelCoroutine parallelCoroutine = new HG.Coroutines.ParallelCoroutine();
            parallelCoroutine.Add(ResolveBaseSkins());
            parallelCoroutine.Add(ResolveRendererInfos());
            parallelCoroutine.Add(ResolveGameObjectActivations());
            parallelCoroutine.Add(ResolveMeshReplacements());
            parallelCoroutine.Add(ResolveProjectileGhostReplacements());
            parallelCoroutine.Add(ResolveMinionSkinReplacements());
            parallelCoroutine.Add(ResolveLightInfos());
            parallelCoroutine.Add(AssignVFXOverrides());

            while(!parallelCoroutine.IsDone())
            {
                yield return null;
            }
        }

        private IEnumerator ResolveRootObject()
        {
            GameObject rootObject = null;
            if(targetObject.UseDirectReference)
            {
                rootObject = targetObject.Asset;

                var characterModel = rootObject.GetComponentInChildren<CharacterModel>();
                if(!characterModel)
                {
                    throw new NullReferenceException($"The targetObject of {this} does not have a CharacterModel!");
                }
                rootObject = characterModel.gameObject;
            }
            else
            {
                rootObject = targetObject.LoadAssetNow();

                var characterModel = rootObject.GetComponentInChildren<CharacterModel>();
                if(!characterModel)
                {
                    throw new NullReferenceException($"The targetObject of {this} does not have a CharacterModel!");
                }
                rootObject = characterModel.gameObject;
            }
            targetSkinDef.rootObject = rootObject;
            yield break;
        }

        private IEnumerator ResolveBaseSkins()
        {
            if (targetSkinDef.baseSkins.Length != baseSkins.Length)
            {
                Array.Resize(ref targetSkinDef.baseSkins, baseSkins.Length);
            }

            for(int i = 0; i < baseSkins.Length; i++)
            {
                var baseSkin = baseSkins[i];
                if(baseSkin.AssetExists)
                {
                    targetSkinDef.baseSkins[i] = baseSkin.Asset;
                }
                else
                {
                    if(baseSkin.CanLoadFromCatalog)
                    {
                        throw new InvalidOperationException($"The BaseSkin at index {i} for {this} has an Address reference and \"CanLoadFromCatalog\" set to true! MSU should've made sure that this internal boolean is set to false, if you find this error while developing please open a bug report.");
                    }

                    targetSkinDef.baseSkins[i] = baseSkin.LoadAssetNow();
                }
            }

            yield break;
        }

        private IEnumerator ResolveRendererInfos()
        {
            var skinDefParams = targetSkinDef.skinDefParams;
            if(skinDefParams.rendererInfos.Length != rendererInfos.Length)
                Array.Resize(ref skinDefParams.rendererInfos, rendererInfos.Length);

            for(int i = 0; i < rendererInfos.Length;i++)
            {
                yield return null;
                skinDefParams.rendererInfos[i] = rendererInfos[i].ToCharacterModelRendererInfo(targetSkinDef.rootObject.transform);
            }
        }
        private IEnumerator ResolveGameObjectActivations()
        {
            var skinDefParams = targetSkinDef.skinDefParams;
            if (skinDefParams.gameObjectActivations.Length != gameObjectActivations.Length)
                Array.Resize(ref skinDefParams.gameObjectActivations, gameObjectActivations.Length);

            for(int i = 0; i < gameObjectActivations.Length; i++)
            {
                yield return null;
                skinDefParams.gameObjectActivations[i] = gameObjectActivations[i].ToSkinDefParamsGameObjectActivation(targetSkinDef.rootObject.transform);
            }
        }

        private IEnumerator ResolveMeshReplacements()
        {
            var skinDefParams = targetSkinDef.skinDefParams;
            if (skinDefParams.meshReplacements.Length != meshReplacements.Length)
                Array.Resize(ref skinDefParams.meshReplacements, meshReplacements.Length);

            for(int i = 0; i < meshReplacements.Length; i++)
            {
                yield return null;
                skinDefParams.meshReplacements[i] = meshReplacements[i].ToSkinDefParamsMeshReplacement(targetSkinDef.rootObject.transform);
            }
        }

        private IEnumerator ResolveProjectileGhostReplacements()
        {
            var skinDefParams = targetSkinDef.skinDefParams;
            if (skinDefParams.projectileGhostReplacements.Length != projectileGhostReplacements.Length)
                Array.Resize(ref skinDefParams.projectileGhostReplacements, projectileGhostReplacements.Length);

            for(int i = 0; i < projectileGhostReplacements.Length; i++)
            {
                yield return null;
                skinDefParams.projectileGhostReplacements[i] = projectileGhostReplacements[i].ToSkinDefParamsProjectileGhostReplacement();
            }
        }

        private IEnumerator ResolveMinionSkinReplacements()
        {
            var skinDefParams = targetSkinDef.skinDefParams;
            if(skinDefParams.minionSkinReplacements.Length != minionSkinReplacements.Length)
                Array.Resize(ref skinDefParams.minionSkinReplacements, minionSkinReplacements.Length);

            for(int i = 0; i < minionSkinReplacements.Length; i++)
            {
                yield return null;
                skinDefParams.minionSkinReplacements[i] = minionSkinReplacements[i].ToSkinDefParamsMinionSkinReplacement();
            }
        }

        private IEnumerator ResolveLightInfos()
        {
            var skinDefParams = targetSkinDef.skinDefParams;
            if(skinDefParams.lightReplacements.Length != lightReplacements.Length)
                Array.Resize(ref skinDefParams.lightReplacements, lightReplacements.Length);
            
            for(int i = 0; i < lightReplacements.Length; i++)
            {
                yield return null;
                skinDefParams.lightReplacements[i] = lightReplacements[i].ToCharacterModelLightInfo(targetSkinDef.rootObject.transform);
            }
        }

        private IEnumerator AssignVFXOverrides()
        {
            for(int i = 0; i < vfxOverrides.Length; i++)
            {
                yield return null;

                SkinVFX.AddSkinVFX(vfxOverrides[i].ToSkinVFXInfo(targetSkinDef));
            }
        }
    }
}
