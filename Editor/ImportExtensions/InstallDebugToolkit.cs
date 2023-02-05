using ThunderKit.Integrations.Thunderstore;

namespace Moonstorm.EditorUtils.Importers
{
    public class InstallDebugToolkit : ThunderstorePackageInstaller
    {
        public override int Priority => Constants.Priority.InstallDebugToolkit;
        public override string ThunderstoreAddress => "https://thunderstore.io";
        public override string DependencyId => "IHarbHD-DebugToolkit";
        public override string Description => $"Installs the latest version of DebugToolkit.\r\nAdds console command for debugging mods.\n(Used by MSUDebug and commands from MSU)";
        public override string Name => $"Install DebugToolkit";
    }
}
