using BepInEx;
using R2API.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
        public static PluginInfo PluginInfo { get; private set; }
        /// <summary>
        /// The AssetBundle for MSU
        /// </summary>
        public static AssetBundle MSUAssetBundle { get; private set; }
        private static string AssetBundleDir { get => Path.Combine(Path.GetDirectoryName(PluginInfo.Location), "assetbundles"); }

        private void Awake()
        {
            PluginInfo = Info;
            new MSULog(Logger);
            MSUAssetBundle = AssetBundle.LoadFromFile(Path.Combine(AssetBundleDir, "runtimemsuassetbundle"));
            new MSUConfig(this);

#if DEBUG
            gameObject.AddComponent<MSUDebug>();
#endif
        }
    }
}