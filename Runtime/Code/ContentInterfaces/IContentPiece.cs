using RoR2;
using RoR2.ContentManagement;
using System.Collections;

namespace MSU
{
    /// <summary>
    /// Base interface which all ContentInterfaces inherit from.
    /// <br>An <see cref="IContentPiece"/> is an Interface which is used to represent that a specific class is a class that adds Content to the Game</br>
    /// <para>It contains methods to check if the class is available for the game using <see cref="IsAvailable(ContentPack)"/>, which can be checked via configurations or by checking if a dependant content is added, a method to load it's assets asynchronously using <see cref="LoadContentAsync"/>, and an <see cref="Initialize"/> method.</para>
    /// <para>All ContentInterfaces that come built in with MSU have a corresponding Module, which is used for initialization and generic implementation of functionality for said content</para>
    /// <br>While this is the base, it is heavily recommended to utilize an interface that implements <see cref="IContentPiece{T}"/>.</br>
    /// <br>See also <see cref="IContentPackModifier"/></br>
    /// </summary>
    public interface IContentPiece
    {
        /// <summary>
        /// Method that's called during Module initialization that's used for loading a ContentPiece's Assets asynchronously.
        /// 
        /// <br>To allow for proper paralellization of your mod's content loading, make sure to yield return null, do not yield other coroutines, instead do the following:</br>
        /// 
        /// <code>
        ///     var enumerator = SomeOtherAsyncMethod();
        ///     while(enumerator.MoveNext())
        ///         yield return null;
        /// </code>
        /// </summary>
        IEnumerator LoadContentAsync();

        /// <summary>
        /// Method thats called during Module Initialization thats used for checking if the class is available for the current instance of the game.
        /// </summary>
        /// <param name="contentPack">Your mod's ContentPack, this is provided to check for asset dependencies.</param>
        /// <returns>True if the content is available, false otherwise.</returns>
        bool IsAvailable(ContentPack contentPack);

        /// <summary>
        /// Method that's called during Module Initialization thats used for Initializing a specific content piece, you can clone prefabs, modify components, hook, and anything you may need to make sure a content works properly.
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// See also <see cref="IContentPiece"/> for more information on how ContentInterfaces work.
    /// <para>Interface used to represent a singular Asset for a content piece. Contains all the other functionality of <see cref="IContentPiece"/></para>
    /// <br>If you need a ContentPiece to represent a GameObject, see <see cref="IGameObjectContentPiece{T}"/></br>
    /// <br>If you need to add more assets to a ContentPack (IE: a broken version of an Item for another one), see <see cref="IContentPackModifier"/></br>
    /// 
    /// <para>MSU comes built in with custom Interfaces that are used for it's specific modules, these are:</para>
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="ArtifactDef"/></term>
    ///         <description><see cref="IArtifactContentPiece"/></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="CharacterBody"/></term>
    ///         <description><see cref="ICharacterContentPiece"/>, for Survivors you can use <see cref="ISurvivorContentPiece"/> and for Monsters you can use <see cref="IMonsterContentPiece"/> instead</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="EquipmentDef"/></term>
    ///         <description><see cref="IEquipmentContentPiece"/>, for Elite Equipments, you can use <see cref="IEliteContentPiece"/> instead</description>
    ///     </item>
    ///     <item>
    ///         <term>Interactables</term>
    ///         <description><see cref="IInteractableContentPiece"/></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ItemDef"/></term>
    ///         <description><see cref="IItemContentPiece"/>, for Void Items, you can use <see cref="IVoidItemContentPiece"/> instead</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ItemTierDef"/></term>
    ///         <description><see cref="IItemTierContentPiece"/></description>
    ///     </item>
    ///     <item>
    ///         <term>Adding content to Vanilla Survivors</term>
    ///         <description>If you wish to add new content to VanillaSurvivors, utilize the interface <see cref="IVanillaSurvivorContentPiece"/>. Read it's documentation as it works a bit different than other IContentPiece interfaces</description>
    ///     </item>
    /// </list>
    /// </summary>
    /// <typeparam name="T">The type of asset that this ContentPiece represents</typeparam>
    public interface IContentPiece<T> : IContentPiece where T : UnityEngine.Object
    {
        /// <summary>
        /// The asset that's implemented by this ContentPiece
        /// </summary>
        T asset { get; }
    }
}