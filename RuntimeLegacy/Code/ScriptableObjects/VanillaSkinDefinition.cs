using RoR2;
using RoR2.Projectile;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Moonstorm
{
    [Obsolete]
    public class VanillaSkinDefinition : SkinDef
    {
        #region Internal Types
        [Serializable]
        public class MSBaseSkin
        {
            public string skinAddress;
            public SkinDef skin;

            internal  Task<SkinDef> Upgrade()
            {
                throw new System.NotImplementedException();
            }
        }

        [Serializable]
        public class MSRendererInfo
        {
            public Material defaultMaterial;
            public ShadowCastingMode defaultShadowCastingMode;
            public bool ignoreOverlays;
            public bool hideOnDeath;
            public int rendererIndex;

            internal CharacterModel.RendererInfo Upgrade(CharacterModel model)
            {
                throw new System.NotImplementedException();
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

            internal SkinDef.GameObjectActivation Upgrade(CharacterModel model, CharacterModel displayModel)
            {
                return isCustomActivation ? CreateCustomActivation(model, displayModel) : CreateActivationFromRendererIndex(model);
            }

            private SkinDef.GameObjectActivation CreateCustomActivation(CharacterModel model, CharacterModel displayModel)
            {
                throw new System.NotImplementedException();
            }

            private SkinDef.GameObjectActivation CreateActivationFromRendererIndex(CharacterModel model)
            {
                throw new System.NotImplementedException();
            }
        }

        [Serializable]
        public class MSMeshReplacement
        {
            public Mesh mesh;

            public int rendererIndex;

            internal SkinDef.MeshReplacement Upgrade(CharacterModel model)
            {
                throw new System.NotImplementedException();
            }
        }

        [Serializable]
        public class MSProjectileGhostReplacement
        {
            public string projectilePrefabAddress;
            public GameObject projectileGhostReplacement;

            internal  Task<SkinDef.ProjectileGhostReplacement> Upgrade()
            {
                throw new System.NotImplementedException();
            }
        }

        [Serializable]
        public class MSMinionSkinReplacements
        {
            public string minionPrefabAddress;
            public SkinDef minionSkin;

            internal  Task<SkinDef.MinionSkinReplacement> Upgrade()
            {
                throw new System.NotImplementedException();
            }
        }
        #endregion

        public string bodyAddress;

        public string displayAddress;

        public MSBaseSkin[] _baseSkins;

        public MSRendererInfo[] _rendererInfos;

        public MSGameObjectActivation[] _gameObjectActivations;

        public MSMeshReplacement[] _meshReplacements;

        public MSProjectileGhostReplacement[] _projectileGhostReplacements;

        public MSMinionSkinReplacements[] _minionSkinReplacements;

        [HideInInspector]
        public int rendererAmounts;

        private new  void Awake()
        {
            throw new System.NotImplementedException();
        }

        #region Validators

        private  void GetRendererCount()
        {
            throw new System.NotImplementedException();
        }

        private  void ValidateDisplayAddress()
        {
            throw new System.NotImplementedException();
        }

        private  void ValidateBaseSkinAddresses()
        {
            throw new System.NotImplementedException();
        }

        private  void ValidateProjectileAddresses()
        {
            throw new System.NotImplementedException();
        }

        private  void ValidateMinionAddresses()
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}