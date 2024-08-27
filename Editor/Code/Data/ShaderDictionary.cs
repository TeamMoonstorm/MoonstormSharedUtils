using RoR2EditorKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MSU.Editor.Settings
{
    public sealed class ShaderDictionary : ThunderKitSetting
    {
        const string ShaderRootGUID = "9baa48c4908f85f43ae0c54e90e44447";

        [InitializeOnLoadMethod]
        static void CreateDictionaryOnDomainReload()
        {
            var dict = GetOrCreateSettings<ShaderDictionary>();
            if(dict)
            {
                dict.ReloadDictionaries();
            }
        }

        private SerializedObject shaderDictionarySO;

        [Tooltip("This string lets the shader dictionary to serialize properly, fill this value with whatever you want.")]
        public string serializationString;

        public List<ShaderPair> shaderPairs = new List<ShaderPair>();

        public static Dictionary<Shader, Shader> YAMLToHLSL
        {
            get
            {
                if (_yamlToHlsl == null)
                {
                    var shaderPairs = GetOrCreateSettings<ShaderDictionary>().shaderPairs;
                    _yamlToHlsl = new Dictionary<Shader, Shader>();
                    foreach (var pair in shaderPairs)
                    {
                        var hlsl = pair.hlsl.LoadShader();
                        var yaml = pair.yaml.LoadShader();

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
                    var shaderPairs = GetOrCreateSettings<ShaderDictionary>().shaderPairs;
                    _hlslToYaml = new Dictionary<Shader, Shader>();
                    foreach(var pair in shaderPairs)
                    {
                        var hlsl = pair.hlsl.LoadShader();
                        var yaml = pair.yaml.LoadShader();

                        if (!hlsl)
                            continue;

                        if(_hlslToYaml.ContainsKey(hlsl))
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

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            if (shaderDictionarySO == null)
                shaderDictionarySO = new SerializedObject(this);

            var addDefaultStubbeds = new Button();
            addDefaultStubbeds.text = $"Add Default Stubbed HLSL Shaders";
            addDefaultStubbeds.tooltip = $"When clicked, MSU will populate the list with it's default stubbed hlsl shaders";
            addDefaultStubbeds.style.maxWidth = new StyleLength(new Length(250));
            addDefaultStubbeds.clicked += AddDefaultStubbeds;
            rootElement.Add(addDefaultStubbeds);

            var attemptToFinish = new Button();
            attemptToFinish.text = $"Attempt to find missing YAML keys";
            attemptToFinish.tooltip = $"When clicked, MSEU will attempt to find the missing keys based off the value of stubbed shader." +
                $"\nThis is done by looking at the fileName of the stubbed shader, and finding a YAML shader with the same fileName (A YAML shader in this case being a shader with the .asset extension instead of .shader)" +
                $"\nthis process is not guaranteed to work constantly and or find all the keys.";
            attemptToFinish.style.maxWidth = new StyleLength(new Length(250));
            attemptToFinish.clicked += AttemptToFinishDictionaryAutomatically;
            rootElement.Add(attemptToFinish);

            var reloadDictionary = new Button();
            reloadDictionary.text = $"Reload Internal Dictionary";
            reloadDictionary.tooltip = $"Clears out the current dictionary loaded into memory and re-creates them with the current shader pair values." +
                $"\nUse this after the list is created.";
            reloadDictionary.style.maxWidth = new StyleLength(new Length(250));
            reloadDictionary.clicked += ReloadDictionaries;
            rootElement.Add(reloadDictionary);

            var dumb = CreateStandardField(nameof(this.serializationString));
            dumb.tooltip = "This string lets the shader dictionary to serialize properly, fill this value with whatever you want.";
            rootElement.Add(dumb);

            var shaderPair = CreateStandardField(nameof(shaderPairs));
            shaderPair.tooltip = $"The ShaderPairs that are used for the Dictionary system in MSEU's SwapShadersAndStageAssetBundles pipeline." +
                $"\n The original shader should be the YAML exported shader from AssetRipper" +
                $"\nthe stubbed shader can be either a default stubbed shader from MSEU, or a stubbed shader from the Dummy shader exporter of AssetRipper.";
            shaderPair.Q(null, "thunderkit-field-input").style.maxWidth = new StyleLength(new Length(100f, LengthUnit.Percent));
            rootElement.Add(shaderPair);

            rootElement.Bind(shaderDictionarySO);
        }
        internal static List<Shader> GetAllShadersFromDictionary()
        {
            List<Shader> list = new List<Shader>();
            var sd = GetOrCreateSettings<ShaderDictionary>();
            foreach (ShaderPair pair in sd.shaderPairs)
            {
                var stubbed = pair.hlsl.LoadShader();
                var orig = pair.yaml.LoadShader();
                if (stubbed != null && !list.Contains(stubbed))
                    list.Add(stubbed);
                if (orig != null && !list.Contains(orig))
                    list.Add(orig);
            }
            return list;
        }

        private void AddDefaultStubbeds()
        {
            string rootPath = AssetDatabase.GUIDToAssetPath(ShaderRootGUID);
            string directory = Path.GetDirectoryName(rootPath);
            string folderToSearch = RoR2EditorKit.IOUtils.FormatPathForUnity(directory);
            string[] guids = AssetDatabase.FindAssets("t:Shader", new string[] { folderToSearch });

            Shader[] shadersFound = guids?.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Shader>).ToArray();

            foreach (Shader shader in shadersFound)
            {
                var stubbeds = shaderPairs.Select(sp => sp.hlsl.LoadShader());
                if (!stubbeds.Contains(shader))
                {
                    shaderPairs.Add(new ShaderPair(null, shader));
                }
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private void AttemptToFinishDictionaryAutomatically()
        {
            Shader[] allYAMLShaders = AssetDatabaseUtils.FindAssetsByType<Shader>()
                .Where(shader => AssetDatabase.GetAssetPath(shader).EndsWith(".asset")).ToArray();

            foreach (ShaderPair pair in shaderPairs)
            {
                var orig = pair.yaml.LoadShader();
                var stubbed = pair.hlsl.LoadShader();
                if (orig || !stubbed)
                    continue;

                if(stubbedShaderNameToYamlShaderName.TryGetValue(stubbed.name, out var yamlName))
                {
                    pair.yaml.SetShader(Shader.Find(yamlName));
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

                pair.yaml.SetShader(origShader);
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private void ReloadDictionaries()
        {
            _yamlToHlsl = null;
            _hlslToYaml = null;

            _ = YAMLToHLSL;
            _ = HLSLToYAML;
            UnityEditor.EditorUtility.SetDirty(this);
        }

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
