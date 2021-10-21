using BepInEx;
using Moonstorm.Utilities;
using System.IO;
using UnityEngine;


namespace Moonstorm
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class MoonstormSharedUtils : BaseUnityPlugin
    {
        public const string GUID = "com.TeamMoonstorm.MoonstormSharedUtils";
        public const string MODNAME = "Moonstorm Shared Utils";
        public const string VERSION = "0.4.0";

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
            mainAssetBundle = AssetBundle.LoadFromFile(Path.Combine(assemblyDir, "msuassets"));

        }
    }
}
