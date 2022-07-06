using System.Collections.Generic;

namespace Moonstorm
{
    /// <summary>
    /// <inheritdoc cref="EquipmentBase"/>
    /// <para>An EliteEquipmentBase is half-way initialized by the <see cref="EquipmentModuleBase"/>, to finalize the initialization you must also use the <see cref="EliteModuleBase"/></para>
    /// <para>An EliteEquipmentBase contains a List of <see cref="MSEliteDef"/> for storing the EliteDefs associated with the <see cref="EliteEquipmentBase"/></para>
    /// </summary>
    public abstract class EliteEquipmentBase : EquipmentBase
    {
        /// <summary>
        /// The <see cref="MSEliteDef"/> associated with this EliteEquipmentBase
        /// </summary>
        public abstract List<MSEliteDef> EliteDefs { get; }
    }
}
