using System;
using System.Collections.Generic;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace RoR2EditorKit.Settings
{
    public class EnabledAndDisabledInspectorsSettings : ThunderKitSetting
    {
        [Serializable]
        public class InspectorSetting
        {
            public string inspectorName;

            [HideInInspector]
            public string typeReference;

            public bool isEnabled;
        }

        const string MarkdownStylePath = "Packages/com.passivepicasso.thunderkit/Documentation/uss/markdown.uss";
        const string DocumentationStylePath = "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss";

        [InitializeOnLoadMethod]
        static void SetupSettings()
        {
            GetOrCreateSettings<EnabledAndDisabledInspectorsSettings>();
        }

        private SerializedObject enabledAndDisabledInspectorSettingsSO;

        public List<InspectorSetting> EnabledInspectors = new List<InspectorSetting>();

        public RoR2EditorKitSettings MainSettings { get => GetOrCreateSettings<RoR2EditorKitSettings>(); }

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            var enabledInspectors = CreateStandardField(nameof(EnabledInspectors));
            enabledInspectors.tooltip = $"Which Inspectors that use RoR2EditorKit systems are enabled.";
            rootElement.Add(enabledInspectors);

            if (enabledAndDisabledInspectorSettingsSO == null)
                enabledAndDisabledInspectorSettingsSO = new SerializedObject(this);

            rootElement.Bind(enabledAndDisabledInspectorSettingsSO);
        }

        /// <summary>
        /// Tries to get or create the settings for an inspector
        /// </summary>
        /// <param name="type">The inspector's Type</param>
        /// <returns>The Inspector's InspectorSetting</returns>
        public InspectorSetting GetOrCreateInspectorSetting(Type type)
        {
            var setting = EnabledInspectors.Find(x => x.typeReference == type.AssemblyQualifiedName);
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
                EnabledInspectors.Add(setting);
                return setting;
            }
        }
    }
}
