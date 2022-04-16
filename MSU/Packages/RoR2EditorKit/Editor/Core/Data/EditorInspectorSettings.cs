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
    /// The RoR2EK Editor Inspector Settings
    /// </summary>
    public sealed class EditorInspectorSettings : ThunderKitSetting
    {
        /// <summary>
        /// Represents an ExtendedInspector
        /// </summary>
        [Serializable]
        public class InspectorSetting
        {
            /// <summary>
            /// The name of the inspector
            /// </summary>
            public string inspectorName;

            /// <summary>
            /// The type of the inspector
            /// </summary>
            [HideInInspector]
            public string typeReference;

            /// <summary>
            /// Wether the inspector is enabled or not
            /// </summary>
            public bool isEnabled;
        }

        private SerializedObject enabledAndDisabledInspectorSettingsSO;

        /// <summary>
        /// If true, RoR2EditorKit will notify the user when theyre not following the modding community's naming conventions
        /// </summary>
        public bool enableNamingConventions = true;

        /// <summary>
        /// The list of inspector settings
        /// </summary>
        public List<InspectorSetting> inspectorSettings = new List<InspectorSetting>();

        /// <summary>
        /// Direct access to the main settings file
        /// </summary>
        public RoR2EditorKitSettings MainSettings { get => GetOrCreateSettings<RoR2EditorKitSettings>(); }

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            if (enabledAndDisabledInspectorSettingsSO == null)
                enabledAndDisabledInspectorSettingsSO = new SerializedObject(this);

            var namingConventions = CreateStandardField(nameof(enableNamingConventions));
            namingConventions.tooltip = $"If enabled, certain inspectors will notify you that you're not following the mod community's naming conventions.";
            rootElement.Add(namingConventions);

            /*var enabledInspectors = CreateStandardField(nameof(inspectorSettings));
            enabledInspectors.tooltip = $"Which Inspectors that use RoR2EditorKit systems are enabled.";*/

            var enabledInspectors = EditorInspectorSettingsInspector.StaticInspectorGUI(enabledAndDisabledInspectorSettingsSO);
            rootElement.Add(enabledInspectors);


            rootElement.Bind(enabledAndDisabledInspectorSettingsSO);
        }

        /// <summary>
        /// Tries to get or create the settings for an inspector
        /// </summary>
        /// <param name="type">The inspector's Type</param>
        /// <returns>The Inspector's InspectorSetting</returns>
        public InspectorSetting GetOrCreateInspectorSetting(Type type)
        {
            var setting = inspectorSettings.Find(x => x.typeReference == type.AssemblyQualifiedName);
            if (setting != null)
            {
                return setting;
            }
            else
            {
                setting = new InspectorSetting
                {
                    inspectorName = type.Name,
                    typeReference = type.AssemblyQualifiedName,
                    isEnabled = true
                };
                inspectorSettings.Add(setting);
                return setting;
            }
        }
    }
}
