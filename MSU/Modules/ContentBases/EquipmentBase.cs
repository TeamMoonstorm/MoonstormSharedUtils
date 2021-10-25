using RoR2;

namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing an Equipment
    /// </summary>
    public abstract class EquipmentBase : ContentBase
    {
        /// <summary>
        /// Your Equipment's EquipmentDef
        /// </summary>
        public abstract EquipmentDef EquipmentDef { get; set; }

        /// <summary>
        /// Add an ItemBehavior here
        /// </summary>
        /// <param name="body"></param>
        /// <param name="stack"></param>
        public virtual void AddBehavior(ref CharacterBody body, int stack)
        {
        }

        /// <summary>
        /// Used for writing the logic of firing your Equipment.
        /// </summary>
        /// <param name="slot">The EquipmentSlot of the CharBody</param>
        /// <returns>True if succesfully fired, false otherwise</returns>
        public virtual bool FireAction(EquipmentSlot slot)
        {
            return false;
        }
    }
}
