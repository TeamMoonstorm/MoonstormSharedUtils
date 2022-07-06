using RoR2;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="ContentBase"/> that represents a <see cref="RoR2.BuffDef"/> for the game, the Buff is represented via the <see cref="BuffDef"/>
    /// <para>Its tied ModuleBase is the <see cref="BuffModuleBase"/></para>
    /// <para>Also see <see cref="Components.BaseBuffBodyBehavior"/></para>
    /// </summary>
    public abstract class BuffBase : ContentBase
    {
        /// <summary>
        /// The BuffDef associated with this BuffBase
        /// </summary>
        public abstract BuffDef BuffDef { get; }

        /// <summary>
        /// If supplied, this material will be applied to the body that has the buff declared in <see cref="BuffDef"/>
        /// </summary>
        public virtual Material OverlayMaterial { get; }
    }
}
