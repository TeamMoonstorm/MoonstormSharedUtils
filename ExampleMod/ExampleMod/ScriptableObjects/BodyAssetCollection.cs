using RoR2;
using UnityEngine;
namespace ExampleMod
{
    [CreateAssetMenu(fileName = "BodyAssetCollection", menuName = "ExampleMod/AssetCollections/BodyAssetCollection")]
    public class BodyAssetCollection : ExtendedAssetCollection
    {
        public GameObject bodyPrefab;
        public GameObject masterPrefab;
    }
}
