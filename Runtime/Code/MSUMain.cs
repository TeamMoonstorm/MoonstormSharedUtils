using BepInEx;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MSU
{
    /// <summary>
    /// MSU's main class
    /// </summary>
    [BepInDependency("com.rune580.riskofoptions")]
    [BepInDependency("iHarbHD.DebugToolkit", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class MSUMain : BaseUnityPlugin
    {
        /// <summary>
        /// The GUID for MSU
        /// </summary>
        public const string GUID = "com.TeamMoonstorm.MSU";
        /// <summary>
        /// The human readable name of MSU
        /// </summary>
        public const string MODNAME = "Moonstorm Shared Utils";
        /// <summary>
        /// The version of MSU that's being used
        /// </summary>
        public const string VERSION = "2.0.0";

        /// <summary>
        /// The plugin's PluginInfo
        /// </summary>
        public static PluginInfo pluginInfo { get; private set; }
        /// <summary>
        /// The AssetBundle for MSU
        /// </summary>
        public static AssetBundle msuAssetBundle { get; private set; }
        private static string assetBundleDir { get => Path.Combine(Path.GetDirectoryName(pluginInfo.Location), "assetbundles"); }


        private void Awake()
        {
            pluginInfo = Info;
            new MSULog(Logger);
            msuAssetBundle = AssetBundle.LoadFromFile(Path.Combine(assetBundleDir, "runtimemsuassetbundle"));
            new MSUConfig(this);

#if DEBUG
            gameObject.AddComponent<MSUDebug>();
#endif
        }
    }
}