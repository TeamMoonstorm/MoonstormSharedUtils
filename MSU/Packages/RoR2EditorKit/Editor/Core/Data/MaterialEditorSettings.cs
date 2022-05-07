using RoR2EditorKit.Core.Inspectors;
using System;
using System.Collections.Generic;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Settings
{
    /// <summary>
    /// The RoR2EK Material Editor Settings
    /// </summary>
    public sealed class MaterialEditorSettings : ThunderKitSetting
    {
        /// <summary>
        /// Represents a pair of string and shader
        /// </summary>
        [Serializable]
        public class ShaderStringPair
        {
            /// <summary>
            /// The shader's name, ideally this should be the File name, not the actual shader.name
            /// </summary>
            public string shaderName;
            /// <summary>
            /// The shader that belongs to this pair
            /// </summary>
            public Shader shader;

            /// <summary>
            /// The type that added this ShaderStringPair
            /// </summary>
            [HideInInspector]
            public string typeReference;
        }

        private SerializedObject materialEditorSettingsSO;

        /// <summary>
        /// Wether the material editor system is enabled or disabled
        /// </summary>
        public bool EnableMaterialEditor = true;

        /// <summary>
        /// The Shader String Pairs of the Material Editor Setting
        /// </summary>
        public List<ShaderStringPair> shaderStringPairs = new List<ShaderStringPair>();

        /// <summary>
        /// Direct access to the main settings file
        /// </summary>
        public RoR2EditorKitSettings MainSettings { get => GetOrCreateSettings<RoR2EditorKitSettings>(); }

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            if (materialEditorSettingsSO == null)
                materialEditorSettingsSO = new SerializedObject(this);

            rootElement.Add(MaterialEditorSettingsInspector.StaticInspectorGUI(materialEditorSettingsSO));

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