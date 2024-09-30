using BepInEx;
using System.IO;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// MSU's main class
    /// </summary>
    #region R2API
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(R2API.ArtifactCodeAPI.PluginGUID)]
    [BepInDependency(R2API.ColorsAPI.PluginGUID)]
    [BepInDependency(R2API.DamageAPI.PluginGUID)]
    [BepInDependency(R2API.DirectorAPI.PluginGUID)]
    [BepInDependency(R2API.EliteAPI.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(R2API.AddressablesPlugin.PluginGUID)]
    [BepInDependency(R2API.DotAPI.PluginGUID)]
    [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID)]
    [BepInDependency(R2API.StageRegistration.PluginGUID)]
    [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID)]
    #endregion
    [BepInDependency("com.rune580.riskofoptions")]
    [BepInDependency("xyz.yekoc.Holy", BepInDependency.DependencyFlags.SoftDependency)]
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
        /// Access to MSU's instance plugin.
        /// </summary>
        public static MSUMain instance { get; private set; }

        /// <summary>
        /// The AssetBundle for MSU
        /// </summary>
        public static AssetBundle msuAssetBundle { get; private set; }
        private static string assetBundleDir { get => Path.Combine(Path.GetDirectoryName(pluginInfo.Location), "assetbundles"); }


        private void Awake()
        {
            instance = this;
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