using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    public interface ISceneContentPiece : IContentPiece<SceneDef>
    {
        NullableRef<MusicTrackDef> MainTrack { get; }

        NullableRef<MusicTrackDef> BossTrack { get; }

        Texture2D BazaarTextureBase { get; }

        void OnServerStageComplete(Stage stage);
        void OnServerStageBegin(Stage stage);
    }
}