using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Moonstorm.Loaders;
using UnityEngine;

namespace Moonstorm
{
    public class MSUTAssets : AssetsLoader<MSUTAssets>
    {
        public override AssetBundle MainAssetBundle => _bundle;

        public string AssemblyDir => Path.GetDirectoryName(MSUTestsMain.PluginInfo.Location);

        private static AssetBundle _bundle;

        private const string assetbundleFolderName = "assetbundles";
        private const string mainAssetBundleName = "msu.tests.assets";

        internal void Init()
        {
            MSUTLog.Info("AssetLoader Initialized");
            var path = Path.Combine(AssemblyDir, assetbundleFolderName, mainAssetBundleName);
            MSUTLog.Info($"Using path {path} to load the main assetbundle");
            _bundle = AssetBundle.LoadFromFile(Path.Combine(AssemblyDir, assetbundleFolderName, mainAssetBundleName));
            MSUTLog.Info($"Assetbundle {mainAssetBundleName} loaded");
        }

        internal void SwapMaterialShaders()
        {
            var mats = MainAssetBundle.LoadAllAssets<Material>().Where(mat => mat.shader.name.StartsWith("Stubbed"));
            var toLog = mats.Select(mat => $"Name: \"{mat.name}\" - Shader: \"{mat.shader.name}\"").ToArray();
            MSUTLog.Info($"Trying to swap a total of {toLog.Length} materials' stubbed shaders for real shaders.\n {string.Join("\n", toLog)}");
            SwapShadersFromMaterials(mats);
            toLog = mats.Select(mat => $"Name: \"{mat.name}\" - Shader: \"{mat.shader.name}\"").ToArray();
            MSUTLog.Info($"Finished swapping material shaders.\n {string.Join("\n", toLog)}");
        }
    }
}
