using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests;
using ThunderKit.Markdown;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;

namespace RoR2EditorKit.Settings
{
    public class RoR2EditorKitSettings : ThunderKitSetting
    {
        const string MarkdownStylePath = "Packages/com.passivepicasso.thunderkit/Documentation/uss/markdown.uss";
        const string DocumentationStylePath = "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss";

        [InitializeOnLoadMethod]
        static void SetupSettings()
        {
            GetOrCreateSettings<RoR2EditorKitSettings>();
        }

        private SerializedObject ror2EditorKitSettingsSO;

        public string TokenPrefix;

        public Manifest MainManifest;

        public EnabledAndDisabledInspectorsSettings InspectorSettings { get => GetOrCreateSettings<EnabledAndDisabledInspectorsSettings>(); }

        public MaterialEditorSettings MaterialEditorSettings { get => GetOrCreateSettings<MaterialEditorSettings>(); }

        public bool CloseWindowWhenAssetIsCreated = true;

        public override void Initialize() => TokenPrefix = "";

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            MarkdownElement markdown = null;
            if (string.IsNullOrEmpty(TokenPrefix))
            {
                markdown = new MarkdownElement
                {
                    Data = $@"**__Warning:__** No Token Prefix assigned. Assign a token prefix before continuing.",

                    MarkdownDataType = MarkdownDataType.Text
                };
                markdown.AddStyleSheetPath(MarkdownStylePath);

                markdown.AddToClassList("m4");
                markdown.RefreshContent();
                rootElement.Add(markdown);
            }

            rootElement.Add(CreateStandardField(nameof(TokenPrefix)));

            var mainManifest = CreateStandardField(nameof(MainManifest));
            mainManifest.tooltip = $"The main manifest of this unity project, used for certain windows and utilities";
            rootElement.Add(mainManifest);

            var assetCreatorCloses = CreateStandardField(nameof(CloseWindowWhenAssetIsCreated));
            assetCreatorCloses.tooltip = $"By default, when an asset creator window creates an asset, it closes, uncheck this so it doesnt closes.";
            rootElement.Add(assetCreatorCloses);

            if (ror2EditorKitSettingsSO == null)
                ror2EditorKitSettingsSO = new SerializedObject(this);

            rootElement.Bind(ror2EditorKitSettingsSO);
        }
    }
}
