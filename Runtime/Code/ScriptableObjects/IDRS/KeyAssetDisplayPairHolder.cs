using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Moonstorm
{
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