using JetBrains.Annotations;
using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    public interface IArtifactContentPiece : IContentPiece<ArtifactDef>
    {
        NullableRef<ArtifactCode> ArtifactCode { get; }
        void OnArtifactEnabled();
        void OnArtifactDisabled();
    }
}
