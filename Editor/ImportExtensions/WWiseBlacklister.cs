using ThunderKit.Core.Config;

namespace MSU.Editor.Importers
{
    public class WWiseBlacklister : OptionalExecutor
    {
        public override int Priority => Constants.Priority.WWiseBlacklister;
        public override string Description => $"Blacklists the following assemblies:" +
            $"\nAkSoundEngine.dll" +
            $"\nAk.Wwise.API.WAAPI.dll" +
            $"\nAK.WWise.Unity.API.dll" +
            $"\nAK.Wwise.Unity.API.WwiseTypes.dll" +
            $"\nAK.Wwise.Unity.MonoBehaviour.dll"+
            $"\nAK.Wwise.Unity.Timeline" +
            $"\nUseful if your project uses the Wwise Integration";
        public override string Name => $"Wwise Blacklister";
        public override bool Execute() => true;
    }
}