using RoR2.ContentManagement;

namespace MSU
{
    /// <summary>
    /// Base interface that provides ContentPieces
    /// <br>An <see cref="IContentPieceProvider"/>, as the name suggests, is an Interface used to provide <see cref="IContentPiece"/> implementing classes to MSU's Modules. Which then said modules utilize the ContentPieces provided by <see cref="GetContents"/> to initialize them.</br>
    /// <br>If you do not want to create your own implementation of IContentPieceProvider, see <see cref="ContentUtil.CreateGenericContentPieceProvider{T}(BepInEx.BaseUnityPlugin, ContentPack)"/> and <see cref="ContentUtil.CreateGameObjectGenericContentPieceProvider{T}(BepInEx.BaseUnityPlugin, ContentPack)"/></br>
    /// </summary>
    public interface IContentPieceProvider
    {
        /// <summary>
        /// The ContentPack to be populated with the Contents in <see cref="GetContents"/>
        /// </summary>
        ContentPack contentPack { get; }
        /// <summary>
        /// All the ContentPieces stored by this provider.
        /// </summary>
        /// <returns>An array of ContentPieces</returns>
        IContentPiece[] GetContents();
    }

    /// <summary>
    /// See <see cref="IContentPieceProvider"/> for more information about ContentProviders.
    /// <br>A generic version of <see cref="IContentPieceProvider"/> that can be used to specify the kind of Content to be provided.</br>
    /// </summary>
    /// <typeparam name="T">The type of content to provide, if its a component, utilize <see cref="UnityEngine.GameObject"/></typeparam>
    public interface IContentPieceProvider<T> : IContentPieceProvider where T : UnityEngine.Object
    {
        /// <inheritdoc cref="IContentPieceProvider.GetContents"/>
        new IContentPiece<T>[] GetContents();
    }
}
