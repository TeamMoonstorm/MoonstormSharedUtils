using RoR2;
using System.Collections;

namespace MSU
{
    /// <summary>
    /// See <see cref="IContentPiece"/> for more information regarding ContentPieces
    /// <br></br>
    /// <br>A version of <see cref="IContentPiece"/> used to represent new additions to vanilla survivors from the game.</br>
    /// <br>It's module is the <see cref="VanillaSurvivorModule"/></br>
    /// <para>Unlike most ContentPieces related to the different modules from MSU, the VanillaSurvivorContentPiece only has a single property which is <see cref="survivorDef"/>, this property must return the SurvivorDef that will be modified by this instance of <see cref="IVanillaSurvivorContentPiece"/> (IE: Commando)</para>
    /// <para>It also contains a <see cref="InitializeAsync"/> method, which can be utilized for initializing <see cref="VanillaSkinDef"/>s</para>
    /// </summary>
    public interface IVanillaSurvivorContentPiece : IContentPiece
    {
        /// <summary>
        /// The SurvivorDef that represents what survivor the <see cref="IVanillaSurvivorContentPiece"/> is modifying, this must be a base game survivor. not your own survivor or a modded survivor.
        /// </summary>
        SurvivorDef survivorDef { get; }

        /// <summary>
        /// A Coroutine that can be utilized for initializing asynchronously, can be used to call <see cref="VanillaSkinDef.Initialize"/>
        /// </summary>
        /// <returns></returns>
        IEnumerator InitializeAsync();
    }
}