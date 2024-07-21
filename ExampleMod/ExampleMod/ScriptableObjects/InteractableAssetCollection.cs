using RoR2;
using UnityEngine;
using System.Collections.Generic;
using MSU;
namespace ExampleMod
{
    [CreateAssetMenu(fileName = "InteractableAssetCollection", menuName = "ExampleMod/AssetCollections/InteractableAssetCollection")]
    public class InteractableAssetCollection : ExtendedAssetCollection
    {
        public GameObject interactablePrefab;
        public InteractableCardProvider interactableCardProvider;
    }
}