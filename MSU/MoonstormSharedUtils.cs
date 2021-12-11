using BepInEx;
using Moonstorm.Utilities;
using R2API;
using R2API.Utils;
using RoR2.ContentManagement;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Moonstorm
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [R2APISubmoduleDependency(new string[]
        {
          nameof(ArtifactCodeAPI),
          nameof(DamageAPI),
          nameof(RecalculateStatsAPI)
        })]
    public class MoonstormSharedUtils : BaseUnityPlugin
    {
        public const string GUID = "com.TeamMoonstorm.MoonstormSharedUtils";
        public const string MODNAME = "Moonstorm Shared Utils";
        public const string VERSION = "0.6.0";

        public static MoonstormSharedUtils instance;
        public static PluginInfo pluginInfo;

        public static AssetBundle mainAssetBundle;
        public static string assemblyDir { get => Path.Combine(Path.GetDirectoryName(pluginInfo.Location), "assetbundles"); }

        private void Awake()
        {
            pluginInfo = Info;
            instance = this;
            MSULog.logger = Logger;
            ConfigLoader.Init(Config);
            if (ConfigLoader.EnableDebugFeatures.Value)
            {
                gameObject.AddComponent<MSUDebug>();
            }
            Patches.Init();
            Events.Init();
            mainAssetBundle = AssetBundle.LoadFromFile(Path.Combine(assemblyDir, "msuassets"));
            ContentPackProvider.serializedContentPack = mainAssetBundle.LoadAsset<SerializableContentPack>("ContentPack");
            ContentPackProvider.Initialize();
        }
    }

    public class ContentPackProvider : IContentPackProvider
    {
        public static SerializableContentPack serializedContentPack;
        public static ContentPack contentPack;

        public string identifier
        {
            get
            {
                //If I see this name while loading a mod I will make fun of you
                return "Moonstorm";
            }
        }

        internal static void Initialize()
        {
            contentPack = serializedContentPack.CreateContentPack();
            ContentManager.collectContentPackProviders += AddCustomContent;
        }

        private static void AddCustomContent(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new ContentPackProvider());
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
