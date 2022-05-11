using static R2API.DamageAPI;

namespace Moonstorm
{
    public abstract class DamageTypeBase : ContentBase
    {
        /// <summary>
        /// The ModedDamageType of this DamageTypeBase
        /// <para>Set by the DamageTypeModule.</para>
        /// </summary>
        public abstract ModdedDamageType ModdedDamageType { get; protected set; }

        internal void SetDamageType(ModdedDamageType dt) => ModdedDamageType = dt;

        /// <summary>
        /// Subscribe here to any On. or IL. hooks to make your DamageType work properly.
        /// <para>Method gets called automatically by the DamageTypeModule</para>
        /// </summary>
        public abstract void Delegates();
    }
}
