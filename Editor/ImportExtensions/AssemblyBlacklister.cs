using System.Collections.Generic;
using System.Linq;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;

namespace Moonstorm.EditorUtils.Importers
{
    public class AssemblyBlacklister : BlacklistProcessor
    {
        public override string Name => "MSU Related Assembly Blacklist";
        public override int Priority => 9_000;

        public override IEnumerable<string> Process(IEnumerable<string> blacklist)
        {
            var importConfig = ThunderKitSetting.GetOrCreateSettings<ImportConfiguration>();

            if (importConfig.ConfigurationExecutors.OfType<WWiseBlacklister>().Any(ie => ie.enabled))
            {
                blacklist = blacklist.Append($"Wwise.dll");
                blacklist = blacklist.Append($"AkSoundEngine.dll");
                blacklist = blacklist.Append($"AkWaapiClient");
            }
            return blacklist;
        }
    }
}