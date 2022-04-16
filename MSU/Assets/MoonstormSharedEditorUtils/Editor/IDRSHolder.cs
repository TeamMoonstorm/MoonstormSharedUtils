using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class IDRSHolder : ScriptableObject
{
    [Serializable]
    public struct IDRSStringAssetReference
    {
        public string IDRSName;
        public Object IDRS;
    }

    public List<IDRSStringAssetReference> IDRSStringAssetReferences = new List<IDRSStringAssetReference>();
}
