using R2API;
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
        /// Unhook the delegates you hooked onto in "OnArtifactEnabled".
        /// </summary>
        public abstract void OnArtifactDisabled();

        /// <summary>
        /// Hook any delegates you need here
        /// </summary>
        public abstract void OnArtifactEnabled();
    }
}
