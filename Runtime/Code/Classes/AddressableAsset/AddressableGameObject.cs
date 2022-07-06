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
    }
}
