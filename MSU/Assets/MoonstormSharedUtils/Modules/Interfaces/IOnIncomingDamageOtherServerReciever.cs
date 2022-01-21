using RoR2;

namespace Moonstorm
{
    /// <summary>
    /// Interface for modifying the Incoming damage of a soon to be victim.
    /// <para>Should be used only in ItemBehaviors</para>
    /// </summary>
    public interface IOnIncomingDamageOtherServerReciever
    {
        /// <summary>
        /// Method for modifying the damage info before health is deducted from the victim.
        /// </summary>
        /// <param name="victimHealthComponent">The Victim's health component.</param>
        /// <param name="damageInfo">The DamageInfo you can modify.</param>
        void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo);
    }
}
