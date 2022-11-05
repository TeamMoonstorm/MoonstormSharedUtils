using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Moonstorm.AddressableAssets
{
    /// <summary>
    /// <inheritdoc cref="AddressableAsset{T}"/>
    /// The type of asset this references is a <see cref="GameObject"/>, and can only be loaded via Addressables
    /// </summary>
    [Serializable]
    public class AddressableGameObject : AddressableAsset<GameObject>
    {

        /// <summary>
        /// Parameterless Constructor for <see cref="AddressableGameObject"/>
        /// </summary>
        public AddressableGameObject() { }
        /// <summary>
        /// Constructor for <see cref="AddressableGameObject"/> that sets the <see cref="GameObject"/> asset.
        /// </summary>
        /// <param name="go">The <see cref="GameObject"/> for this <see cref="AddressableGameObject"/></param>
        public AddressableGameObject(GameObject go)
        {
            asset = go;
            useDirectReference = true;
        }
        /// <summary>
        /// Constructor for <see cref="AddressableGameObject"/> that sets the address that'll load the asset
        /// </summary>
        /// <param name="address">The Address for the <see cref="GameObject"/></param>
        public AddressableGameObject(string address)
        {
            this.address = address;
            useDirectReference = false;
        }
    }
}
