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
using System.Collections;
using HG.Coroutines;
using HG;
using static MSU.UberSkinDef;

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

        [Serializable]
        public struct GameObjectActivation
        {
            public bool spawnPrefabOnModelObject;
            public PrefabReferenceOrTransformPath<Transform> gameObject;
            public bool shouldActivate;
            public GameObject prefab;
            public Vector3 localPosition;
            public Vector3 localRotation;
            public Vector3 localScale;

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

        [Serializable]
        public struct MeshReplacement
        {
            public PrefabReferenceOrTransformPath<Renderer> renderer;
            public Mesh mesh;

            public SkinDefParams.MeshReplacement ToSkinDefParamsMeshReplacement(Transform rootObject)
            {
                var result = new SkinDefParams.MeshReplacement();
                result.mesh = mesh;
                result.renderer = renderer.ResolveReference(rootObject);
                return result;
            }
        }

        [Serializable]
        public struct ProjectileGhostReplacement
        {
            public AddressReferencedPrefab projectilePrefab;
            public GameObject projectileGhostReplacementPrefab;

            public SkinDefParams.ProjectileGhostReplacement ToSkinDefParamsProjectileGhostReplacement()
            {
                var result = new SkinDefParams.ProjectileGhostReplacement();
                result.projectileGhostReplacementPrefab = projectileGhostReplacementPrefab;
                result.projectilePrefab = projectilePrefab.LoadAssetNow();
                return result;
            }
        }

        [Serializable]
        public struct MinionSkinReplacement
        {
            public AddressReferencedPrefab minionBodyPrefab;
            public SkinDef minionSkin;

            public SkinDefParams.MinionSkinReplacement ToSkinDefParamsMinionSkinReplacement()
            {
                var result = new SkinDefParams.MinionSkinReplacement();
                result.minionSkin = minionSkin;
                result.minionBodyPrefab = minionBodyPrefab.LoadAssetNow();

                return result;
            }
        }

        [Serializable]
        public struct LightInfo
        {
            public PrefabReferenceOrTransformPath<Light> light;
            public Color lightColor;

            public CharacterModel.LightInfo ToCharacterModelLightInfo(Transform rootTransform)
            {
                var result = new CharacterModel.LightInfo();
                result.light = light.ResolveReference(rootTransform);
                result.defaultColor = lightColor;

                return result;
            }
        }


        [Serializable]
        public struct VFXOverride
        {
            public AddressReferencedPrefab targetEffect;
            public GameObject replacementEffectPrefab;

            [SerializableStaticMethod.RequiredArguments(typeof(void), typeof(GameObject))]
            public SerializableStaticMethod serializedOnEffectSpawned;

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

        [Serializable]
        public struct PrefabReferenceOrTransformPath<T> where T : UnityEngine.Component
        {
            [PrefabReference]
            public T reference;
            [TransformPath(nameof(targetObject), rootComponentType = typeof(CharacterModel), siblingPropertyComponentTypeRequirement = nameof(reference), allowSelectingRoot = false)]
            public string transformPath;

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


        public SkinDef targetSkinDef;

        public AddressReferencedSkinDef[] baseSkins;
        public AddressReferencedPrefab targetObject;

        public RendererInfo[] rendererInfos = Array.Empty<RendererInfo>();
        public GameObjectActivation[] gameObjectActivations = Array.Empty<GameObjectActivation>();
        public MeshReplacement[] meshReplacements = Array.Empty<MeshReplacement>();
        public ProjectileGhostReplacement[] projectileGhostReplacements = Array.Empty<ProjectileGhostReplacement>();
        public MinionSkinReplacement[] minionSkinReplacements = Array.Empty<MinionSkinReplacement>();
        public LightInfo[] lightReplacements = Array.Empty<LightInfo>();
        public VFXOverride[] vfxOverrides = Array.Empty<VFXOverride>();

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
            }
            else
            {
                rootObject = targetObject.LoadAssetNow();

                var characterModel = rootObject.GetComponentInChildren<CharacterModel>();
                if(!characterModel)
                {
                    throw new NullReferenceException($"The RootObject of {this} does not have a CharacterModel component!");
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
