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
    }
}
