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
    [CreateAssetMenu(fileName = "New VanillaSkinDefinition", menuName = "MSU/VanillaSkinDefinition")]
    public class VanillaSkinDefinition : SkinDef
    {
        public string bodyAddress;
        public string displayAddress;

        [Space(5)]
        public AddressReferencedSkinDef[] _baseSkins = Array.Empty<AddressReferencedSkinDef>();
        public RendererInfo[] _rendererInfos = Array.Empty<RendererInfo>();
        public CustomGameObjectActivation[] _gameObjectActivations = Array.Empty<CustomGameObjectActivation>();
        public CustomMeshReplacement[] _meshReplacements = Array.Empty<CustomMeshReplacement>();
        public AddressedProjectileGhostReplacement[] _projectileGhostReplacements = Array.Empty<AddressedProjectileGhostReplacement>();
        public AddressedMinionSkinReplacement[] _minionSkinReplacements = Array.Empty<AddressedMinionSkinReplacement>();

        [ContextMenu("test")]
        private void Test()
        {
            var bodyPrefab = Addressables.LoadAssetAsync<GameObject>(bodyAddress).WaitForCompletion();
            var characterModel = bodyPrefab.GetComponentInChildren<CharacterModel>();

            var displayPrefab = Addressables.LoadAssetAsync<GameObject>(displayAddress).WaitForCompletion();
            var displayModel = displayPrefab.GetComponentInChildren<CharacterModel>();
            foreach (var item in _gameObjectActivations)
            {
                item.Upgrade(characterModel, displayModel);
            }
        }
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

        [Serializable]
        public struct RendererInfo
        {
            public Material defaultMaterial;
            public ShadowCastingMode defaultShadowCastingMode;
            public bool ignoreOverlays;
            public bool hideOnDeath;
            public int renderer;

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

        [Serializable]
        public struct CustomGameObjectActivation
        {
            //Vanilla stuff
            public int renderer;
            public bool shouldActivate;

            public bool isCustomActivation;
            public GameObject customObject;
            public string childLocatorEntry;
            public Vector3 localPos;
            public Vector3 localAngles;
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
                    var path = AnimationUtility.CalculateTransformPath(child, characterModel.transform);
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

        [Serializable]
        public struct CustomMeshReplacement
        {
            public Mesh newMesh;
            public int renderer;

            internal SkinDef.MeshReplacement Upgrade(CharacterModel characterModel)
            {
                return new MeshReplacement
                {
                    mesh = newMesh,
                    renderer = characterModel.baseRendererInfos[renderer].renderer
                };
            }
        }

        [Serializable]
        public struct AddressedProjectileGhostReplacement
        {
            public AddressReferencedPrefab projectilePrefab;
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

        [Serializable]
        public struct AddressedMinionSkinReplacement
        {
            public AddressReferencedPrefab minionPrefab;
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
    }
}