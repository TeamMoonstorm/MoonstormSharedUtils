using R2API.ScriptableObjects;
using RoR2;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="ContentBase"/> that represents an Artifact for the game, the artifact is represented via the <see cref="ArtifactDef"/>
    /// <para>It's tied ModuleBase is the <see cref="ArtifactModuleBase"/></para>
    /// <para>Contains methods to implement and remove the hooks for making your artifact work properly</para>
    /// <para>Contains implementation for using <see cref="R2API.ArtifactCodeAPI"/> via the <see cref="ArtifactCode"/></para>
    /// </summary>
    public abstract class ArtifactBase : ContentBase
    {

        /// <summary>
        /// The ArtifactDef associated with this ArtifactBase
        /// </summary>
        public abstract ArtifactDef ArtifactDef { get; }

        /// <summary>
        /// The ArtifactCode for this ArtifactDef, used with <see cref="R2API.ArtifactCodeAPI"/>
        /// </summary>
        public virtual ArtifactCode ArtifactCode { get; }

        /// <summary>
        /// When an artifact gets disabled in a run, this method gets called
        /// </summary>
        public abstract void OnArtifactDisabled();
        /// <summary>
        /// When an artifact gets enabled in a run, this method gets called
        /// </summary>
        public abstract void OnArtifactEnabled();
    }
}
