using System.Collections;

namespace MSU
{
    /// <summary>
    /// See also <see cref="IContentPiece"/> and <see cref="IContentPiece{T}"/> for more information on how ContentInterfaces work.
    /// <br></br>
    /// An Interface Addon that can be added to a ContentClass, during module initialization for said content, the Module will call <see cref="InitializeAsync"/>, which can be used for asynchronously initializint the content class.
    /// <para></para>
    /// An example is a <see cref="IItemContentPiece"/> that utilizes a <see cref="ItemDisplayAddressedDictionary"/>, with an implementation of <see cref="IAsyncContentInitializer"/> you can call <see cref="ItemDisplayAddressedDictionary.AddEntries"/> and process it in an async fashion.
    /// </summary>
    public interface IAsyncContentInitializer
    {
        /// <summary>
        /// A Coroutine that can be utilized for initializing a ContentPiece asynchronously.
        /// </summary>
        /// <returns>A Coroutine, which is awaited by the Module.</returns>
        IEnumerator InitializeAsync();
    }
}