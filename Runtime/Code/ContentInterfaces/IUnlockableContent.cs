using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    /// <summary>
    /// See also <see cref="IContentPiece"/> and <see cref="IContentPiece{T}"/> for more information on how ContentInterfaces work.
    /// <br>An Interface Addon that can be added to a ContentClass, during module intialization for said content, the module will get the ContentClass' <see cref="TiedUnlockables"/> and add them to your ContentPack.</br>
    /// <br>This is done to allow for implementations of "UnlockAll" configs for mods.</br>
    /// <br>See also <see cref="IContentPackModifier"/></br>
    /// </summary>
    public interface IUnlockableContent
    {
        /// <summary>
        /// Your ContentClass's UnlockableDef
        /// </summary>
        UnlockableDef[] TiedUnlockables { get; }
    }
}
