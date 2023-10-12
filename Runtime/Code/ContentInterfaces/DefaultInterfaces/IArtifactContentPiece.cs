using JetBrains.Annotations;
using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm
{
    public interface IArtifactContentPiece : IContentPiece<ArtifactDef>
    {

        [CanBeNull]
        ArtifactCode ArtifactCode { get; }
        void OnArtifactEnabled();
        void OnArtifactDisabled();
    }
}
