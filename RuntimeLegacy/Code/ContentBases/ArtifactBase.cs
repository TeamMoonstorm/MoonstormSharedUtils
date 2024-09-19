using R2API.ScriptableObjects;
using RoR2;

namespace Moonstorm
{
    public abstract class ArtifactBase : ContentBase
    {
        public abstract ArtifactDef ArtifactDef { get; }

        public virtual ArtifactCode ArtifactCode { get; }

        public abstract void OnArtifactDisabled();

        public abstract void OnArtifactEnabled();
    }
}
