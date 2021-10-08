using RoR2;

namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing Artifacts
    /// </summary>
    public abstract class ArtifactBase
    {
        /// <summary>
        /// Your Artifact's ArtifactDef
        /// </summary>
        public abstract ArtifactDef ArtifactDef { get; set; }

        /// <summary>
        /// Initialize your Artifact
        /// </summary>
        public virtual void Initialize() { }
    }
}
