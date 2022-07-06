using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="ContentBase"/> that represents a <see cref="CombatDirector.EliteTierDef"/> for the game. 
    /// The EliteTierDef is represented via the <see cref="SerializableEliteTierDef"/>
    /// <para>It's tied ModuleBase is the <see cref="EliteTierDefModuleBase"/></para>
    /// <para>More information about MSU's EliteTierDef system can be found in the documentation of <see cref="SerializableEliteTierDef"/></para>
    /// </summary>
    public abstract class EliteTierDefBase : ContentBase
    {
        /// <summary>
        /// The Serialized version of this EliteTierDef
        /// </summary>
        public abstract SerializableEliteTierDef SerializableEliteTierDef { get; }

        /// <summary>
        /// A function to determine wether the Elite can be used for the current spawn card's elite rules
        /// </summary>
        public abstract Func<SpawnCard.EliteRules, bool> IsAvailable { get; }
    }
}