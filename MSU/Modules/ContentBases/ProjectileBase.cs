using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing a Projectile
    /// </summary>
    public abstract class ProjectileBase : ContentBase
    {
        /// <summary>
        /// The prefab of your projectile.
        /// </summary>
        public abstract GameObject ProjectilePrefab { get; set; }
    }
}
