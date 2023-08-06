using R2API.AddressReferencedAssets;
using System;
using UnityEngine;

namespace Moonstorm.AddressableAssets
{
    [Obsolete("Use R2API's AddressReferencedPrefab Instead")]
    [Serializable]
    public class AddressableGameObject : AddressableAsset<GameObject>
    {

        public static implicit operator AddressReferencedPrefab(AddressableGameObject bd)
        {
            if (bd.asset)
                return new AddressReferencedPrefab(bd.asset);
            else
                return new AddressReferencedPrefab(bd.address);
        }

        public AddressableGameObject() { }
        public AddressableGameObject(GameObject go)
        {
            asset = go;
            useDirectReference = true;
        }
        public AddressableGameObject(string address)
        {
            this.address = address;
            useDirectReference = false;
        }
    }
}
