using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// See <see cref="IContentPiece"/> and <see cref="IContentPiece{T}"/> for more information regarding Content Pieces
    /// <br></br>
    /// <br>A version of <see cref="IContentPiece{T}"/> used to represent a Scene for the game.</br>
    /// <br>It's module is the <see cref="SceneModule"/></br>
    /// <para>Scenes added via this interface get different utilities to easily create a material for the Bazaar Between Time's seer, alongside two methods that are ran each time it's scene starts via the Stage monobehaviour.</para>
    /// </summary>
    public interface ISceneContentPiece : IContentPiece<SceneDef>
    {
        /// <summary>
        /// An optional MusicTrackDef used to adding your stage's main music track to your ContentPack. Leave this null if you're reusing the game's music tracks
        /// </summary>
        NullableRef<MusicTrackDef> MainTrack { get; }

        /// <summary>
        /// An optional MusicTrackDef used to adding your stage's boss music track to your ContentPack. Leave this null if you're reusing the game's music tracks
        /// </summary>
        NullableRef<MusicTrackDef> BossTrack { get; }

        /// <summary>
        /// A texture that will be used to create the Bazaar material for your SceneDef.
        /// </summary>
        Texture2D BazaarTextureBase { get; }

        /// <summary>
        /// Method called when a <see cref="Stage"/> ends and it's <see cref="Stage.sceneDef"/> is this Scene
        /// </summary>
        void OnServerStageComplete(Stage stage);

        /// <summary>
        /// Method called when a <see cref="Stage"/> begins and it's <see cref="Stage.sceneDef"/> is this Scene
        /// </summary>
        void OnServerStageBegin(Stage stage);
    }
}