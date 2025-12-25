using RoR2;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;

namespace MSU
{
    [Obsolete("VanillaSkinDefs are obsolete, and will be replaced by a new system using SkinDefParams.")]
    [CreateAssetMenu(fileName = "New VanillaSkinDef", menuName = "MSU/VanillaSkinDef")]
    public class VanillaSkinDef : SkinDef
    {
        [Space(10)]
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
#if DEBUG && !UNITY_EDITOR
            MSULog.Info($"Awake called for VanillaSkinDef {this}, remember to call \"Initialize\" on your VaillaSkinDefs so they load properly and bake properly!");
#endif
        }

        public IEnumerator Initialize()
        {
            yield break;
        }
        #region internal types

        [Serializable]
        public class MoonstormBaseSkin
        {
            [SerializeField] internal string _skinAddress;
            [SerializeField] internal SkinDef _skinDef;

            public SkinDef skinDef { get; private set; }

            public IEnumerator GetSkin()
            {
                yield break;
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
                return default;
            }
        }

        [Serializable]
        public class MoonstormGameObjectActivation
        {
            public bool isCustomActivation;

            public int rendererIndex;

            public bool shouldActivate;

            public GameObject gameObjectPrefab;
            public string childName;
            public Vector3 localPos;
            public Vector3 localAngles;
            public Vector3 localScale;


            public SkinDef.GameObjectActivation GetGameObjectActivation(CharacterModel model, CharacterModel displayModel)
            {
                return default;
            }
        }

        [Serializable]
        public class MoonstormMeshReplacement
        {
            public Mesh mesh;

            public int rendererIndex;

            public SkinDef.MeshReplacement GetMeshReplacement(CharacterModel model)
            {
                return default;
            }
        }

        [Serializable]
        public class MoonstormProjectileGhostReplacement
        {
            [SerializeField]
            internal string _projectilePrefabAddress;
            [SerializeField]
            internal GameObject _projectileGhostReplacement;

            public ProjectileGhostReplacement projectileGhostReplacement { get; private set; }

            public IEnumerator GetProjectileGhostReplacement()
            {
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

            public MinionSkinReplacement minionSkinReplacement { get; private set; }

            public IEnumerator GetMinionSkinReplacement()
            {
                yield break;
            }
        }
        #endregion
    }
}