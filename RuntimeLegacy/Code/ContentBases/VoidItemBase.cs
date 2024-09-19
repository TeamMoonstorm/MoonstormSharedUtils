using RoR2;
using System.Collections.Generic;

namespace Moonstorm
{
    public abstract class VoidItemBase : ItemBase
    {
        public abstract IEnumerable<ItemDef> LoadItemsToInfect();
    }
}
