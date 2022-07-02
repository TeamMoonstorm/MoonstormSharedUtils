using RoR2;
using RoR2.Projectile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Moonstorm
{
    /*
     * TODO: editor to hide the following vanilla fields:
     * > Renderer Infos
     * > 
    */
    [CreateAssetMenu(fileName = "New VanillaSkinDefinition", menuName = "Moonstorm/VanillaSkinDefinition")]
    public class VanillaSkinDefinition : SkinDef
    {
        #region Internal Types
        [Serializable]
        public class MSBaseSkin
        {
            public string skinAddress;

            public async Task<SkinDef> Upgrade()
            {
                return await Addressables.LoadAssetAsync<SkinDef>(skinAddress).Task;
            }
        }

        [Serializable]
        public class MSGameObjectActivation
        {
            public bool isCustomActivation = false;
            public int rendererIndex;

            public GameObject gameObjectPrefab;
            public string childName;

            public bool shouldActivate;

            public SkinDef.GameObjectActivation Upgrade(CharacterModel model, CharacterModel displayModel)
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

        [Serializable]
        public class MSMeshReplacement
        {
            public Mesh mesh;
            public int rendererIndex;

            public SkinDef.MeshReplacement Upgrade(CharacterModel model)
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
        public class MSProjectileGhostReplacement
        {
            public string projectilePrefabAddress;
            public GameObject projectileGhostReplacement;

            public async Task<SkinDef.ProjectileGhostReplacement> Upgrade()
            {
                var prefab = await Addressables.LoadAssetAsync<GameObject>(projectilePrefabAddress).Task;
                var ghostReplacement = new SkinDef.ProjectileGhostReplacement();
                ghostReplacement.projectilePrefab = prefab;
                ghostReplacement.projectileGhostReplacementPrefab = projectileGhostReplacement;
                return ghostReplacement;
            }
        }

        [Serializable]
        public class MSMinionSkinReplacements
        {
            public string minionPrefabAddress;
            public VanillaSkinDefinition minionSkin;

            public async Task<SkinDef.MinionSkinReplacement> Upgrade()
            {
                var prefab = await Addressables.LoadAssetAsync<GameObject>(minionPrefabAddress).Task;
                var minionReplacement = new SkinDef.MinionSkinReplacement();
                minionReplacement.minionBodyPrefab = prefab;
                minionReplacement.minionSkin = minionSkin;
                return minionReplacement;
            }
        }
        #endregion

        public string bodyAddress;
        public string displayAddress;
        public MSBaseSkin[] _baseSkins;
        public MSGameObjectActivation[] _gameObjectActivations;
        public MSMeshReplacement[] _meshReplacements;
        public MSProjectileGhostReplacement[] _projectileGhostReplacements;
        public MSMinionSkinReplacements[] _minionSkinReplacements;

        [HideInInspector]
        public int rendererAmounts;

        public async void Awake()
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
                    rendererInfos = model.baseRendererInfos;
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