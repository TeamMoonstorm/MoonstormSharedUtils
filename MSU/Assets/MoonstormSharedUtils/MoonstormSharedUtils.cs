using BepInEx;
using Moonstorm.Utilities;
using R2API;
using R2API.ScriptableObjects;
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
          nameof(RecalculateStatsAPI),
          nameof(UnlockableAPI),
          nameof(DirectorAPI)
        })]
    public class MoonstormSharedUtils : BaseUnityPlugin
    {
        public const string GUID = "com.TeamMoonstorm.MoonstormSharedUtils";
        public const string MODNAME = "Moonstorm Shared Utils";
        public const string VERSION = "1.0.0";

        public static MoonstormSharedUtils Instance { get; private set; }
        public static PluginInfo PluginInfo { get; private set; }
        public static AssetBundle MSUAssetBundle { get; private set; }
        public static R2APISerializableContentPack MSUSerializableContentPack { get; private set; }
        public static string assemblyDir { get => Path.Combine(Path.GetDirectoryName(PluginInfo.Location), "assetbundles"); }

        private void Awake()
        {
            Instance = this;
            PluginInfo = Info;
            new MSULog(Logger);
            ConfigLoader.Init(Config);
            if (ConfigLoader.EnableDebugFeatures.Value)
            {
                gameObject.AddComponent<MSUDebug>();
            }
            Events.Init();
            MSUAssetBundle = AssetBundle.LoadFromFile(Path.Combine(assemblyDir, "msuassets"));
            R2API.ContentManagement.R2APIContentManager.AddPreExistingSerializableContentPack(MSUAssetBundle.LoadAsset<R2APISerializableContentPack>("ContentPack"));
        }
    }
}
