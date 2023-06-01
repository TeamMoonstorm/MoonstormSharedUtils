using BepInEx;
using RoR2;
using R2API.ScriptableObjects;
using UnityEngine;
using Path = System.IO.Path;

namespace Moonstorm
{
    /// <summary>
    /// The main class of MSU
    /// </summary>
    [BepInDependency(R2API.ArtifactCodeAPI.PluginGUID)]
    [BepInDependency(R2API.ColorsAPI.PluginGUID)]
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(R2API.DamageAPI.PluginGUID)]
    [BepInDependency(R2API.DirectorAPI.PluginGUID)]
    [BepInDependency(R2API.EliteAPI.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions")]
    [BepInDependency("iHarbHD.DebugToolkit", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class MoonstormSharedUtils : BaseUnityPlugin
    {
        public const string GUID = "com.TeamMoonstorm.MoonstormSharedUtils";
        public const string MODNAME = "Moonstorm Shared Utils";
        public const string VERSION = "1.5.2";

        /// <summary>
        /// Instance of MSU
        /// </summary>
        public static MoonstormSharedUtils Instance { get; private set; }
        /// <summary>
        /// MSU's PluginInfo
        /// </summary>
        public static PluginInfo PluginInfo { get; private set; }
        /// <summary>
        /// The main AssetBundle of MSU
        /// </summary>
        public static AssetBundle MSUAssetBundle { get; private set; }
        private static R2APISerializableContentPack MSUSerializableContentPack { get; set; }
        private static string AssemblyDir { get => Path.Combine(Path.GetDirectoryName(PluginInfo.Location), "assetbundles"); }

        private void Awake()
        {
            Instance = this;
            PluginInfo = Info;
            new MSULog(Logger);
            MSUAssetBundle = AssetBundle.LoadFromFile(Path.Combine(AssemblyDir, "msuassets"));
            R2API.ContentManagement.R2APIContentManager.AddPreExistingSerializableContentPack(MSUAssetBundle.LoadAsset<R2APISerializableContentPack>("MSUSCP"));

            new MSUConfig().Init();
            ConfigSystem.AddMod(this);
#if DEBUG
            RoR2Application.onLoad += () => gameObject.AddComponent<MSUDebug>();
#endif
        }
    }
}
