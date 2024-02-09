using System.Collections.Generic;

namespace Moonstorm
{
    public abstract class EliteEquipmentBase : EquipmentBase
    {
        public abstract List<MSEliteDef> EliteDefs { get; }
    }
}
