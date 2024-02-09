using static R2API.DamageAPI;

namespace Moonstorm
{
    public abstract class DamageTypeBase : ContentBase
    {
        public abstract ModdedDamageType ModdedDamageType { get; protected set; }

        internal void SetDamageType(ModdedDamageType dt) => ModdedDamageType = dt;

        public abstract void Delegates();
    }
}
