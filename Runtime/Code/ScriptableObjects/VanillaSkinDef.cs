using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MSU
{
    [CreateAssetMenu(fileName = "New VanillaSkinDef", menuName = "MSU/VanillaSkinDef")]
    public class VanillaSkinDef : SkinDef
    {
        [Space(10)]
        [Header("MSU Data")]
        [SerializeField] internal string _bodyAddress;
        [SerializeField] internal string _displayAddress;
        [SerializeField] internal MoonstormBaseSkin[] _baseSkins = Array.Empty<MoonstormBaseSkin>();
        [SerializeField] internal MoonstormRendererInfo[] _rendererInfos = Array.Empty<MoonstormRendererInfo>();
        [SerializeField] internal MoonstormGameObjectActivation[] _gameObjectActivations = Array.Empty<MoonstormGameObjectActivation>();
        [SerializeField] internal MoonstormMeshReplacement[] _meshReplacements = Array.Empty<MoonstormMeshReplacement>();
        [SerializeField] internal MoonstormProjectileGhostReplacement[] _projectileGhostReplacements = Array.Empty<MoonstormProjectileGhostReplacement>();
        [SerializeField] internal MoonstormMinionSkinReplacement[] _minionSkinReplacements = Array.Empty<MoonstormMinionSkinReplacement>();

        public int cachedRendererAmount;

        private new void Awake()
        {
            //Do Nothing
#if DEBUG && !UNITY_EDITOR
            MSULog.Info($"Awake called for VanillaSkinDef {this}, remember to call \"Initialize\" on your VaillaSkinDefs so they load properly and bake properly!");
#endif
        }

        private new void Bake()
        {
            //Do Nothing
        }

        public IEnumerator Initialize()
        {
#if DEBUG
            MSULog.Debug($"Attempting to initialize and bake {this}");
#endif
            
            ParallelCoroutine coroutine = new ParallelCoroutine();
            var bodyAddressLoad = Addressables.LoadAssetAsync<GameObject>(_bodyAddress);
            var displayAddressLoad = Addressables.LoadAssetAsync<GameObject>(_displayAddress);
            
            coroutine.Add(bodyAddressLoad);
            coroutine.Add(displayAddressLoad);
            foreach(MoonstormBaseSkin baseSkin in _baseSkins)
            {
                coroutine.Add(baseSkin.GetSkin());
            }
            foreach(MoonstormProjectileGhostReplacement projectileGhostReplacement in _projectileGhostReplacements)
            {
                coroutine.Add(projectileGhostReplacement.GetProjectileGhostReplacement());
            }
            foreach(MoonstormMinionSkinReplacement minionSkinReplacement in _minionSkinReplacements)
            {
                coroutine.Add(minionSkinReplacement.GetMinionSkinReplacement());
            }

            while (!coroutine.IsDone())
                yield return null;

            var bodyPrefab = bodyAddressLoad.Result;
            var displayPrefab = displayAddressLoad.Result;

            var modelObject = bodyPrefab.GetComponent<ModelLocator>().modelTransform.gameObject;

            Bake(modelObject.GetComponent<CharacterModel>(), displayPrefab.GetComponentInChildren<CharacterModel>());
        }

        private void Bake(CharacterModel model, CharacterModel displayModel)
        {
            //Fills the unfilled base fields
            foreach (var item in _baseSkins)
            {
                var skin = item.SkinDef;
                HG.ArrayUtils.ArrayAppend(ref baseSkins, skin);
            }
            rootObject = model.gameObject;
            foreach(var item in _rendererInfos)
            {
                HG.ArrayUtils.ArrayAppend(ref rendererInfos, item.GetRendererInfo(model));
            }
            foreach(var item in _gameObjectActivations)
            {
                HG.ArrayUtils.ArrayAppend(ref gameObjectActivations, item.GetGameObjectActivation(model, displayModel));
            }
            foreach(var item in _meshReplacements)
            {
                HG.ArrayUtils.ArrayAppend(ref meshReplacements, item.GetMeshReplacement(model));
            }
            foreach(var item in _projectileGhostReplacements)
            {
                HG.ArrayUtils.ArrayAppend(ref projectileGhostReplacements, item.ProjectileGhostReplacement);
            }
            foreach(var item in _minionSkinReplacements)
            {
                HG.ArrayUtils.ArrayAppend(ref minionSkinReplacements, item.MinionSkinReplacement);
            }

            base.Bake();

            //Adds the skindefs to the models
            ModelSkinController controller = model.GetComponent<ModelSkinController>();
            if (controller)
                HG.ArrayUtils.ArrayAppend(ref controller.skins, this);
            controller = displayModel.GetComponent<ModelSkinController>();
            if (controller)
                HG.ArrayUtils.ArrayAppend(ref controller.skins, this);
        }
        #region internal types
        [Serializable]
        public class MoonstormBaseSkin
        {
            [SerializeField] internal string _skinAddress;
            [SerializeField] internal SkinDef _skinDef;

            public SkinDef SkinDef { get; private set; }
            public IEnumerator GetSkin()
            {
                if(_skinDef)
                {
                    SkinDef = _skinDef;
                    yield break;
                }

                var load = Addressables.LoadAssetAsync<SkinDef>(_skinAddress);
                while (!load.IsDone)
                    yield return null;

                _skinDef = load.Result;
            }
        }

        [Serializable]
        public class MoonstormRendererInfo
        {
            public Material defaultMaterial;
            public ShadowCastingMode defaultShadowCastingMode;
            public bool ignoreOverlays;
            public bool hideOnDeath;
            public int rendererIndex;

            public CharacterModel.RendererInfo GetRendererInfo(CharacterModel model)
            {
                if (rendererIndex < 0 || rendererIndex > model.baseRendererInfos.Length)
                {
                    throw new IndexOutOfRangeException($"Renderer Index of MSGameObjectActivation is out of bounds. (index: {rendererIndex})");
                }

                var baseRenderInfo = model.baseRendererInfos[rendererIndex];
                return new CharacterModel.RendererInfo
                {
                    defaultMaterial = defaultMaterial,
                    defaultShadowCastingMode = defaultShadowCastingMode,
                    hideOnDeath = hideOnDeath,
                    ignoreOverlays = ignoreOverlays,
                    renderer = baseRenderInfo.renderer
                };
            }
        }

        [Serializable]
        public class MoonstormGameObjectActivation
        {
            public bool isCustomActivation;
            public int rendererIndex;
            public GameObject gameObjectPrefab;
            public string childName;
            public bool shouldActivate;
            public Vector3 localPos;
            public Vector3 localAngles;
            public Vector3 localScale;

            public SkinDef.GameObjectActivation GetGameObjectActivation(CharacterModel model, CharacterModel displayModel)
            {
                return isCustomActivation ? CreateCustomActivation(model, displayModel) : CreateActivationFromRendererIndex(model);
            }

            private SkinDef.GameObjectActivation CreateCustomActivation(CharacterModel model, CharacterModel displayModel)
            {
                Transform child = model.childLocator.FindChild(childName);
                if (child)
                {
                    GameObject objectInstance = Instantiate(gameObjectPrefab, child, false);
                    objectInstance.transform.localPosition = localPos;
                    objectInstance.transform.localEulerAngles = localAngles;
                    objectInstance.transform.localScale = localScale;
                    objectInstance.SetActive(false);
                    //This instantiates it on the display model too so it shows up in the menu
                    if (displayModel)
                        Instantiate(gameObjectPrefab, model.childLocator.FindChild(childName), false).SetActive(false);
                    return new SkinDef.GameObjectActivation
                    {
                        gameObject = objectInstance,
                        shouldActivate = false
                    };
                }
                MSULog.Error($"Error: child {childName} to parent {gameObjectPrefab} to not found. Did you misspell the name?");
                return new SkinDef.GameObjectActivation { };
            }

            private SkinDef.GameObjectActivation CreateActivationFromRendererIndex(CharacterModel model)
            {
                if (rendererIndex < 0 || rendererIndex > model.baseRendererInfos.Length)
                {
                    throw new IndexOutOfRangeException($"Renderer Index of MSGameObjectActivation is out of bounds. (index: {rendererIndex})");
                }
                var goActivation = new SkinDef.GameObjectActivation();
                goActivation.shouldActivate = shouldActivate;
                goActivation.gameObject = model.baseRendererInfos[rendererIndex].renderer.gameObject;
                return goActivation;
            }
        }

        [Serializable]
        public class MoonstormMeshReplacement
        {
            public Mesh mesh;
            public int rendererIndex;

            internal SkinDef.MeshReplacement GetMeshReplacement(CharacterModel model)
            {
                if (rendererIndex < 0 || rendererIndex > model.baseRendererInfos.Length)
                {
                    throw new IndexOutOfRangeException($"Renderer Index of MSGameObjectActivation is out of bounds. (index: {rendererIndex})");
                }
                var meshReplacement = new SkinDef.MeshReplacement();
                meshReplacement.mesh = mesh;
                meshReplacement.renderer = model.baseRendererInfos[rendererIndex].renderer;
                return meshReplacement;
            }
        }

        [Serializable]
        public class MoonstormProjectileGhostReplacement
        {
            [SerializeField]
            internal string _projectilePrefabAddress;
            [SerializeField]
            internal GameObject _projectileGhostReplacement;

            public ProjectileGhostReplacement ProjectileGhostReplacement { get; private set; }
            public IEnumerator GetProjectileGhostReplacement()
            {
                var load = Addressables.LoadAssetAsync<GameObject>(_projectilePrefabAddress);
                
                while (!load.IsDone)
                    yield return null;

                ProjectileGhostReplacement = new ProjectileGhostReplacement
                {
                    projectileGhostReplacementPrefab = _projectileGhostReplacement,
                    projectilePrefab = load.Result
                };

                yield break;
            }
        }

        [Serializable]
        public class MoonstormMinionSkinReplacement
        {
            [SerializeField]
            internal string _minionPrefabAddress;
            [SerializeField]
            internal SkinDef _minionSkin;

            public MinionSkinReplacement MinionSkinReplacement { get; private set; }
            public IEnumerator GetMinionSkinReplacement()
            {
                var load = Addressables.LoadAssetAsync<GameObject>(_minionPrefabAddress);
                while (!load.IsDone)
                    yield return null;

                MinionSkinReplacement = new MinionSkinReplacement
                {
                    minionBodyPrefab = load.Result,
                    minionSkin = _minionSkin,
                };

                yield break;
            }
        }
        #endregion
    }
}