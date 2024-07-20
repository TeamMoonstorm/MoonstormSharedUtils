using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
namespace ExampleMod
{
    public abstract class ExampleModScene : ISceneContentPiece, IContentPackModifier
    {
        public SceneAssetCollection AssetCollection { get; private set; }
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);

        public abstract ExampleAssetRequest<SceneAssetCollection> AssetRequest { get; }

        public NullableRef<MusicTrackDef> MainTrack => Asset.mainTrack;
        public NullableRef<MusicTrackDef> BossTrack => Asset.bossTrack;

        public Texture2D BazaarTextureBase { get; protected set; } // ???

        public SceneDef Asset { get; protected set; }

        public virtual IEnumerator LoadContentAsync()
        {
            ExampleAssetRequest<SceneAssetCollection> request = AssetRequest;

            request.StartLoad();
            while (!request.isComplete)
                yield return null;

            AssetCollection = request.asset;

            Asset = AssetCollection.sceneDef;

        }


        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }

        public virtual void OnServerStageComplete(Stage stage)
        {
        }

        public virtual void OnServerStageBegin(Stage stage)
        {           
        }
    }
}
