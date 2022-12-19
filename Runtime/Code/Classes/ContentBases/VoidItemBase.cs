using Moonstorm;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm
{
    /// <summary>
    /// <inheritdoc cref="ItemBase"/>
    /// <para>This variation of ItemBase allows you to easily create an Infectious relationship, allowing for Void Items.</para>
    /// </summary>
    public abstract class VoidItemBase : ItemBase
    {        
        /// <summary>
        /// Implement this method to specify what items this Void Item can infect.
        /// <para>Can be used to load Items via a plethora of ways, such as Addressables, AssetBundles and the ItemCatalog.</para>
        /// </summary>
        /// <returns>An IEnuemrable of items that can be infected by this VoidItems, it returns an IEnumerable because certain void items corrupt multiple items (IE: Void Bands)</returns>
        public abstract IEnumerable<ItemDef> LoadItemsToInfect();
    }
}
