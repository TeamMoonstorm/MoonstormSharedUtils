using static R2API.DamageAPI;

namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing a Damage Type
    /// </summary>
    public abstract class DamageTypeBase : ContentBase
    {
        /// <summary>
        /// Your ModdedDamageType
        /// </summary>
        public abstract ModdedDamageType ModdedDamageType { get; set; }

        /// <summary>
        /// Get the DamageType asociated with this DamageTypeBase
        /// </summary>
        /// <returns>The ModdedDamageType asociated with this DamageTypeBase</returns>
        public abstract ModdedDamageType GetDamageType();

        /// <summary>
        /// Subscribe to any needed Events or Delegates here to run your DamageType's logic
        /// </summary>
        public virtual void Delegates() { }
    }
}
