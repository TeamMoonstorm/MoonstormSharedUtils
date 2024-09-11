using RoR2.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MSU.Editor
{
    [FilePath("ProjectSettings/MSU/ShaderDictionary.asset", FilePathAttribute.Location.ProjectFolder)]
    public class ShaderDictionary : ScriptableSingleton<ShaderDictionary>
    {
        const string ShaderRootGUID = "9baa48c4908f85f43ae0c54e90e44447";

        [SerializeField]
        private List<ShaderPair> shaderPairs = new List<ShaderPair>();

        public static Dictionary<Shader, Shader> YAMLToHLSL
        {
            get
            {
                if (_yamlToHlsl == null)
                {
                    var shaderPairs = instance.shaderPairs;
                    _yamlToHlsl = new Dictionary<Shader, Shader>();
                    foreach (var pair in shaderPairs)
                    {
                        var hlsl = pair.hlsl.shader;
                        var yaml = pair.yaml.shader;

                        if (!yaml)
                            continue;

                        if (_yamlToHlsl.ContainsKey(yaml))
                        {
                            continue;
                        }
                        _yamlToHlsl.Add(yaml, hlsl);
                    }

                }
                return _yamlToHlsl;
            }
        }
        private static Dictionary<Shader, Shader> _yamlToHlsl;
        public static Dictionary<Shader, Shader> HLSLToYAML
        {
            get
            {
                if (_hlslToYaml == null)
                {
                    var shaderPairs = instance.shaderPairs;
                    _hlslToYaml = new Dictionary<Shader, Shader>();
                    foreach (var pair in shaderPairs)
                    {
                        var hlsl = pair.hlsl.shader;
                        var yaml = pair.yaml.shader;

                        if (!hlsl)
                            continue;

                        if (_hlslToYaml.ContainsKey(hlsl))
                        {
                            continue;
                        }
                        _hlslToYaml.Add(hlsl, yaml);
                    }
                }
                return _hlslToYaml;
            }
        }
        private static Dictionary<Shader, Shader> _hlslToYaml;

        public static Dictionary<string, string> stubbedShaderNameToYamlShaderName = new Dictionary<string, string>
        {
            ["StubbedTextMeshPro/Distance Field"] = "TextMeshPro/Distance Field",
            ["StubbedDecalicious/DecaliciousDeferredDecal"] = "Decalicious/Deferred Decal",
            ["StubbedDecalicious/DecaliciousGameObjectID"] = "Hidden/Decalicious Game Object ID",
            ["StubbedDecalicious/DecaliciousUnlitDecal"] = "Decalicious/Unlit Decal"
        };

        public void DoSave() => Save(true);

        internal List<Shader> GetAllShadersFromDictionary()
        {
            List<Shader> list = new List<Shader>();
            foreach (ShaderPair pair in shaderPairs)
            {
                var stubbed = pair.hlsl.shader;
                var orig = pair.yaml.shader;
                if (stubbed != null && !list.Contains(stubbed))
                    list.Add(stubbed);
                if (orig != null && !list.Contains(orig))
                    list.Add(orig);
            }
            return list;
        }

        public void AddDefaultStubbeds()
        {
            string rootPath = AssetDatabase.GUIDToAssetPath(ShaderRootGUID);
            string directory = Path.GetDirectoryName(rootPath);
            string folderToSearch = RoR2.Editor.IOUtils.FormatPathForUnity(directory);
            string[] guids = AssetDatabase.FindAssets("t:Shader", new string[] { folderToSearch });

            Shader[] shadersFound = guids?.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Shader>).ToArray();

            foreach (Shader shader in shadersFound)
            {
                var stubbeds = shaderPairs.Select(sp => sp.hlsl.shader);
                if (!stubbeds.Contains(shader))
                {
                    shaderPairs.Add(new ShaderPair(null, shader));
                }
            }
            DoSave();
        }

        public void AttemptToFinishDictionaryAutomatically()
        {
            Shader[] allYAMLShaders = AssetDatabaseUtil.FindAssetsByType<Shader>()
                .Where(shader => AssetDatabase.GetAssetPath(shader).EndsWith(".asset")).ToArray();

            foreach (ShaderPair pair in shaderPairs)
            {
                var orig = pair.yaml.shader;
                var stubbed = pair.hlsl.shader;
                if (orig || !stubbed)
                    continue;

                if (stubbedShaderNameToYamlShaderName.TryGetValue(stubbed.name, out var yamlName))
                {
                    pair.yaml.shader = Shader.Find(yamlName);
                    continue;
                }

                string stubbedShaderFileName = Path.GetFileName(AssetDatabase.GetAssetPath(stubbed));
                string origShaderFileName = stubbedShaderFileName.Replace(".shader", ".asset");

                //Try to find using name
                Shader origShader = allYAMLShaders.FirstOrDefault(shader =>
                {
                    string yamlShaderFileName = Path.GetFileName(AssetDatabase.GetAssetPath(shader));
                    if (string.Compare(yamlShaderFileName, origShaderFileName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                    return false;
                });

                if (!origShader)
                    continue;

                pair.yaml.shader = origShader;
            }
            DoSave();
        }

        public void ReloadDictionaries()
        {
            _yamlToHlsl = null;
            _hlslToYaml = null;

            _ = YAMLToHLSL;
            _ = HLSLToYAML;
            DoSave();
        }

        [InitializeOnLoadMethod]
        static void CreateDictionaryOnDomainReload()
        {
            var dict = instance;
            if(dict)
            {
                dict.ReloadDictionaries();
            }
        }

        [MenuItem(MSUConstants.MSUMenuRoot + "Shaders/Shader Dictionary", priority = -1000000)]
        public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/R2EK Editor Settings");

        [Serializable]
        public class ShaderPair
        {
            public SerializableShaderWrapper yaml;
            public SerializableShaderWrapper hlsl;

            public ShaderPair(Shader original, Shader stubbed)
            {
                this.yaml = new SerializableShaderWrapper(original);
                this.hlsl = new SerializableShaderWrapper(stubbed);
            }
        }
    }
}
