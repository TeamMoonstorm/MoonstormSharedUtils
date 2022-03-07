using System.Threading.Tasks;
using System.Collections.Generic;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Pipelines;
using RoR2EditorKit.Core.ManifestDatums;
using UnityEditor;
using System.Linq;
using UnityEngine.Networking;
using ThunderKit.Core.Paths;
using UnityEngine;
using SimpleJSON;
using System.IO;
using System.Text;
using System;

namespace RoR2EditorKit.Core.PipelineJobs
{
    [PipelineSupport(typeof(Pipeline)), RequiresManifestDatumType(typeof(LanguageFolderTree))]
    public class StageLanguageFiles : PipelineJob
    {
        public enum InvalidFile
        {
            InvalidExtension,
            FormatError,
            NodeError
        }

        public string LanguageArtifactPath = "<LanguageStaging>";
        public override Task Execute(Pipeline pipeline)
        {
            var validExtensions = new string[] { ".json", ".txt" };

            AssetDatabase.SaveAssets();
            var manifests = pipeline.Manifests;
            var lftIndices = new Dictionary<LanguageFolderTree, int>();
            var lfts = new List<LanguageFolderTree>();

            for(int i = 0; i < manifests.Length; i++)
            {
                foreach(var lft in manifests[i].Data.OfType<LanguageFolderTree>())
                {
                    lfts.Add(lft);
                    lftIndices.Add(lft, i);
                }
            }

            var languageFolderTrees = lfts.ToArray();
            var hasValidTreeFolders = languageFolderTrees.Any(lft => lft.languageFolders.Any(lf => !lf.languageName.IsNullOrEmptyOrWhitespace() && lf.languageFiles.Any()));
            if(!hasValidTreeFolders)
            {
                var scriptPath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
                pipeline.Log(LogLevel.Warning, $"No valid LanguageTreeFolder defined, skipping [{nameof(StageLanguageFiles)}](assetlink://{scriptPath}) PipelineJob");
                return Task.CompletedTask;
            }

            var languageArtifactPath = LanguageArtifactPath.Resolve(pipeline, this);

            Directory.CreateDirectory(languageArtifactPath);

            var allLanguageFiles = languageFolderTrees
                .SelectMany(lft => lft.languageFolders)
                .SelectMany(lf => lf.languageFiles)
                .ToArray();

            if(HasInvalidFile(allLanguageFiles, out var invalidFile, out var invalidFileReason))
            {
                var scriptPath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
                switch(invalidFileReason)
                {
                    case InvalidFile.InvalidExtension:
                        pipeline.Log(LogLevel.Warning, $"File {invalidFile} has an invalid extension, extension should be either \".txt\" or \".json\", skipping [{nameof(StageLanguageFiles)}](assetlink://{scriptPath}) PipelineJob.");
                        break;
                    case InvalidFile.FormatError:
                        pipeline.Log(LogLevel.Warning, $"File {invalidFile} has JSON related format errors, skipping [{nameof(StageLanguageFiles)}](assetlink://{scriptPath}) PipelineJob.");
                        break;
                    case InvalidFile.NodeError:
                        pipeline.Log(LogLevel.Warning, $"File {invalidFile} has an invalid JSON Node, the name for the json dictionary needs to be \"strings\", skipping [{nameof(StageLanguageFiles)}](assetlink://{scriptPath}) PipelineJob.");
                        break;
                }
                return Task.CompletedTask;
            }

            var logBuilder = new StringBuilder();

            for(pipeline.ManifestIndex = 0; pipeline.ManifestIndex < pipeline.Manifests.Length; pipeline.ManifestIndex++)
            {
                var manifest = pipeline.Manifest;
                foreach(var languageFolderTree in manifest.Data.OfType<LanguageFolderTree>())
                {
                    string rootLanguageFolderPath = Path.Combine(languageArtifactPath, languageFolderTree.rootFolderName);
                    if(!Directory.Exists(rootLanguageFolderPath))
                    {
                        Directory.CreateDirectory(rootLanguageFolderPath);
                    }

                    foreach(LanguageFolder languageFolder in languageFolderTree.languageFolders)
                    {
                        string langFolder = Path.Combine(rootLanguageFolderPath, languageFolder.languageName);
                        if(!Directory.Exists(langFolder))
                        {
                            Directory.CreateDirectory(langFolder);
                        }
                        foreach(TextAsset asset in languageFolder.languageFiles)
                        {
                            string relativeAssetPath = AssetDatabase.GetAssetPath(asset);
                            string fullAssetPath = Path.GetFullPath(relativeAssetPath);
                            string fullAssetName = Path.GetFileName(fullAssetPath);
                            string destPath = Path.Combine(langFolder, fullAssetName);
                            FileUtil.ReplaceFile(relativeAssetPath, destPath);
                        }
                    }
                }
            }
            pipeline.ManifestIndex = -1;

            return Task.CompletedTask;
        }

        private bool HasInvalidFile(TextAsset[] langFiles, out TextAsset invalidFile, out InvalidFile invalidFileReason)
        {
            for(int i = 0; i < langFiles.Length; i++)
            {
                TextAsset asset = langFiles[i];
                string fileExtension = Path.GetExtension(Path.GetFileName(AssetDatabase.GetAssetPath(asset)));
                if(MatchesExtension(fileExtension, ".txt") || MatchesExtension(fileExtension, ".json"))
                {
                    Stream stream = File.Open(AssetDatabase.GetAssetPath(asset), FileMode.Open, FileAccess.Read);
                    StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
                    JSONNode node = JSON.Parse(streamReader.ReadToEnd());

                    if(!(node != null))
                    {
                        invalidFile = asset;
                        invalidFileReason = InvalidFile.FormatError;
                        return true;
                    }
                    JSONNode node2 = node["strings"];
                    if(!(node2 != null))
                    {
                        invalidFile = asset;
                        invalidFileReason = InvalidFile.NodeError;
                        return true;
                    }
                }
                else
                {
                    invalidFile = asset;
                    invalidFileReason = InvalidFile.InvalidExtension;
                    return true;
                }
            }

            invalidFile = null;
            invalidFileReason = (InvalidFile)int.MinValue;
            return false;

             bool MatchesExtension(string fileExtension, string testExtension)
            {
                return string.Compare(fileExtension, testExtension, StringComparison.OrdinalIgnoreCase) == 0;
            }
        }
    }
}
