using RoR2;
using UnityEngine;
namespace ExampleMod
{
    [CreateAssetMenu(fileName = "SceneAssetCollection", menuName = "ExampleMod/AssetCollections/SceneAssetCollection")]
    public class SceneAssetCollection : ExtendedAssetCollection
    {
        public SceneDef sceneDef;
    }
}
