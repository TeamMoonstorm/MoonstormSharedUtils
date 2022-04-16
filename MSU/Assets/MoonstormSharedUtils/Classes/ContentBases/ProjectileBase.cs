using UnityEngine;

namespace Moonstorm
{
    public abstract class ProjectileBase : ContentBase
    {
        public abstract GameObject ProjectilePrefab { get; }

        public abstract GameObject ProjectileGhost { get; }
    }
}
