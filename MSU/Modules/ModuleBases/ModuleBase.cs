using RoR2.ContentManagement;
using System.Reflection;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// An abstract class for creating a module base.
    /// </summary>
    public abstract class ModuleBase
    {

        /// <summary>
        /// Your Mod's Content Pack
        /// </summary>
        public virtual SerializableContentPack ContentPack { get; set; }

        /// <summary>
        /// Your Mod's AssetBundle
        /// </summary>
        public virtual AssetBundle AssetBundle { get; set; }

        public virtual void Init() { }
    }
}
