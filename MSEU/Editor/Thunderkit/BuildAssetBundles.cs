using System.IO;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.Pipelines
{
    [PipelineSupport(typeof(Pipeline))]
    public class BuildAssetBundles : PipelineJob
    {
        [EnumFlag]
        public BuildAssetBundleOptions AssetBundleBuildOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        [PathReferenceResolver]
        public string outputFolder;

        public override void Execute(Pipeline pipeline)
        {
            AssetDatabase.SaveAssets();
            var stagingPath = outputFolder.Resolve(pipeline, this);
            if (!Directory.Exists(stagingPath)) Directory.CreateDirectory(stagingPath);

            var result = BuildPipeline.BuildAssetBundles(stagingPath, AssetBundleBuildOptions, buildTarget);
            if (!result)
            {
                throw new System.Exception("Failed to build AssetBundles");
            }
            if (BuildPipeline.isBuildingPlayer)
            {
                Debug.Log("Building");

            }
        }
    }
}
