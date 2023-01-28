using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Moonstorm
{
    //[CreateAssetMenu(fileName = "New Key Asset Display Pair Holder", menuName = "Moonstorm/IDRS/Key Asset Display Pair Holder", order = 0)]
    [Obsolete]
    public class KeyAssetDisplayPairHolder : ScriptableObject
    {
        [Serializable]
        public struct KeyAssetDisplayPair
        {
            public Object keyAsset;
            public List<GameObject> displayPrefabs;
        }

        public KeyAssetDisplayPair[] KeyAssetDisplayPairs;
    }
}