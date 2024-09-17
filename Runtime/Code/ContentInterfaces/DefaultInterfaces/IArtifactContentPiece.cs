using R2API.ScriptableObjects;
using RoR2;

namespace MSU
{
    /// <summary>
    /// See <see cref="IContentPiece"/> and <see cref="IContentPiece{T}"/> for more information regarding Content Pieces
    /// <br></br>
    /// <br>A version of <see cref="IContentPiece{T}"/> used to represent an Artifact for the game.</br>
    /// <br>It's module is the <see cref="ArtifactModule"/></br>
    /// <br>Contains functionality for dynamic hooking and unhooking when the artifact is enabled and disabled.</br>
    /// </summary>
    public interface IArtifactContentPiece : IContentPiece<ArtifactDef>
    {
        /// <summary>
        /// An <see cref="artifactCode"/> used to reach the Bulwark's Ambry stage to unlock this Artifact, Can be null
        /// </summary>
        NullableRef<ArtifactCode> artifactCode { get; }
        /// <summary>
        /// Method that's ran when this Artifact becomes Enabled, it is recommended to Hook any methods you may need for this artifact to work.
        /// </summary>
        void OnArtifactEnabled();
        /// <summary>
        /// Method that's ran when this Artifact becomes Disabled, it is recommended that you Unhook any methods you hooked in <see cref="OnArtifactEnabled"/>
        /// </summary>
        void OnArtifactDisabled();
    }
}
