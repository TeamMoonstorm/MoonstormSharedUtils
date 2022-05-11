using ThunderKit.Integrations.Thunderstore;

namespace Moonstorm.EditorUtils.Importers
{
    public class InstallFPTS : ThunderstorePackageInstaller
    {
        public override int Priority => Constants.Priority.InstallFPTS;
        public override string SourcePath => "Assets/ThunderKitSettings/RoR2Thunderstore.asset";
        public override string DependencyId => "RiskofThunder-FixPluginTypesSerialization";
        public override string Description => $"Installs the latest version of DebugToolkit.\r\nFix custom Serializable structs and such not properly getting deserialized by Unity.";
        public override string Name => $"Install FixPluginTypesSerialization";
    }
}