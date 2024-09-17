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
    /// <summary>
    /// The VanillaSkinDef is a subclass of <see cref="SkinDef"/> which is used for declaring custom SkinDefs for Vanilla survivors.
    /// <para>It avoids running the Awake method of SkinDef and its regular Bake method to ensure the end user chooses when to bake it. To bake the skin you must call <see cref="Initialize"/>, this is a coroutine, and as such its recommended to call it during your content loading routine of your ContentPackProvider.</para>
    /// </summary>
    [CreateAssetMenu(fileName = "New VanillaSkinDef", menuName = "MSU/VanillaSkinDef")]
    public class VanillaSkinDef : SkinDef
    {
        [Space(10)]
        [Header("MSU Data")]
        [Tooltip("The Address for the body which will use this skin def.")]
        [SerializeField] internal string _bodyAddress;

        [Tooltip("The Address for the lobby display object which will use this skin def.")]
        [SerializeField] internal string _displayAddress;

        [Tooltip("An array of Base skins to apply prior to this skin")]
        [SerializeField] internal MoonstormBaseSkin[] _baseSkins = Array.Empty<MoonstormBaseSkin>();

        [Tooltip("An array of renderer infos to modify")]
        [SerializeField] internal MoonstormRendererInfo[] _rendererInfos = Array.Empty<MoonstormRendererInfo>();

        [Tooltip("An array of GameObject Activations to modify")]
        [SerializeField] internal MoonstormGameObjectActivation[] _gameObjectActivations = Array.Empty<MoonstormGameObjectActivation>();

        [Tooltip("An array of Mesh Replacements for this skin")]
        [SerializeField] internal MoonstormMeshReplacement[] _meshReplacements = Array.Empty<MoonstormMeshReplacement>();

        [Tooltip("An array of projectile ghost replacements for this skin")]
        [SerializeField] internal MoonstormProjectileGhostReplacement[] _projectileGhostReplacements = Array.Empty<MoonstormProjectileGhostReplacement>();

        [Tooltip("An array of skins that the minions of the body address will use, in vanilla, this is used to change the aspect of engineer's turrets.")]
        [SerializeField] internal MoonstormMinionSkinReplacement[] _minionSkinReplacements = Array.Empty<MoonstormMinionSkinReplacement>();

        [Tooltip("The total amount of renderer infos the Character has, this is not something you should modify.")]
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

        /// <summary>
        /// Method for initializing this VanillaSkinDef.
        /// <para>The Coroutine will load all the assets required for this skin to work (Base game skinDefs, gameObjects, addresses, etc). Afterwards it'll create runtime versions of <see cref="SkinDef"/>'s internal types, and then bake it.</para>
        /// </summary>
        /// <returns>A Coroutine which can be awaited</returns>
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
                var skin = item.skinDef;
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
                HG.ArrayUtils.ArrayAppend(ref projectileGhostReplacements, item.projectileGhostReplacement);
            }
            foreach(var item in _minionSkinReplacements)
            {
                HG.ArrayUtils.ArrayAppend(ref minionSkinReplacements, item.minionSkinReplacement);
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
        /// <summary>
        /// Represents a <see cref="SkinDef.baseSkins"/> that can be created at runtime
        /// </summary>
        [Serializable]
        public class MoonstormBaseSkin
        {
            [SerializeField, Tooltip("The address of the SkinDef to use as a base")] internal string _skinAddress;
            [SerializeField, Tooltip("A direct reference to an existing SkinDef to use as a base")] internal SkinDef _skinDef;

            /// <summary>
            /// The SkinDef that was obtained by calling <see cref="GetSkin"/>
            /// </summary>
            public SkinDef skinDef { get; private set; }

            /// <summary>
            /// A Coroutine that'll assign a SkinDef to the property <see cref="skinDef"/>
            /// <para>This Coroutine is called automatically by the <see cref="VanillaSkinDef.Initialize"/> method</para>
            /// </summary>
            /// <returns>A coroutine which can be awaited</returns>
            public IEnumerator GetSkin()
            {
                if(_skinDef)
                {
                    skinDef = _skinDef;
                    yield break;
                }

                var load = Addressables.LoadAssetAsync<SkinDef>(_skinAddress);
                while (!load.IsDone)
                    yield return null;

                _skinDef = load.Result;
            }
        }

        /// <summary>
        /// Represents a <see cref="CharacterModel.RendererInfo"/> that can be created at runtime.
        /// </summary>
        [Serializable]
        public class MoonstormRendererInfo
        {
            [Tooltip("The Material that'll be used on the RendererInfo")]
            public Material defaultMaterial;

            [Tooltip("How the Renderer will cast shadows")]
            public ShadowCastingMode defaultShadowCastingMode;

            [Tooltip("Wether the Renderer will ignore overlays")]
            public bool ignoreOverlays;

            [Tooltip("Wether the Renderer will be hidden when the character dies")]
            public bool hideOnDeath;

            [Tooltip("The RendererIndex of the CharacterModel to use to obtain the Renderer")]
            public int rendererIndex;

            /// <summary>
            /// Method that'll create a <see cref="CharacterModel.RendererInfo"/> utilizing the values from this <see cref="MoonstormRendererInfo"/>
            /// </summary>
            /// <param name="model">The model to use to obtain the Renderer for the RendererInfo</param>
            /// <returns>A valid <see cref="CharacterModel.RendererInfo"/></returns>
            /// <exception cref="IndexOutOfRangeException">When <see cref="rendererIndex"/> is out of bounds of the CharacterModel's BaseRendererInfos array.</exception>
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

        /// <summary>
        /// Represents a <see cref="SkinDef.GameObjectActivation"/> that can be created at runtime.
        /// <para>Unlike regular GameObjectActivations, this class can be used for creating new GameObjects for models, such as custom headgear or potentially dynamic bones</para>
        /// </summary>
        [Serializable]
        public class MoonstormGameObjectActivation
        {
            [Tooltip("Wether this GameObjectActivation is a custom activation, custom activations instantiate new objects on the models and activate them accordingly")]
            public bool isCustomActivation;

            [Tooltip("The RendererIndex of the CharacterModel to activate")]
            public int rendererIndex;

            [Tooltip("Wether the RendererInfo should activate.")]
            public bool shouldActivate;

            [Header("Custom Activation")]
            [Tooltip("The prefab to instantiate on the CharacterModel")]
            public GameObject gameObjectPrefab;
            [Tooltip("A ChildLocator entry of the CharacterModel, which will act as a parent of the custom activation")]
            public string childName;
            [Tooltip("The local position of the instantiated GameObject")]
            public Vector3 localPos;
            [Tooltip("The local angles of the instantiated GameObject")]
            public Vector3 localAngles;
            [Tooltip("The local scales of the instantiated GameObject")]
            public Vector3 localScale;

            private SkinDef.GameObjectActivation? _activation;

            /// <summary>
            /// Obtains the generated <see cref="SkinDef.GameObjectActivation"/> from this <see cref="MoonstormGameObjectActivation"/>
            /// </summary>
            /// <param name="model">The model for the activation</param>
            /// <param name="displayModel">The display model for the activation</param>
            /// <returns>The GameObjectActivation</returns>
            public SkinDef.GameObjectActivation GetGameObjectActivation(CharacterModel model, CharacterModel displayModel)
            {
                if (_activation.HasValue)
                    return _activation.Value;

                _activation = isCustomActivation ? CreateCustomActivation(model, displayModel) : CreateActivationFromRendererIndex(model);
                return _activation.Value;
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
                        shouldActivate = true
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

        /// <summary>
        /// Represents a <see cref="SkinDef.MeshReplacement"/> that can be created at runtime
        /// </summary>
        [Serializable]
        public class MoonstormMeshReplacement
        {
            [Tooltip("The new mesh for the renderer")]
            public Mesh mesh;

            [Tooltip("The RendererIndex of the CharacterModel to obtain the renderer which mesh will be replaced")]
            public int rendererIndex;

            /// <summary>
            /// Creates a <see cref="SkinDef.MeshReplacement"/> from this <see cref="MoonstormMeshReplacement"/>
            /// </summary>
            /// <param name="model">The model to obtain the renderer</param>
            /// <returns>The created MeshReplacement</returns>
            /// <exception cref="IndexOutOfRangeException">When <see cref="rendererIndex"/> is out of bounts of the <paramref name="model"/>'s base renderer infos.</exception>
            public SkinDef.MeshReplacement GetMeshReplacement(CharacterModel model)
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

        /// <summary>
        /// Represents a <see cref="SkinDef.ProjectileGhostReplacement"/> that can be created at runtime
        /// </summary>
        [Serializable]
        public class MoonstormProjectileGhostReplacement
        {
            [SerializeField, Tooltip("The Address of the projectile to replace it's ghost")]
            internal string _projectilePrefabAddress;
            [SerializeField, Tooltip("The ghost to use for the projectile")]
            internal GameObject _projectileGhostReplacement;

            /// <summary>
            /// The ProjectileGhostReplacement that was obtained by calling <see cref="GetProjectileGhostReplacement"/>
            /// </summary>
            public ProjectileGhostReplacement projectileGhostReplacement { get; private set; }

            /// <summary>
            /// A Coroutine that'll assign a ProjectileGhostReplacement to the property <see cref="projectileGhostReplacement"/>
            /// <para>This Coroutine is called automatically by the <see cref="VanillaSkinDef.Initialize"/> method</para>
            /// </summary>
            /// <returns>A coroutine which can be awaited</returns>
            public IEnumerator GetProjectileGhostReplacement()
            {
                var load = Addressables.LoadAssetAsync<GameObject>(_projectilePrefabAddress);
                
                while (!load.IsDone)
                    yield return null;

                projectileGhostReplacement = new ProjectileGhostReplacement
                {
                    projectileGhostReplacementPrefab = _projectileGhostReplacement,
                    projectilePrefab = load.Result
                };

                yield break;
            }
        }

        /// <summary>
        /// Represents a <see cref="SkinDef.MinionSkinReplacement"/> that can be created at runtime
        /// </summary>
        [Serializable]
        public class MoonstormMinionSkinReplacement
        {
            [SerializeField, Tooltip("The Address of the minion body to apply the skin to")]
            internal string _minionPrefabAddress;
            [SerializeField, Tooltip("The SkinDef for the minion")]
            internal SkinDef _minionSkin;

            /// <summary>
            /// The MinionSkinReplacement that was obtained by calling <see cref="GetMinionSkinReplacement"/>
            /// </summary>
            public MinionSkinReplacement minionSkinReplacement { get; private set; }

            /// <summary>
            /// A Coroutine that'll assign a MinionSkinReplacement to the property <see cref="minionSkinReplacement"/>
            /// <para>This Coroutine is called automatically by the <see cref="VanillaSkinDef.Initialize"/> method</para>
            /// </summary>
            /// <returns>A coroutine which can be awaited</returns>
            public IEnumerator GetMinionSkinReplacement()
            {
                var load = Addressables.LoadAssetAsync<GameObject>(_minionPrefabAddress);
                while (!load.IsDone)
                    yield return null;

                minionSkinReplacement = new MinionSkinReplacement
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