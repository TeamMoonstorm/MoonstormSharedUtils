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
        /// Add an ItemBehavior to the body.
        /// <para>Use body.AddItemBehavior(stack)</para>
        /// <para>T must be a class that inherits from CharacterBody.ItemBehavior</para>
        /// <para>This class must implement all the functionality of the equipment</para>
        /// <para>The behavior can also be added on FireAction instead</para>
        /// </summary>
        /// <param name="body">The body which is getting affected by the equipment</param>
        /// <param name="stack">The amount of stacks of the equipment, this value is always 1</param>
        public virtual void AddBehavior(ref CharacterBody body, int stack)
        {
        }

        /// <summary>
        /// Used for writing the logic of firing your Equipment.
        /// </summary>
        /// <param name="slot">The EquipmentSlot of the Characterbody</param>
        /// <returns>True if succesfully fired, false otherwise</returns>
        public virtual bool FireAction(EquipmentSlot slot)
        {
            return false;
        }
    }
}
