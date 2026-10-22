using EntityStates.GameplayEvents;
using RoR2.ContentManagement;
using System.Collections;

namespace MSU
{
    internal sealed class MSUContent : IContentPackProvider
    {
        public string identifier => MSUMain.GUID;

        private readonly ContentPack msuContentPack = new ContentPack();

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            args.ReportProgress(0);
            ContentPack.Copy(msuContentPack, args.output);
            args.ReportProgress(1);
            yield break;
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            args.ReportProgress(0);
            msuContentPack.entityStateTypes.Add(new System.Type[]
            {
                typeof(GameplayEventState),
                typeof(GameplayEventTextController.EventTextState),
                typeof(GameplayEventTextController.FadeInState),
                typeof(GameplayEventTextController.FadeOutState),
                typeof(GameplayEventTextController.WaitState)
            });
            args.ReportProgress(1);
            yield break;
        }

        public MSUContent()
        {
            ContentManager.collectContentPackProviders += AddSelf;
        }

        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }
    }
}