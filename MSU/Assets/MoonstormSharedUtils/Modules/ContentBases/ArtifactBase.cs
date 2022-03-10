using R2API.ScriptableObjects;
using RoR2;

namespace Moonstorm
{
    public abstract class ArtifactBase : ContentBase
    {
        public abstract ArtifactDef ArtifactDef { get; set; }

        public virtual ArtifactCode ArtifactCode { get; set; }
        public abstract void OnArtifactDisabled();
        public abstract void OnArtifactEnabled();
    }
}
