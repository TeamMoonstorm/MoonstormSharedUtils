using HG;
using RoR2;
using System;
using UnityEngine;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New VanillaSkinDef", menuName = "Moonstorm/Vanilla SkinDef", order = 0)]
    public class VanillaSkinDef : SkinDef
    {
        public CustomGameObjectActivation[] customGameObjectActivations = Array.Empty<CustomGameObjectActivation>();
        public VanillaProjectileGhostReplacement[] vanillaProjectileGhostReplacements = Array.Empty<VanillaProjectileGhostReplacement>();
        public VanillaMinionSkinReplacement[] vanillaMinionSkinReplacements = Array.Empty<VanillaMinionSkinReplacement>();

        [Space]
        [Tooltip("e.g. \"Toolbot\"")]
        public string bodyResourcePathKeyword;


        public void Awake()
        {
            if (Application.IsPlaying(this))
            {
                //Finds the model
                var modelSkinController = Resources.Load<GameObject>($"prefabs/characterbodies/{bodyResourcePathKeyword}Body")?.GetComponent<ModelLocator>()?.modelTransform?.GetComponent<ModelSkinController>();
                if (!modelSkinController)
                {
                    MSULog.Error($"Error: Root Object not found for SkinDef {name} or model does not have a model skin controller!");
                    return;
                }
                CharacterModel model = modelSkinController.characterModel;
                GameObject rootObject = modelSkinController.gameObject;
                CharacterModel displayModel = Resources.Load<GameObject>($"prefabs/characterdisplays/{bodyResourcePathKeyword}Display")?.GetComponentInChildren<CharacterModel>();

                foreach (var item in customGameObjectActivations)
                    ArrayUtils.ArrayAppend(ref gameObjectActivations, item.Upgrade(model, displayModel));
                foreach (var item in vanillaProjectileGhostReplacements)
                    ArrayUtils.ArrayAppend(ref projectileGhostReplacements, item.Upgrade());
                foreach (var item in vanillaMinionSkinReplacements)
                    ArrayUtils.ArrayAppend(ref minionSkinReplacements, item.Upgrade());

                base.Awake();

                //Adds the skindefs to the models
                ArrayUtils.ArrayAppend(ref rootObject.GetComponent<ModelSkinController>().skins, this);
                if (displayModel?.GetComponent<ModelSkinController>())
                    ArrayUtils.ArrayAppend(ref displayModel.GetComponent<ModelSkinController>().skins, this);
            }
        }


        [Serializable]
        public struct CustomGameObjectActivation
        {
            public SkinDef.GameObjectActivation Upgrade(CharacterModel model, CharacterModel displayModel)
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
            public GameObject gameObjectPrefab;
            public string childName;
            public bool shouldActivate;
        }

        [Serializable]
        public struct VanillaProjectileGhostReplacement
        {
            public SkinDef.ProjectileGhostReplacement Upgrade()
            {
                GameObject projectilePrefab = Resources.Load<GameObject>(projectilePrefabPath);
                if (projectilePrefab)
                {
                    return new SkinDef.ProjectileGhostReplacement
                    {
                        projectilePrefab = projectilePrefab,
                        projectileGhostReplacementPrefab = projectileGhostReplacementPrefab
                    };
                }
                MSULog.Error($"Error: Projectile Prefab {projectilePrefabPath} not found to replace! Did you misspell something?");
                return new SkinDef.ProjectileGhostReplacement { };
            }

            public string projectilePrefabPath;
            public GameObject projectileGhostReplacementPrefab;
        }

        [Serializable]
        public struct VanillaMinionSkinReplacement
        {
            public SkinDef.MinionSkinReplacement Upgrade()
            {
                GameObject minionBodyPrefab = Resources.Load<GameObject>(minionBodyPrefabResourcePath);
                if (minionBodyPrefab)
                {
                    return new SkinDef.MinionSkinReplacement
                    {
                        minionBodyPrefab = minionBodyPrefab,
                        minionSkin = minionSkin
                    };
                }
                MSULog.Error($"Error: Minon Body Prefab {minionBodyPrefabResourcePath} not found to replace for skin {minionSkin}! Did you misspell something?");
                return new SkinDef.MinionSkinReplacement { };
            }

            public string minionBodyPrefabResourcePath;

            public SkinDef minionSkin;
        }
    }
}