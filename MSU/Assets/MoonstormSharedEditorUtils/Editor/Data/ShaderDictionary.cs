using System;
using System.Collections;
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
    public class ShaderDictionary : ThunderKitSetting
    {
        [Serializable]
        public class ShaderPair
        {
            public Shader original;
            public Shader stubbed;

            public ShaderPair(Shader original, Shader stubbed)
            {
                this.original = original;
                this.stubbed = stubbed;
            }
        }

        const string ShaderRootGUID = "e57526cd2e529264f8e9999843849112";
        const string MarkdownStylePath = "Packages/com.passivepicasso.thunderkit/Documentation/uss/markdown.uss";
        const string DocumentationStylePath = "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss";

        [InitializeOnLoadMethod]
        static void SetupSettings()
        {
            GetOrCreateSettings<ShaderDictionary>();
        }

        public override void Initialize()
        {
            base.Initialize();
            foreach(ShaderPair pair in shaderPairs)
            {
                if(pair.original != null && pair.stubbed != null)
                {
                    if(!allShaders.Contains(pair.original))
                        allShaders.Add(pair.original);
                    if(!allShaders.Contains(pair.stubbed))
                        allShaders.Add(pair.stubbed);

                    StubbedToOriginal.Add(pair.stubbed, pair.original);
                    OriginalToStubbed.Add(pair.original, pair.stubbed);
                }
            }
        }

        private SerializedObject shaderDictionarySO;

        public List<ShaderPair> shaderPairs = new List<ShaderPair>();
        public List<Shader> allShaders = new List<Shader>();
        public Dictionary<Shader, Shader> StubbedToOriginal { get; } = new Dictionary<Shader, Shader>();
        public Dictionary<Shader, Shader> OriginalToStubbed { get; } = new Dictionary<Shader, Shader>();
        
        public override void CreateSettingsUI(VisualElement rootElement)
        {
            if (shaderDictionarySO == null)
                shaderDictionarySO = new SerializedObject(this);

            if (shaderPairs.Count == 0)
                FillWithDefaultShaders();

            var shaderPair = CreateStandardField(nameof(shaderPairs));
            shaderPair.tooltip = $"The ShaderPairs that are used for the Dictionary system in MSEU's SwapShadersAndStageAssetBundles pipeline." +
                $"\n The original shader should be the YAML exported shader from AssetRipper" +
                $"\nthe stubbed shader can be either a default stubbed shader from MSEU, or a stubbed shader from the Dummy shader exporter of AssetRipper.";
            shaderPair.Q(null, "thunderkit-field-input").style.maxWidth = new StyleLength(new Length(100f, LengthUnit.Percent));

            rootElement.Add(shaderPair);

            rootElement.Bind(shaderDictionarySO);
        }

        private void FillWithDefaultShaders()
        {
            string rootPath = AssetDatabase.GUIDToAssetPath(ShaderRootGUID);
            string fullPath = Path.GetFullPath(rootPath);
            string pathWithoutFile = fullPath.Replace(Path.GetFileName(fullPath), "");
            IEnumerable<string> files = Directory.EnumerateFiles(pathWithoutFile, "*.shader", SearchOption.AllDirectories);
            shaderPairs = files.Select(path => FileUtil.GetProjectRelativePath(path.Replace("\\", "/")))
                .Select(relativePath => AssetDatabase.LoadAssetAtPath<Shader>(relativePath))
                .Select(shader => new ShaderPair(null, shader)).ToList();

            shaderDictionarySO.ApplyModifiedProperties();
        }
    }
}
