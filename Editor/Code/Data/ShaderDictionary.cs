using RoR2EditorKit.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.Settings
{
    public sealed class ShaderDictionary : ThunderKitSetting
    {
        [Serializable]
        public class ShaderPair
        {
            public SerializableShaderWrapper original;
            public SerializableShaderWrapper stubbed;

            public ShaderPair(Shader original, Shader stubbed)
            {
                this.original = new SerializableShaderWrapper(original);
                this.stubbed = new SerializableShaderWrapper(stubbed);
            }
        }

        const string ShaderRootGUID = "e57526cd2e529264f8e9999843849112";

        [InitializeOnLoadMethod]
        static void CreateDictionaryOnDomainReload()
        {
            GetOrCreateSettings<ShaderDictionary>().ReloadDictionaries();
        }

        private SerializedObject shaderDictionarySO;

        public List<ShaderPair> shaderPairs = new List<ShaderPair>();
        public string testString;

        public static Dictionary<Shader, Shader> OrigToStubbed
        {
            get
            {
                if (_origToStubbed == null)
                {
                    _origToStubbed = GetOrCreateSettings<ShaderDictionary>().shaderPairs
                                        .Select(sp => (sp.stubbed.LoadShader(), sp.original.LoadShader()))
                                        .Where(sp => sp.Item1 && sp.Item2)
                                        .ToDictionary(k => k.Item2, v => v.Item1);
                }
                return _origToStubbed;
            }
        }
        private static Dictionary<Shader, Shader> _origToStubbed;
        public static Dictionary<Shader, Shader> StubbedToOrig
        {
            get
            {
                if (_stubbedToOrig == null)
                {
                    _stubbedToOrig = GetOrCreateSettings<ShaderDictionary>().shaderPairs
                                        .Select(sp => (sp.stubbed.LoadShader(), sp.original.LoadShader()))
                                        .Where(sp => sp.Item1 && sp.Item2)
                                        .ToDictionary(k => k.Item1, v => v.Item2);
                }
                return _stubbedToOrig;
            }
        }
        private static Dictionary<Shader, Shader> _stubbedToOrig;

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            if (shaderDictionarySO == null)
                shaderDictionarySO = new SerializedObject(this);

            rootElement.Add(CreateStandardField(nameof(testString)));

            var addDefaultStubbeds = new Button();
            addDefaultStubbeds.text = $"Add Default Stubbed Shaders";
            addDefaultStubbeds.tooltip = $"When clicked, MSU will populate the list with it's default stubbed shaders";
            addDefaultStubbeds.style.maxWidth = new StyleLength(new Length(250));
            addDefaultStubbeds.clicked += AddDefaultStubbeds;
            rootElement.Add(addDefaultStubbeds);

            var attemptToFinish = new Button();
            attemptToFinish.text = $"Attempt to find missing keys";
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
                var stubbed = pair.stubbed.LoadShader();
                var orig = pair.original.LoadShader();
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
            string pathWithoutFile = rootPath.Replace(Path.GetFileName(rootPath), "");
            IEnumerable<Shader> files = Directory.EnumerateFiles(pathWithoutFile, "*.shader", SearchOption.AllDirectories)
                .Select(file => file.Replace("\\", "/"))
                .Select(shaderPath => AssetDatabase.LoadAssetAtPath<Shader>(shaderPath));

            foreach (Shader shader in files)
            {
                var stubbeds = shaderPairs.Select(sp => sp.stubbed.LoadShader());
                if (!stubbeds.Contains(shader))
                {
                    shaderPairs.Add(new ShaderPair(null, shader));
                }
            }
        }

        private void AttemptToFinishDictionaryAutomatically()
        {
            Shader[] allYAMLShaders = RoR2EditorKit.Utilities.AssetDatabaseUtils.FindAssetsByType<Shader>()
                .Where(shader => AssetDatabase.GetAssetPath(shader).EndsWith(".asset")).ToArray();

            foreach (ShaderPair pair in shaderPairs)
            {
                var orig = pair.original.LoadShader();
                var stubbed = pair.stubbed.LoadShader();
                if (orig || !stubbed)
                    continue;

                string stubbedShaderFileName = Path.GetFileName(AssetDatabase.GetAssetPath(stubbed));
                string origShaderFileName = stubbedShaderFileName.Replace(".shader", ".asset");

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

                pair.original.SetShader(origShader);
            }
        }

        private void ReloadDictionaries()
        {
            _origToStubbed = null;
            _stubbedToOrig = null;

            _ = OrigToStubbed;
            _ = StubbedToOrig;
        }
    }
}
