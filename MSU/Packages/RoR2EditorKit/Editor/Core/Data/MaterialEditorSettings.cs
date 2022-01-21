using RoR2EditorKit.Core.Windows;
using System;
using System.Collections.Generic;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace RoR2EditorKit.Settings
{
    public class MaterialEditorSettings : ThunderKitSetting
    {
        [Serializable]
        public class ShaderStringPair
        {
            public string shaderName;
            public Shader shader;

            [HideInInspector]
            public string typeReference;
        }

        const string MarkdownStylePath = "Packages/com.passivepicasso.thunderkit/Documentation/uss/markdown.uss";
        const string DocumentationStylePath = "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss";


        [InitializeOnLoadMethod]
        private static void SetupSettings()
        {
            var mes = GetOrCreateSettings<MaterialEditorSettings>();
            if (mes.EnableMaterialEditor)
                mes.CheckForNullSettings();
        }

        private void CheckForNullSettings()
        {
            List<string> nullPairs = new List<string>();
            foreach (ShaderStringPair ssp in shaderStringPairs)
            {
                if (ssp.shader == null)
                    nullPairs.Add(ssp.shaderName);
            }
            if (nullPairs.Count > 0)
            {
                NullShaderPairWindow.Create(nullPairs);
            }
        }

        private SerializedObject materialEditorSettingsSO;

        public bool EnableMaterialEditor = true;

        public List<ShaderStringPair> shaderStringPairs = new List<ShaderStringPair>();


        public RoR2EditorKitSettings MainSettings { get => GetOrCreateSettings<RoR2EditorKitSettings>(); }

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            CheckForNullSettings();
            var enabled = CreateStandardField(nameof(EnableMaterialEditor));
            enabled.tooltip = $"Toggle the ROR2EK Material Editor, disabling this will disable all the MaterialEditor functionality of ROR2EK";
            rootElement.Add(enabled);

            var list = CreateStandardField(nameof(shaderStringPairs));
            list.tooltip = $"Select the shaders that matches the name displayed on the left";
            rootElement.Add(list);


            if (materialEditorSettingsSO == null)
                materialEditorSettingsSO = new SerializedObject(this);

            rootElement.Bind(materialEditorSettingsSO);
        }

        public void CreateShaderStringPairIfNull(string shaderName, Type callingType)
        {
            var pair = shaderStringPairs.Find(x => x.shaderName == shaderName);
            if (pair == null)
            {
                ShaderStringPair shaderStringPair = new ShaderStringPair
                {
                    shader = null,
                    shaderName = shaderName,
                    typeReference = callingType.FullName
                };

                shaderStringPairs.Add(shaderStringPair);
            }
            else
            {
                CheckIfTypeExists(pair);
            }
        }

        private void CheckIfTypeExists(ShaderStringPair ssp)
        {
            var type = Type.GetType(ssp.typeReference);
            if (type == null)
                shaderStringPairs.Remove(ssp);

        }
    }
}