using BepInEx;
using R2API.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MSU
{
    [BepInDependency("com.rune580.riskofoptions")]
    [BepInDependency("iHarbHD.DebugToolkit", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class MSUMain : BaseUnityPlugin
    {
        public const string GUID = "com.TeamMoonstorm.MSU";
        public const string MODNAME = "Moonstorm Shared Utils";
        public const string VERSION = "2.0.0";

        private static PluginInfo PluginInfo { get; set; }
        private static AssetBundle MSUAssetBundle { get; set; }
        private static string AssemblyDir { get => Path.Combine(Path.GetDirectoryName(PluginInfo.Location), "assetbundles"); }

        private void Awake()
        {
            PluginInfo = Info;
            new MSULog(Logger);
            MSUAssetBundle = AssetBundle.LoadFromFile(Path.Combine(AssemblyDir, "msuassets"));
            R2API.ContentManagement.R2APIContentManager.AddPreExistingSerializableContentPack(MSUAssetBundle.LoadAsset<R2APISerializableContentPack>("MSUSCP"));
        }
    }
}