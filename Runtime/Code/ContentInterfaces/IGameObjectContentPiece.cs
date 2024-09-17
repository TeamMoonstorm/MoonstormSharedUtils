using RoR2;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// See also <see cref="IContentPiece{T}"/> and <see cref="IContentPiece"/> for more information on how ContentInterfaces work
    /// <para>Interface used to represent a ContentPiece that takes the form of a GameObject, such examples include CharacterBodies, Interactables, etc.</para>
    /// </summary>
    /// <typeparam name="T">The KEY Component that makes the GameObject a specific ContentPiece, examples include <see cref="ICharacterContentPiece"/>, which has T set to <see cref="CharacterBody"/>. 
    /// <br>
    /// This can also be an Interface, such is the case for <see cref="IInteractableContentPiece"/>, which has T set to <see cref="IInteractable"/>
    /// </br>
    /// </typeparam>
    public interface IGameObjectContentPiece<T> : IContentPiece<GameObject>
    {
        /// <summary>
        /// The key component to this GameObject.
        /// </summary>
        T component { get; }
    }
}