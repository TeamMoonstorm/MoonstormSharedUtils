using MSU.AddressReferencedAssets;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace MSU
{
    /// <summary>
    /// A <see cref="VanillaSkinDefinition"/> is an extension of <see cref="SkinDef"/> that allows for creating skins for vanilla CharacterBodies
    /// </summary>
    [CreateAssetMenu(fileName = "New VanillaSkinDefinition", menuName = "MSU/VanillaSkinDefinition")]
    public class VanillaSkinDefinition : SkinDef
    {
        [Tooltip("The Address for the CharacterBody")]
        public string bodyAddress;
        [Tooltip("The Address of the Body's DisplayPrefab")]
        public string displayAddress;

        [Space(5)]
        [Tooltip("Base skins to apply before this one")]
        public AddressReferencedSkinDef[] _baseSkins = Array.Empty<AddressReferencedSkinDef>();
        [Tooltip("Modify the renderer info's materials and properties")]
        public RendererInfo[] _rendererInfos = Array.Empty<RendererInfo>();
        [Tooltip("Activate or Deactivate game objects, can also be used to add new GameObjects to the SkinDef")]
        public CustomGameObjectActivation[] _gameObjectActivations = Array.Empty<CustomGameObjectActivation>();
        [Tooltip("Replace the renderer info's meshes")]
        public CustomMeshReplacement[] _meshReplacements = Array.Empty<CustomMeshReplacement>();
        [Tooltip("Replace a projectile's Ghost Prefab")]
        public AddressedProjectileGhostReplacement[] _projectileGhostReplacements = Array.Empty<AddressedProjectileGhostReplacement>();
        [Tooltip("Replace a minion's skin")]
        public AddressedMinionSkinReplacement[] _minionSkinReplacements = Array.Empty<AddressedMinionSkinReplacement>();

        new private void Awake()
        {
            if (Application.IsPlaying(this))
            {
                try
                {
                    FinishSkin();
                }
                catch (Exception e)
                {
                    MSULog.Error($"{e}\n({this})");
                }
            }
        }

        private void FinishSkin()
        {
#if DEBUG
            MSULog.Debug("Attempting to finalize " + this);
#endif
            var bodyPrefab = Addressables.LoadAssetAsync<GameObject>(bodyAddress).WaitForCompletion();
            var characterModel = bodyPrefab.GetComponentInChildren<CharacterModel>();

            var displayPrefab = Addressables.LoadAssetAsync<GameObject>(displayAddress).WaitForCompletion();
            var displayModel = displayPrefab.GetComponentInChildren<CharacterModel>();

            //fill unfilled base fields
            foreach (var baseSkin in _baseSkins)
            {
                HG.ArrayUtils.ArrayAppend(ref baseSkins, baseSkin.Asset);
            }
            rootObject = characterModel.gameObject;
            foreach(var item in _rendererInfos)
            {
                var rendererInfo = item.Upgrade(characterModel);
                HG.ArrayUtils.ArrayAppend(ref rendererInfos, rendererInfo);
            }
            foreach(var item in _gameObjectActivations)
            {
                var gameObjectActivation = item.Upgrade(characterModel, displayModel);
                HG.ArrayUtils.ArrayAppend(ref gameObjectActivations, gameObjectActivation);
            }
            foreach(var item in _meshReplacements)
            {
                var meshReplacement = item.Upgrade(characterModel);
                HG.ArrayUtils.ArrayAppend(ref meshReplacements, meshReplacement);
            }
            foreach(var item in _projectileGhostReplacements)
            {
                var ghostReplacement = item.Upgrade();
                HG.ArrayUtils.ArrayAppend(ref projectileGhostReplacements, ghostReplacement);
            }
            foreach(var item in _minionSkinReplacements)
            {
                var minionSkin = item.Upgrade();
                HG.ArrayUtils.ArrayAppend(ref minionSkinReplacements, minionSkin);
            }

            //Create runtime skin
            base.Awake();

            //Adds the skindefs to the models
            ModelSkinController controller = characterModel.GetComponent<ModelSkinController>();
            if (controller)
                HG.ArrayUtils.ArrayAppend(ref controller.skins, this);
            controller = displayModel.GetComponent<ModelSkinController>();
            if (controller)
                HG.ArrayUtils.ArrayAppend(ref controller.skins, this);
        }

        #region Internal Types

        /// <summary>
        /// A Wrapper for <see cref="SkinDef.rendererInfos"/>
        /// <para>Used for replacing materials and modifying the values of a RendererInfo</para>
        /// </summary>
        [Serializable]
        public struct RendererInfo
        {
            [Tooltip("The Renderer index that's going to be modified")]
            public int renderer;
            [Tooltip("The replacement material for the specified renderer info")]
            public Material defaultMaterial;
            [Tooltip("How the renderer should cast shadows")]
            public ShadowCastingMode defaultShadowCastingMode;
            [Tooltip("Wether or not the renderer ignores overlays")]
            public bool ignoreOverlays;
            [Tooltip("Wether or not to hide this renderer when the character dies")]
            public bool hideOnDeath;

            internal CharacterModel.RendererInfo Upgrade(CharacterModel model)
            {
                var baseRendererInfo = model.baseRendererInfos[renderer];
                return new CharacterModel.RendererInfo
                {
                    defaultMaterial = defaultMaterial,
                    defaultShadowCastingMode = defaultShadowCastingMode,
                    hideOnDeath = hideOnDeath,
                    ignoreOverlays = ignoreOverlays,
                    renderer = baseRendererInfo.renderer
                };
            }
        }

        /// <summary>
        /// A Wrapper for <see cref="SkinDef.gameObjectActivations"/>
        /// <para>It also allows for creating and instantiating new GameObjects to skins via child locator entries</para>
        /// </summary>
        [Serializable]
        public struct CustomGameObjectActivation
        {
            //Vanilla stuff
            [Tooltip("The renderer index this GameObjectActivation is going to activate\nNot used if \"Is Custom Activation\" is set to true")]
            public int renderer;
            [Tooltip("Wether or not the renderer specified in \"Renderer\" should be enabled\nNot used if \"Is Custom Activation\" is set to true")]
            public bool shouldActivate;

            [Tooltip("If this is set to True, then this GameObjectActivation will instantiate a new object on the Skin at runtime")]
            public bool isCustomActivation;
            [Tooltip("The custom object to instantiate")]
            public GameObject customObject;
            [Tooltip("The base child locator entry which the new object will be parented to")]
            public string childLocatorEntry;
            [Tooltip("The position of the custom object relative to it's parent")]
            public Vector3 localPos;
            [Tooltip("The angles of the custom object relative to it's parent")]
            public Vector3 localAngles;
            [Tooltip("The scales of the custom object relative to it's parent")]
            public Vector3 localScale;

            internal SkinDef.GameObjectActivation Upgrade(CharacterModel characterModel, CharacterModel displayModel)
            {
                return isCustomActivation ? CreateCustomActivation(characterModel, displayModel) : CreateActivationFromRendererIndex(characterModel);
            }

            private SkinDef.GameObjectActivation CreateCustomActivation(CharacterModel characterModel, CharacterModel displayModel)
            {
                Transform child = characterModel.GetComponent<ChildLocator>().FindChild(childLocatorEntry);
                if(!child)
                {
                    MSULog.Warning($"Cannot create custom game object activation since \"{childLocatorEntry}\" is not a valid entry for model {characterModel}");
                    return new GameObjectActivation { };
                }

                GameObject objectInstance = Instantiate(customObject, child, false);
                var objectInstanceTransform = objectInstance.transform;
                objectInstanceTransform.SetPositionAndRotation(localPos, Quaternion.Euler(localAngles));
                objectInstanceTransform.localScale = localScale;

                objectInstance.SetActive(false);

                if (displayModel)
                {
                    var path = Util.BuildPrefabTransformPath(characterModel.transform, child.transform);
                    var displayChild = displayModel.transform.Find(path);
                    var displayObjectInstance = Instantiate(customObject, displayChild, false);
                    var displayObjectInstanceTransform = displayObjectInstance.transform;
                    displayObjectInstanceTransform.SetPositionAndRotation(localPos, Quaternion.Euler(localAngles));
                    displayObjectInstanceTransform.localScale = localScale;
                    displayObjectInstance.SetActive(false);
                }
                return new GameObjectActivation
                {
                    gameObject = objectInstance,
                    shouldActivate = true
                };
            }

            private GameObjectActivation CreateActivationFromRendererIndex(CharacterModel characterModel)
            {
                var goActivation = new SkinDef.GameObjectActivation
                {
                    shouldActivate = shouldActivate,
                    gameObject = characterModel.baseRendererInfos[renderer].renderer.gameObject
                };
                return goActivation;
            }
        }

        /// <summary>
        /// A Wrapper for <see cref="SkinDef.MeshReplacement"/>
        /// <br>Used to replace meshes</br>
        /// </summary>
        [Serializable]
        public struct CustomMeshReplacement
        {
            [Tooltip("The Renderer index that's going to be modified")]
            public int renderer;
            [Tooltip("The new mesh to use")]
            public Mesh newMesh;

            internal SkinDef.MeshReplacement Upgrade(CharacterModel characterModel)
            {
                return new MeshReplacement
                {
                    mesh = newMesh,
                    renderer = characterModel.baseRendererInfos[renderer].renderer
                };
            }
        }

        /// <summary>
        /// A Wrapper for <see cref="SkinDef.ProjectileGhostReplacement"/>
        /// </summary>
        [Serializable]
        public struct AddressedProjectileGhostReplacement
        {
            [Tooltip("The projectile prefab that will have a new Ghost when this skin is used")]
            public AddressReferencedPrefab projectilePrefab;
            [Tooltip("The new ghost prefab for the projectile")]
            public GameObject ghostReplacement;
        
            internal SkinDef.ProjectileGhostReplacement Upgrade()
            {
                return new ProjectileGhostReplacement
                {
                    projectilePrefab = projectilePrefab,
                    projectileGhostReplacementPrefab = ghostReplacement
                };
            }
        }

        /// <summary>
        /// A Wrapper for <see cref="SkinDef.MinionSkinReplacement"/>
        /// </summary>
        [Serializable]
        public struct AddressedMinionSkinReplacement
        {
            [Tooltip("The minion which's skin will be modified")]
            public AddressReferencedPrefab minionPrefab;

            [Tooltip("The new skin for the minion")]
            public SkinDef minionSkin;

            internal MinionSkinReplacement Upgrade()
            {
                return new MinionSkinReplacement
                {
                    minionBodyPrefab = minionPrefab,
                    minionSkin = minionSkin
                };
            }
        }
        #endregion
    }
}