using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// See <see cref="IContentPiece"/> and <see cref="IContentPiece{T}"/> for more information regarding Content Pieces
    /// <br></br>
    /// <br>A version of <see cref="IContentPiece{T}"/> used to represent an Equipment for the game.</br>
    /// <br>It's module is the <see cref="EquipmentModule"/></br>
    /// <br>Contains methods that are called when the Equipment is used, when it's obtained and when it's lost.</br>
    /// <br>If youre looking to create an Elite, use <see cref="IEliteContentPiece"/> instead.</br>
    /// </summary>
    public interface IEquipmentContentPiece : IContentPiece<EquipmentDef>
    {
        /// <summary>
        /// The ItemDisplayPrefabs for this Equipment, can be null.
        /// </summary>
        NullableRef<List<GameObject>> itemDisplayPrefabs { get; }

        /// <summary>
        /// Method that's ran when an EquipmentSlot tries to execute this Equipment
        /// </summary>
        /// <param name="slot">The slot that's executing this Equipment</param>
        /// <returns>True if the equipment has executed succesfully, false otherwise.</returns>
        bool Execute(EquipmentSlot slot);

        /// <summary>
        /// Method that's ran when this Equipment is obtained by a CharacterBody
        /// </summary>
        /// <param name="body">The body that obtained this Equipment</param>
        void OnEquipmentObtained(CharacterBody body);

        /// <summary>
        /// Method that's ran when <paramref name="body"/> looses this Equipment
        /// </summary>
        /// <param name="body">The body that lost this Equipment</param>
        void OnEquipmentLost(CharacterBody body);
    }
}
