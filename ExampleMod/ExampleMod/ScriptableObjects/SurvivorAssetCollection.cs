using RoR2;
using UnityEngine;

namespace ExampleMod
{
    [CreateAssetMenu(fileName = "SurvivorAssetCollection", menuName = "ExampleMod/AssetCollections/SurvivorAssetCollection")]
    public class SurvivorAssetCollection : BodyAssetCollection
    {
        public SurvivorDef survivorDef;
    }
}
