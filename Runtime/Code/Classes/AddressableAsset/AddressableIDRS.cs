using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    /// <summary>
    /// <inheritdoc cref="AddressableAsset{T}"/>
    /// The type of asset this references is an <see cref="ItemDisplayRuleSet"/> and can only be loaded via Addressables
    /// </summary>
    [Serializable]
    public class AddressableIDRS : AddressableAsset<ItemDisplayRuleSet>
    {
    }
}
