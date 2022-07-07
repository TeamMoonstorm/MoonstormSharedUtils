using RoR2;
using RoR2.Projectile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="VanillaSkinDef"/> is an extension of <see cref="SkinDef"/> that allows for creating of skins for vanilla characterbodies.
    /// </summary>
    [CreateAssetMenu(fileName = "New VanillaSkinDefinition", menuName = "Moonstorm/VanillaSkinDefinition")]
    public class VanillaSkinDefinition : SkinDef
    {
        #region Internal Types
        /// <summary>
        /// A Wrapper for <see cref="SkinDef.baseSkins"/>
        /// <para>Allows loading a base skin via an address</para>
        /// </summary>
        [Serializable]
        public class MSBaseSkin
        {
            [Tooltip("The address of the base skin")]
            public string skinAddress;
            public SkinDef skin;

            internal async Task<SkinDef> Upgrade()
            {
                return !skin ? await Addressables.LoadAssetAsync<SkinDef>(skinAddress).Task : skin;
            }
        }

        /// <summary>
        /// A wrapper for <see cref="SkinDef.rendererInfos"/>
        /// <para>used for replacing materials and modifying the values of a RendererInfo.</para>
        /// </summary>
        [Serializable]
        public class MSRendererInfo
        {
            public Material defaultMaterial;
            public ShadowCastingMode defaultShadowCastingMode;
            public bool ignoreOverlays;
            public bool hideOnDeath;
            [Tooltip("The renderer index this RendererInfo is going to modify.")]
            public int rendererIndex;

            internal CharacterModel.RendererInfo Upgrade(CharacterModel model)
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
        /// A Wrapper for <see cref="SkinDef.GameObjectActivation"/>
        /// <para>Allows for appending new GameObjects to skins via child locator entries</para>
        /// </summary>
        [Serializable]
        public class MSGameObjectActivation
        {
            [Tooltip("Wether this is a custom Activation.\nCustom Activations create new ionstances of the gameObjectPrefab provided on the body, using the child locator entry as the parent.")]
            public bool isCustomActivation = false;
            [Tooltip("The renderer index this GameObjectActivation is going to activate")]
            public int rendererIndex;

            [Tooltip("The prefab to instantiate during the custom activation")]
            public GameObject gameObjectPrefab;
            [Tooltip("If isCustomActivation is set to true, then the gameObjectPrefab will be instantiated on this child")]
            public string childName;

            public bool shouldActivate;

            internal SkinDef.GameObjectActivation Upgrade(CharacterModel model, CharacterModel displayModel)
            {
                return isCustomActivation ? CreateCustomActivation(model, displayModel) : CreateActivationFromRendererIndex(model);
            }

            private SkinDef.GameObjectActivation CreateCustomActivation(CharacterModel model, CharacterModel displayModel)
            {
                Transform child = model.childLocator.FindChild(childName);
                if (child)
                {
                    GameObject objectInstance = Instantiate(gameObjectPrefab, child, false);
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

        /// <summary>
        /// A Wrapper for <see cref="SkinDef.MeshReplacement"/>
        /// </summary>
        [Serializable]
        public class MSMeshReplacement
        {
            public Mesh mesh;
            [Tooltip("The renderer index this MeshReplacement is going to modify")]
            public int rendererIndex;

            internal SkinDef.MeshReplacement Upgrade(CharacterModel model)
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
        /// A wrapper for <see cref="SkinDef.ProjectileGhostReplacement"/>
        /// </summary>
        [Serializable]
        public class MSProjectileGhostReplacement
        {
            [Tooltip("The address of the projectile to replace it's ghost")]
            public string projectilePrefabAddress;
            public GameObject projectileGhostReplacement;

            internal async Task<SkinDef.ProjectileGhostReplacement> Upgrade()
            {
                var prefab = await Addressables.LoadAssetAsync<GameObject>(projectilePrefabAddress).Task;
                var ghostReplacement = new SkinDef.ProjectileGhostReplacement();
                ghostReplacement.projectilePrefab = prefab;
                ghostReplacement.projectileGhostReplacementPrefab = projectileGhostReplacement;
                return ghostReplacement;
            }
        }

        /// <summary>
        /// A Wrapper for <see cref="SkinDef.MinionSkinReplacement"/>
        /// </summary>
        [Serializable]
        public class MSMinionSkinReplacements
        {
            [Tooltip("The address of the minion to apply the skin to")]
            public string minionPrefabAddress;
            public SkinDef minionSkin;

            internal async Task<SkinDef.MinionSkinReplacement> Upgrade()
            {
                var prefab = await Addressables.LoadAssetAsync<GameObject>(minionPrefabAddress).Task;
                var minionReplacement = new SkinDef.MinionSkinReplacement();
                minionReplacement.minionBodyPrefab = prefab;
                minionReplacement.minionSkin = minionSkin;
                return minionReplacement;
            }
        }
        #endregion

        [Tooltip("The Address of the CharacterBody")]
        public string bodyAddress;
        [Tooltip("The Address of the bodyAddress' Display prefab")]
        public string displayAddress;
        [Tooltip("Skins to apply before this one")]
        public MSBaseSkin[] _baseSkins;
        [Tooltip("Modify the renderer infos' materials and properties")]
        public MSRendererInfo[] _rendererInfos;
        [Tooltip("Activate or Deactivate game objects")]
        public MSGameObjectActivation[] _gameObjectActivations;
        [Tooltip("Replace the renderer infos' meshes")]
        public MSMeshReplacement[] _meshReplacements;
        [Tooltip("Replace a Projectile's Ghost Prefab")]
        public MSProjectileGhostReplacement[] _projectileGhostReplacements;
        [Tooltip("Replace a minion's skin")]
        public MSMinionSkinReplacements[] _minionSkinReplacements;

        /// <summary>
        /// The amount of renderers the body in <see cref="bodyAddress"/> has
        /// </summary>
        [HideInInspector]
        public int rendererAmounts;

        private async void Awake()
        {
            if(Application.IsPlaying(this))
            {
                try
                {
                    MSULog.Debug($"Attempting to finalize {this}");
                    var bodyPrefab = await Addressables.LoadAssetAsync<GameObject>(bodyAddress).Task;
                    var modelObject = bodyPrefab.GetComponent<ModelLocator>()?.modelTransform?.gameObject;
                    var displayPrefab = await Addressables.LoadAssetAsync<GameObject>(displayAddress).Task;

                    CharacterModel model = modelObject.GetComponent<CharacterModel>();
                    CharacterModel displayModel = displayPrefab.GetComponentInChildren<CharacterModel>();

                    //Fills the unfilled base fields
                    foreach (var item in _baseSkins)
                    {
                        var skin = await item.Upgrade();
                        HG.ArrayUtils.ArrayAppend(ref baseSkins, skin);
                    }
                    rootObject = modelObject;
                    foreach(var item in _rendererInfos)
                    {
                        HG.ArrayUtils.ArrayAppend(ref rendererInfos, item.Upgrade(model));
                    }
                    foreach (var item in _gameObjectActivations)
                    {
                        HG.ArrayUtils.ArrayAppend(ref gameObjectActivations, item.Upgrade(model, displayModel));
                    }
                    foreach (var item in _meshReplacements)
                    {
                        HG.ArrayUtils.ArrayAppend(ref meshReplacements, item.Upgrade(model));
                    }
                    foreach (var item in _projectileGhostReplacements)
                    {
                        var ghostReplacement = await item.Upgrade();
                        HG.ArrayUtils.ArrayAppend(ref projectileGhostReplacements, ghostReplacement);
                    }
                    foreach (var item in _minionSkinReplacements)
                    {
                        var minionSkin = await item.Upgrade();
                        HG.ArrayUtils.ArrayAppend(ref minionSkinReplacements, minionSkin);
                    }

                    base.Awake();

                    //Adds the skindefs to the models
                    ModelSkinController controller = model.GetComponent<ModelSkinController>();
                    if (controller)
                        HG.ArrayUtils.ArrayAppend(ref controller.skins, this);
                    controller = displayModel.GetComponent<ModelSkinController>();
                    if (controller)
                        HG.ArrayUtils.ArrayAppend(ref controller.skins, this);
                }
                catch(Exception e)
                {
                    MSULog.Error(e);
                }
            }
        }

        #region Validators
        private void OnValidate()
        {
            GetRendererCount();
            ValidateDisplayAddress();
            ValidateBaseSkinAddresses();
            ValidateProjectileAddresses();
            ValidateMinionAddresses();
        }

        private async void GetRendererCount()
        {
            Object obj = null;
            try
            {
                obj = await Addressables.LoadAssetAsync<GameObject>(bodyAddress).Task;
                var modelLoc = (obj as GameObject).GetComponent<ModelLocator>();
                var modelTransform = modelLoc._modelTransform;
                var charModel = modelTransform.gameObject.GetComponent<CharacterModel>();

                rendererAmounts = charModel.baseRendererInfos.Length;
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to validate renderer counts for {this}: {e}");
            }
        }

        private async void ValidateDisplayAddress()
        {
            try
            {
                var obj = await Addressables.LoadAssetAsync<GameObject>(displayAddress).Task;
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to validate Display Address for {this}: {e}");
            }
        }

        private async void ValidateBaseSkinAddresses()
        {
            for (int i = 0; i < _baseSkins.Length; i++)
            {
                UnityEngine.Object obj = null;
                try
                {
                    obj = await Addressables.LoadAssetAsync<SkinDef>(_baseSkins[i].skinAddress).Task;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to validate projectile ghost replacement index {i}: {e}");
                }
            }
        }

        private async void ValidateProjectileAddresses()
        {
            for(int i = 0; i < _projectileGhostReplacements.Length; i++)
            {
                UnityEngine.Object obj = null;
                try
                {
                    obj = await Addressables.LoadAssetAsync<GameObject>(_projectileGhostReplacements[i].projectilePrefabAddress).Task;
                    var projectile = (GameObject)obj;
                    if(!projectile.GetComponent<ProjectileController>())
                    {
                        throw new InvalidOperationException($"Projectile Ghost Replacement provides an address for a game object that does not have a ProjectileController component.");
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError($"Failed to validate projectile ghost replacement index {i}: {e}");
                }
            }
        }

        private async void ValidateMinionAddresses()
        {
            for (int i = 0; i < _minionSkinReplacements.Length; i++)
            {
                UnityEngine.Object obj = null;
                try
                {
                    obj = await Addressables.LoadAssetAsync<GameObject>(_minionSkinReplacements[i].minionPrefabAddress).Task;
                    var body = (GameObject)obj;
                    if(!body.GetComponent<CharacterBody>())
                    {
                        throw new InvalidOperationException($"Minion Skin Replacement provides an address for a game object that does not have a CharacterBody component.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to validate projectile ghost replacement index {i}: {e}");
                }
            }
        }
        #endregion
    }
}