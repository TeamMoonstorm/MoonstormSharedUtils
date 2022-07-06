using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="ContentBase"/> that represents a Projectile for the game, the projectile is represented via the <see cref="ProjectilePrefab"/>
    /// <para>Its tied module base is the <see cref="ProjectileModuleBase"/></para>
    /// </summary>
    public abstract class ProjectileBase : ContentBase
    {
        /// <summary>
        /// The Projectile Prefab that represents this ProjectileBase
        /// </summary>
        public abstract GameObject ProjectilePrefab { get; }
    }
}
