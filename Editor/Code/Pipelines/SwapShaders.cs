using MSU.Editor.Settings;
using RoR2.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace MSU.Editor.Pipelines
{
    [PipelineSupport(typeof(Pipeline)), RequiresManifestDatumType(typeof(AssetBundleDefinitions))]
    public sealed class SwapShaders : PipelineJob
    {
        public SwapType swapType;

        private List<Material> _modifiedMaterials = new List<Material>();
        public override Task Execute(Pipeline pipeline)
        {
            _modifiedMaterials.Clear();
            AssetDatabase.SaveAssets();
            AssetDatabase.StartAssetEditing();
            try
            {
                BeginShaderSwap(pipeline, swapType == SwapType.YamlToHlsl ? ShaderDictionary.YAMLToHLSL : ShaderDictionary.HLSLToYAML);
                return Task.CompletedTask;
            }
            catch(Exception e)
            {
                pipeline.Log(LogLevel.Error, "Exception during SwapShaders job, restoring any material that may have been modified", e.ToString());
                RestoreShaders();
                return Task.CompletedTask;
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }
        }

        private void BeginShaderSwap(Pipeline pipeline, Dictionary<Shader, Shader> shaderDictionary)
        {
            var materials = GetAllMaterials(shaderDictionary.Keys.ToArray());

            if (materials.Length != 0)
            {
                var log = CommitSwap(materials, shaderDictionary);
                pipeline.Log(LogLevel.Information, $"Swapped a total of {log.Count} shaders. ({swapType})", log.ToArray());
            }
        }

        private List<string> CommitSwap(Material[] materials, Dictionary<Shader, Shader> dictionary)
        {
            var log = new List<string>();
            for(int i = 0; i < materials.Length; i++)
            {
                var material = materials[i];
                var shader = material.shader;

                if(dictionary.TryGetValue(shader, out Shader value))
                {
                    int renderQueue = material.renderQueue;
                    material.shader = value;
                    material.renderQueue = renderQueue;
                    if (swapType == SwapType.HlslToYaml && renderQueue == value.renderQueue)
                        material.renderQueue = -1;
                    log.Add($"Swapped {MarkdownUtils.GenerateAssetLink(material)}'s shadder ({MarkdownUtils.GenerateAssetLink(shader)}) with {MarkdownUtils.GenerateAssetLink(value)}.");
                    _modifiedMaterials.Add(material);
                }
                else
                {
                    log.Add($"Could not find matching shader for {MarkdownUtils.GenerateAssetLink(material)}'s shader ({MarkdownUtils.GenerateAssetLink(shader)}).");
                }
            }
            return log;
        }

        private Material[] GetAllMaterials(Shader[] validShaders)
        {
            return AssetDatabaseUtils.FindAssetsByType<Material>().Where(mat => validShaders.Contains(mat.shader)).ToArray();
        }

        private void RestoreShaders()
        {
            var dictionary = swapType == SwapType.YamlToHlsl ? ShaderDictionary.HLSLToYAML : ShaderDictionary.YAMLToHLSL;
            for(int i = 0; i < _modifiedMaterials.Count; i++)
            {
                var material = _modifiedMaterials[i];
                var shader = material.shader;

                if(dictionary.TryGetValue(shader, out Shader value))
                {
                    int renderQueue = material.renderQueue;
                    material.shader = value;
                    material.renderQueue = renderQueue;
                }
            }
        }

        public enum SwapType
        {
            HlslToYaml,
            YamlToHlsl,
        }
    }
}
