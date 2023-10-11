using RoR2;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="ContentBase"/> that represents an <see cref="RoR2.EquipmentDef"/> for the game, the Equipment is represented via the <see cref="EquipmentDef"/>
    /// <para>It's tied ModuleBase is the <see cref="EquipmentModuleBase"/></para>
    /// <para>Contains a method for implementing a <see cref="CharacterBody.ItemBehavior"/> alongside a method that gets ran when the equipment is used</para>
    /// <para>You should also see <seealso cref="EliteEquipmentBase"/> for implementing Elites</para>
    /// </summary>
    public abstract class EquipmentBase : ContentBase
    {
        /// <summary>
        /// The EquipmentDef tied to this EquipmentBase
        /// </summary>
        public abstract EquipmentDef EquipmentDef { get; }

        /// <summary>
        /// Optional, supply your equipment's ItemDisplayPrefab here.
        /// </summary>
        public virtual GameObject ItemDisplayPrefab { get; }

        /// <summary>
        /// Optional, use this to add an <see cref="CharacterBody.ItemBehavior"/> that gets added when the equipment is picked up
        /// </summary>
        /// <param name="body">The body that has this equipment</param>
        /// <param name="stack">The amount of stacks, this is always set to 1</param>
        public virtual void AddBehavior(ref CharacterBody body, int stack) { }

        /// <summary>
        /// Implement your equipment's use action here.
        /// </summary>
        /// <param name="slot">The equipment slot that contains this equipment</param>
        /// <returns>True if the equipment was succesfully used, false otherwise</returns>
        public virtual bool FireAction(EquipmentSlot slot) => false;
    }
}
