using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing a Projectile
    /// </summary>
    public abstract class ProjectileBase : ContentBase
    {
        /// <summary>
        /// Your Projectile Prefab
        /// </summary>
        public abstract GameObject ProjectilePrefab { get; set; }
    }
}
