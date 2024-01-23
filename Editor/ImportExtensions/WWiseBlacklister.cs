using ThunderKit.Core.Config;

namespace MSU.Editor.Importers
{
    public class WWiseBlacklister : OptionalExecutor
    {
        public override int Priority => Constants.Priority.WWiseBlacklister;
        public override string Description => $"Blacklists the following assemblies:" +
            $"\nWwise.dll" +
            $"\nAkSoundEngine.dll" +
            $"\nAkWaapiClient.dll" +
            $"\nUseful if your project uses the Wwise Integration";
        public override string Name => $"Wwise Blacklister";
        public override bool Execute() => true;
    }
}