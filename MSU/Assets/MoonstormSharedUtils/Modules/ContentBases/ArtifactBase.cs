using R2API.ScriptableObjects;
using RoR2;

namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing Artifacts
    /// </summary>
    public abstract class ArtifactBase : ContentBase
    {
        /// <summary>
        /// Your Artifact's ArtifactDef
        /// </summary>
        public abstract ArtifactDef ArtifactDef { get; set; }

        /// <summary>
        /// An ArtifactCode from R2API, used for creating a bulwark's ambry trial for your artifact, can be left null.
        /// </summary>
        public abstract ArtifactCode ArtifactCode { get; set; }
        /// <summary>
        /// Unsubscribe from any Delegates or Events you subscribed before.
        /// Ran when the artifact is disabled
        /// </summary>
        public abstract void OnArtifactDisabled();

        /// <summary>
        /// Subscribe from any Delegates or Events you need.
        /// Ran when the artifact is enabled
        /// </summary>
        public abstract void OnArtifactEnabled();
    }
}
